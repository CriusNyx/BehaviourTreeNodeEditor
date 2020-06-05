using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Stopwatch = System.Diagnostics.Stopwatch;

/// <summary>
/// Represents a cell in the navigation grid
/// </summary>
public class NavMeshCell : MonoBehaviour
{
    #region Fields
    private object graphLock = new object();
    private NavGraph graph;
    private NavCellBoundry
        forwardBoundry,
        backwardBoundry,
        leftBoundry,
        rightBoundry;

    NavigationMesh mesh => controller.mesh;

    // Flags for which things to draw while debugging
    public bool alwaysDrawThisGizmos = false;
    public static bool alwaysDrawAllGizmos = false;

    // Flags for which things to draw while debugging
    public static bool drawQuads = true;
    public static bool drawIslands = true;
    public static bool drawQuadLinks = true;
    public static bool drawIslandLinks = true;
    public static bool drawBoundries = true;

    public bool AlwaysDrawGizmos
    {
        get
        {
            return alwaysDrawThisGizmos || alwaysDrawAllGizmos;
        }
    }

    public int xPosition { get; private set; }
    public int yPosition { get; private set; }

    private static readonly Func<(int x, int y, int z), (int x, int y, int z), (int x, int y, int z)>
        Add = (x, y) => x.Zip(y, (a, b) => a + b);

    public NavCellController controller { get; private set; }

    /// <summary>
    /// The size of the cell for this game
    /// </summary>
    private const int
        X_SIZE = 10,
        Y_SIZE = 10;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        controller = gameObject.AddComponent<NavCellController>();
    }

    private void OnDrawGizmos()
    {
        Matrix4x4 gizmosMatrix = Gizmos.matrix;
        Gizmos.matrix = controller.mesh.transform.localToWorldMatrix;

        // Draw box when this cell is selected
        bool selected = false;

#if UNITY_EDITOR
        selected = Selection.Contains(gameObject);
#endif
        if (selected || AlwaysDrawGizmos)
        {
            NavGraph myGraph = null;
            NavCellBoundry left, right, forward, backward;

            lock (graphLock)
            {
                myGraph = graph;
                (left, right, forward, backward)
                    = (leftBoundry, rightBoundry, forwardBoundry, backwardBoundry);
            }

            if (myGraph != null)
            {
                foreach ((var node, var adjacentList) in myGraph.GetNodes())
                {
                    if (drawQuads)
                    {
                        node.DrawGizmos();
                    }
                    if (drawQuadLinks)
                    {
                        foreach (var adjacent in adjacentList)
                        {
                            Gizmos.DrawLine(node.position, Vector3.Lerp(node.position, adjacent.position, 0.4f));
                        }
                    }
                }

                if (drawBoundries)
                {
                    foreach (var boundry in new[] { left, right, forward, backward })
                    {
                        foreach (var quad in boundry.quads)
                        {

                            var connected = boundry.GetConnectedQuads(quad);

                            if (connected.Count() >= 1)
                            {
                                Color c = Gizmos.color;
                                Gizmos.color = Color.green;

                                quad.DrawGizmos();

                                Gizmos.color = c;
                            }
                            else
                            {
                                quad.DrawGizmos();
                            }
                        }
                    }
                }
            }

            if (drawIslands || drawIslandLinks)
            {
                foreach (var island in graph?.GetIslands() ?? new NavigationIsland[] { })
                {
                    Vector3 thisPos = island.center;

                    Gizmos.DrawWireSphere(thisPos, 0.1f);

                    foreach (var other in GetConnectedIslands(island))
                    {
                        Vector3 otherPos = other.center;
                        Vector3 pos40 = Vector3.Lerp(thisPos, otherPos, 0.4f);
                        Vector3 pos60 = Vector3.Lerp(thisPos, otherPos, 0.6f);

                        if (drawIslands)
                        {
                            Gizmos.DrawWireSphere(thisPos, 0.5f);
                        }
                        if (drawIslandLinks)
                        {
                            Gizmos.DrawLine(thisPos, pos40);
                            Gizmos.DrawLine(pos60, otherPos);
                        }
                    }
                }
            }
        }

        Gizmos.matrix = gizmosMatrix;
    }
    #endregion

    #region Drawing
    /// <summary>
    /// Draw the bounds for this cell
    /// </summary>
    /// <param name="time"></param>
    public void Draw(float time = -1)
        => Draw(Color.white, time);

    /// <summary>
    /// Draw the bounds for this cell
    /// </summary>
    /// <param name="color"></param>
    /// <param name="time"></param>
    public void Draw(Color color, float time = -1)
    {
        DebugDraw.Box(transform.position, controller.mesh.transform.rotation, new Vector3(10, 10, 10), color, time);
    }
    #endregion

    /// <summary>
    /// Get the bounds for this cell with respect to the specified orientation
    /// </summary>
    /// <param name="orientation"></param>
    /// <returns></returns>
    public IVoxelBounds3 GetBounds()
    {
        Vector3 min = transform.position - new Vector3(X_SIZE, 0f, Y_SIZE) * 0.499999f;
        Vector3 max = transform.position + new Vector3(X_SIZE, 0f, Y_SIZE) * 0.499999f;

        Vector3 center = Vector3.Lerp(min, max, 0.5f);
        Vector3 scale = max - min;

        return new BoundingBoxVoxelBounds3(center, scale, transform.rotation, BoundingBoxVoxelBounds3.IgnoreAxis.y);
    }

    #region Generation
    #region Control Flow
    /// <summary>
    /// Generate the nav mesh for this cell
    /// </summary>
    /// <param name="renderers"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public Coroutine Generate(Collider[] colliders, NavMeshGenerationSettings settings)
    {
        /* Algorithm Psudocode
         * filledFoxels = SearchPhysicsSpaceForFilledVoxels(physicsSpace)
         * pathableSpace = FindPathableSpaceFromFilledVoxels(filledVoxels)
         * voxelGraph = LinkVoxelsTogetherToGetVoxelatedNavgrid(pathableSpace, filledVoxels)
         * boundries = GetBoundrySpacesFromPathableSpace(pathableSpace, filledVoxels)
         * polyGraph = GetNavigationGraphFromPathableVoxels(pathableSpace)
         * this.navigationGraph = polyGraph
         * this.boundries = boundries
         */

        float voxelSize = settings.orientation.VoxelSize;
        float halfVoxelSize = voxelSize / 2f;

        settings = new NavMeshGenerationSettings(
            new TransformVoxelOrientation(
                controller.mesh.transform,
                Vector3.right * halfVoxelSize + Vector3.forward * halfVoxelSize,
                Quaternion.identity,
                Vector3.one,
                voxelSize),
            settings.maxClimbDistanceInVoxels,
            settings.maxFallDistanceInVoxels,
            settings.crouchHeightInVoxels,
            settings.standingHeightInVoxels);

        return StartCoroutine(RunOnMainThread(colliders, settings));
    }

    /// <summary>
    /// Run the portion of the generation algorithm that must be run on the main thread
    /// </summary>
    /// <param name="colliders"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    private IEnumerator RunOnMainThread(Collider[] colliders, NavMeshGenerationSettings settings)
    {
        var orientation = settings.orientation;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        HashSet<(int x, int y, int z)>
            filledVoxels = new HashSet<(int x, int y, int z)>(),
            emptyVoxels = new HashSet<(int x, int y, int z)>();

        foreach (var result in FindAllOccupiedVoxels(orientation, colliders, filledVoxels, emptyVoxels, stopwatch))
        {
            yield return result;
        }

        var bounds = GetBounds();

        Task.Run(() => RunOnOffThread(filledVoxels, bounds, settings));
    }

    /// <summary>
    /// Run the portion of the generation algorithm that can be generated on an additional thread
    /// </summary>
    /// <param name="filledSpaces"></param>
    /// <param name="settings"></param>
    private void RunOnOffThread(
        HashSet<(int x, int y, int z)> filledSpaces,
        IVoxelBounds3 bounds,
        NavMeshGenerationSettings settings)
    {
        try
        {
            (var standingVoxels, var crouchingVoxels)
                = FindPathableSpaces(settings, filledSpaces);

            VoxelGraph<VoxelType> graph = LinkPathableVoxels(settings, filledSpaces, standingVoxels, crouchingVoxels);

            NavCellBoundry leftBoundry, rightBoundry, backwardBoundry, forwardBoundry;

            var (outputGraph, voxelsToQuads) = GetNavGraph(graph, settings);

            var quadsToIslands = outputGraph.GenerateIslands(this);

            (leftBoundry, rightBoundry, backwardBoundry, forwardBoundry)
                = GenerateBoundries(filledSpaces, bounds, settings, voxelsToQuads, quadsToIslands, graph);


            lock (graphLock)
            {
                this.graph = outputGraph;
                this.leftBoundry = leftBoundry;
                this.rightBoundry = rightBoundry;
                this.backwardBoundry = backwardBoundry;
                this.forwardBoundry = forwardBoundry;
            }

            LinkBoundries(
                settings,
                leftBoundry,
                rightBoundry,
                backwardBoundry,
                forwardBoundry);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    #endregion

    #region Generation Tasks
    #region Find All Occupied Voxels
    /// <summary>
    /// Find all voxels that are occupied inside the cell
    /// </summary>
    /// <param name="orientation"></param>
    /// <param name="colliders"></param>
    /// <param name="set"></param>
    /// <param name="stopwatch"></param>
    /// <returns></returns>
    private IEnumerable FindAllOccupiedVoxels(
        IVoxelOrientation orientation,
        Collider[] colliders,
        HashSet<(int x, int y, int z)> filledSet,
        HashSet<(int x, int y, int z)> emptySet,
        Stopwatch stopwatch)
    {
        foreach (var renderer in colliders)
        {
            foreach (var result in FindVoxelsForCollider(orientation, renderer, filledSet, emptySet, stopwatch))
            {
                yield return result;
            }
        }
    }

    /// <summary>
    /// Find all voxels for the renderer that are occupied
    /// </summary>
    /// <param name="orientation"></param>
    /// <param name="collider"></param>
    /// <param name="filledSet"></param>
    /// <param name="emptySet"></param>
    /// <param name="stopwatch"></param>
    /// <returns></returns>
    private IEnumerable FindVoxelsForCollider(
        IVoxelOrientation orientation,
        Collider collider,
        HashSet<(int x, int y, int z)> filledSet,
        HashSet<(int x, int y, int z)> emptySet,
        Stopwatch stopwatch)
    {
        var bounds = new IntersectionVoxelBounds3(GetBounds(), new BoundingBoxVoxelBounds3(collider));

        foreach (var index in bounds.GetVoxelsInBounds(orientation))
        {
            // Frame advance if more then 1 milisecond has been spent
            if (stopwatch.ElapsedMilliseconds >= 1)
            {
                yield return null;
                stopwatch.Restart();
            }

            if (!filledSet.Contains(index) && !emptySet.Contains(index))
            {
                Vector3 pos = orientation.GetWorldPointOfVoxel(index);
                Vector3 scale = Vector3.one * orientation.VoxelSize;

                if (Physics.CheckBox(pos, scale / 2f))
                {
                    filledSet.Add(index);
                }
                else
                {
                    emptySet.Add(index);
                }
            }
        }
    }
    #endregion

    #region Find Pathable Spaces
    /// <summary>
    /// Find all spaces that can be walked on by the AI agent
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="filledSet"></param>
    /// <param name="standingSet"></param>
    /// <param name="crouchingSet"></param>
    private (HashSet<(int x, int y, int z)> standingSet, HashSet<(int x, int y, int z)> crouchingSet) FindPathableSpaces(
        NavMeshGenerationSettings settings,
        HashSet<(int x, int y, int z)> filledSet)
    {
        var standingSet = new HashSet<(int x, int y, int z)>();
        var crouchingSet = new HashSet<(int x, int y, int z)>();

        foreach (var voxel in filledSet)
        {
            bool tallEnoughToCrouch = false;
            bool tallEnoughToStand = false;
            CheckPatchingForVoxel(settings, filledSet, voxel, ref tallEnoughToCrouch, ref tallEnoughToStand);

            var groundVoxel = (voxel.x, voxel.y + 1, voxel.z);

            if (tallEnoughToStand)
            {
                standingSet.Add(groundVoxel);
            }
            else if (tallEnoughToCrouch)
            {
                crouchingSet.Add(groundVoxel);
            }
        }

        return (standingSet, crouchingSet);
    }

    /// <summary>
    /// Check if a particular voxel is pathable
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="filledSet"></param>
    /// <param name="voxel"></param>
    /// <param name="tallEnoughToCrouch"></param>
    /// <param name="tallEnoughTostand"></param>
    private void CheckPatchingForVoxel(
        NavMeshGenerationSettings settings,
        HashSet<(int x, int y, int z)> filledSet, (int x, int y, int z) voxel,
        ref bool tallEnoughToCrouch,
        ref bool tallEnoughTostand)
    {
        for (int i = 1; i <= settings.standingHeightInVoxels; i++)
        {
            var newIndex = (voxel.x, voxel.y + i, voxel.z);
            if (filledSet.Contains(newIndex))
            {
                break;
            }
            else if (i == settings.crouchHeightInVoxels)
            {
                tallEnoughToCrouch = true;
            }
            else if (i == settings.standingHeightInVoxels)
            {
                tallEnoughTostand = true;
            }
        }
    }
    #endregion

    #region Link Pathable Voxels
    /// <summary>
    /// Find all adjacent voxels that are pathable, and link them.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="filledSet"></param>
    /// <param name="standingSet"></param>
    /// <param name="crouchingSet"></param>
    /// <returns></returns>
    private VoxelGraph<VoxelType> LinkPathableVoxels(
        NavMeshGenerationSettings settings,
        HashSet<(int x, int y, int z)> filledSet,
        HashSet<(int x, int y, int z)> standingSet,
        HashSet<(int x, int y, int z)> crouchingSet)
    {
        VoxelGraph<VoxelType> output = new VoxelGraph<VoxelType>(settings.orientation);

        foreach (var voxel in standingSet)
        {
            LinkVoxelToNeighbors(settings, voxel, VoxelType.standing, filledSet, standingSet, crouchingSet, output);
        }
        foreach (var voxel in crouchingSet)
        {
            LinkVoxelToNeighbors(settings, voxel, VoxelType.crouching, filledSet, standingSet, crouchingSet, output);
        }

        return output;
    }

    /// <summary>
    /// Finds the pathable neighbors of this voxel, and links it to them
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="index"></param>
    /// <param name="voxelType"></param>
    /// <param name="filledVoxels"></param>
    /// <param name="standingSet"></param>
    /// <param name="crouchingSet"></param>
    /// <param name="graph"></param>
    private void LinkVoxelToNeighbors(
        NavMeshGenerationSettings settings,
        (int x, int y, int z) index,
        VoxelType voxelType,
        HashSet<(int x, int y, int z)> filledVoxels,
        HashSet<(int x, int y, int z)> standingSet,
        HashSet<(int x, int y, int z)> crouchingSet,
        VoxelGraph<VoxelType> graph)
    {
        (int x, int y, int z)[] adjacentOffset
            = new[]
            {
                (1, 0, 0),
                (-1, 0, 0),
                (0, 0, 1),
                (0, 0, -1),
            };

        foreach (var offset in adjacentOffset)
        {
            var adjacentIndex = Add(index, offset);
            LinkVoxelToNeighborsIfDrop(
                settings,
                index,
                voxelType,
                filledVoxels,
                standingSet,
                crouchingSet,
                graph,
                adjacentIndex);

            LinkVoxelToNeighborsIfRise(
                settings,
                index,
                voxelType,
                filledVoxels,
                standingSet,
                crouchingSet,
                graph,
                adjacentIndex);
        }
    }

    /// <summary>
    /// Link the voxel to the neighbor if the actor can drop onto the adjacent voxel
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="index"></param>
    /// <param name="voxelType"></param>
    /// <param name="filledVoxels"></param>
    /// <param name="standingSet"></param>
    /// <param name="crouchingSet"></param>
    /// <param name="graph"></param>
    /// <param name="adjacentIndex"></param>
    private void LinkVoxelToNeighborsIfDrop(
        NavMeshGenerationSettings settings,
        (int x, int y, int z) index,
        VoxelType voxelType,
        HashSet<(int x, int y, int z)> filledVoxels,
        HashSet<(int x, int y, int z)> standingSet,
        HashSet<(int x, int y, int z)> crouchingSet,
        VoxelGraph<VoxelType> graph,
        (int x, int y, int z) adjacentIndex)
    {
        // Check for drop
        if (CanLink(index, adjacentIndex, settings.crouchHeightInVoxels, filledVoxels))
        {
            // Scan down
            for (int i = 1; i <= settings.maxFallDistanceInVoxels + 1; i++)
            {
                var target = Add(adjacentIndex, (0, -i, 0));
                if (filledVoxels.Contains(target))
                {
                    var adjacentPathableVoxel = Add(target, (0, 1, 0));
                    var adjacentPathableVoxelType = VoxelType.none;
                    if (standingSet.Contains(adjacentPathableVoxel))
                    {
                        adjacentPathableVoxelType = VoxelType.standing;
                    }
                    if (crouchingSet.Contains(adjacentPathableVoxel))
                    {
                        adjacentPathableVoxelType = VoxelType.crouching;
                    }

                    if (adjacentPathableVoxelType == VoxelType.none)
                    {
                        Debug.LogError("Voxel Miscalculation Error");
                    }

                    graph.Link(index, voxelType, adjacentPathableVoxel, adjacentPathableVoxelType);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Link the voxel to the neighbor if the actor can climb to the adjacent voxel
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="index"></param>
    /// <param name="voxelType"></param>
    /// <param name="filledVoxels"></param>
    /// <param name="standingSet"></param>
    /// <param name="crouchingSet"></param>
    /// <param name="graph"></param>
    /// <param name="adjacentIndex"></param>
    private void LinkVoxelToNeighborsIfRise(
        NavMeshGenerationSettings settings,
        (int x, int y, int z) index,
        VoxelType voxelType,
        HashSet<(int x, int y, int z)> filledVoxels,
        HashSet<(int x, int y, int z)> standingSet,
        HashSet<(int x, int y, int z)> crouchingSet,
        VoxelGraph<VoxelType> graph,
        (int x, int y, int z) adjacentIndex)
    {
        // Check for rises
        for (int i = 1; i < settings.maxClimbDistanceInVoxels + 1; i++)
        {
            // Check if this voxel is open
            // If not, short circuit
            var thisMatchingVoxelIndex = Add(index, (0, i, 0));
            if (filledVoxels.Contains(thisMatchingVoxelIndex))
            {
                break;
            }

            // Get the index of the actor would move to
            var adjacentPathableVoxel = Add(adjacentIndex, (0, i, 0));

            // Check if the adjacent voxel is pathable
            bool adjacentIsStanding = standingSet.Contains(adjacentPathableVoxel);
            bool adjacentIsCrouching = crouchingSet.Contains(adjacentPathableVoxel);

            if (adjacentIsStanding || adjacentIsCrouching)
            {
                //Check to see if this voxel can link to the adjacent voxel
                if (CanLink(thisMatchingVoxelIndex, adjacentPathableVoxel, settings.crouchHeightInVoxels, filledVoxels))
                {
                    var adjacentVoxelType = adjacentIsStanding ? VoxelType.standing : VoxelType.crouching;
                    graph.Link(index, voxelType, adjacentPathableVoxel, adjacentVoxelType);
                }
            }
        }
    }

    /// <summary>
    /// Returns true if an actor can traverse between the voxels while crouching
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="crouchingHeight"></param>
    /// <param name="filledSet"></param>
    /// <returns></returns>
    private bool CanLink(
        (int x, int y, int z) a,
        (int x, int y, int z) b,
        int crouchingHeight,
        HashSet<(int x, int y, int z)> filledSet)
    {
        if (a.y != b.y)
        {
            throw new ArgumentException("a and b must be adjacent");
        }

        int xDistance = Math.Abs(a.x - b.x);
        switch (xDistance)
        {
            case 0:
                int zDistance = Math.Abs(a.z - b.z);
                if (zDistance != 1)
                    throw new ArgumentException("a and b must be adjacent");
                break;
            case 1:
                if (a.z != b.z)
                    throw new ArgumentException("a and b must be adjacent");
                break;
            default:
                throw new ArgumentException("a and b must be adjacent");
        }

        for (int i = 0; i < crouchingHeight; i++)
        {
            var index = (a.x, a.y + i, a.z);
            if (filledSet.Contains(index))
            {
                return false;
            }
        }
        for (int i = 0; i < crouchingHeight; i++)
        {
            var index = (b.x, b.y + i, b.z);
            if (filledSet.Contains(index))
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region Generate Boundries
    private (NavCellBoundry left,
        NavCellBoundry right,
        NavCellBoundry back,
        NavCellBoundry forward)
        GenerateBoundries(
      HashSet<(int x, int y, int z)> filledSpaces,
      IVoxelBounds3 bounds,
      NavMeshGenerationSettings settings,
      Dictionary<(int x, int y, int z), NavQuad> baseQuadMap,
      Dictionary<NavQuad, NavigationIsland> quadsToIslands,
      VoxelGraph<VoxelType> graph)
    {
        Func<
            Func<(int x, int y, int z), bool>,
            Vector3,
            (int x, int y, int z),
            NavCellBoundry>
                Generate
                    = (x, y, z) =>
                        {
                            return GenerateNavBoundry(filledSpaces, graph, x, y, z, baseQuadMap, quadsToIslands, settings);
                        };

        var (min, max) = bounds.GetMinAndMax(settings.orientation);

        var leftBoundry
            = Generate(
                (x) => min.x == x.x,
                Vector3.left,
                (0, 0, 1));

        var rightBoundry
            = Generate(
                (x) => max.x == x.x,
                Vector3.right,
                (0, 0, 1));

        var backwardBoundry
            = Generate(
                (x) => min.z == x.z,
                Vector3.back,
                (1, 0, 0));

        var forwardBoundry
            = Generate(
                (x) => max.z == x.z,
                Vector3.forward,
                (1, 0, 0));

        return (leftBoundry, rightBoundry, backwardBoundry, forwardBoundry);
    }

    /// <summary>
    /// Generate a nav boundry for the given filter
    /// </summary>
    /// <param name="filledSpaces"></param>
    /// <param name="graph"></param>
    /// <param name="voxelFilter"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    private NavCellBoundry GenerateNavBoundry(
        HashSet<(int x, int y, int z)> filledSpaces,
        VoxelGraph<VoxelType> graph,
        Func<(int x, int y, int z), bool> voxelFilter,
        Vector3 normal,
        (int x, int y, int z) tangent,
        Dictionary<(int x, int y, int z), NavQuad> baseQuadMap,
        Dictionary<NavQuad, NavigationIsland> quadsToIslands,
        NavMeshGenerationSettings settings)
    {
        Dictionary<(int x, int y, int z), int> heightOfBorderVoxels = new Dictionary<(int x, int y, int z), int>();

        Dictionary<NavQuad, NavigationIsland> boundryQuadsToIslands = new Dictionary<NavQuad, NavigationIsland>();

        foreach ((var index, var type) in graph.GetElements())
        {
            if (voxelFilter(index))
            {
                heightOfBorderVoxels[index]
                    = CalcHeightOfVoxel(
                        index,
                        filledSpaces,
                        Math.Max(
                            settings.maxClimbDistanceInVoxels,
                            settings.maxFallDistanceInVoxels)
                        + settings.crouchHeightInVoxels);
            }
        }

        var negativeTangent = (-tangent.x, -tangent.y, -tangent.z);

        List<NavQuad> quads = new List<NavQuad>();

        while (heightOfBorderVoxels.Count > 0)
        {
            ProcessVoxelsOnBoundry(
                normal,
                tangent,
                settings,
                heightOfBorderVoxels,
                negativeTangent,
                quads,
                baseQuadMap,
                quadsToIslands,
                boundryQuadsToIslands);
        }

        return new NavCellBoundry(quads, boundryQuadsToIslands);
    }

    private static void ProcessVoxelsOnBoundry(
        Vector3 normal,
        (int x, int y, int z) tangent,
        NavMeshGenerationSettings settings,
        Dictionary<(int x, int y, int z), int> heightOfBorderVoxels,
        (int, int, int) negativeTangent,
        List<NavQuad> quads,
        Dictionary<(int x, int y, int z), NavQuad> baseQuadMap,
        Dictionary<NavQuad, NavigationIsland> baseQuadsToIslands,
        Dictionary<NavQuad, NavigationIsland> boundryQuadsToIslands)
    {
        var pair = heightOfBorderVoxels.FirstOrDefault();
        var startIndex = pair.Key;
        var height = pair.Value;

        (var leftIndex, var rightIndex)
            = GetLeftAndRight(startIndex, height, heightOfBorderVoxels, tangent, negativeTangent);
        (int x, int y, int z) min = leftIndex.Zip(rightIndex, Math.Min);
        (int x, int y, int z) max = leftIndex.Zip(rightIndex, Math.Max);

        Vector3 minPos = settings.orientation.GetLocalPointOfVoxel(min);
        Vector3 maxPos = settings.orientation.GetLocalPointOfVoxel(max);

        float quadHeight = height * settings.orientation.VoxelSize;

        // calculate width vector
        Vector3 widthVector = maxPos - minPos;

        // abs width vector
        widthVector.x = Abs(widthVector.x);
        widthVector.y = Abs(widthVector.y);
        widthVector.z = Abs(widthVector.z);

        // extend width vector to edge of voxel
        widthVector += new Vector3(tangent.x, tangent.y, tangent.z) * settings.orientation.VoxelSize;

        // calculate height vector
        Vector3 heightVector = Vector3.up * quadHeight;

        // calculate center position
        Vector3 center = Vector3.Lerp(minPos, maxPos, 0.5f) + heightVector * 0.5f;

        // extend height vector to edge of voxels
        heightVector += Vector3.up * settings.orientation.VoxelSize;

        //correct position of center to normal
        center += normal * settings.orientation.VoxelSize * 0.5f;

        NavQuad newQuad = new NavQuad(center, heightVector + widthVector, Quaternion.identity);
        NavQuad baseQuad = baseQuadMap[startIndex];

        boundryQuadsToIslands[newQuad] = baseQuadsToIslands[baseQuad];

        // add new quad to output
        quads.Add(newQuad);
    }

    private static ((int x, int y, int z) left, (int x, int y, int z) right) GetLeftAndRight(
        (int x, int y, int z) startIndex,
        int height,
        Dictionary<(int x, int y, int z), int> heightOfBorderVoxels,
        (int x, int y, int z) tangent,
        (int, int, int) negativeTangent)
    {
        heightOfBorderVoxels.Remove(startIndex);

        // scan left
        var leftIndex = startIndex;
        var next = Add(leftIndex, tangent);
        while (heightOfBorderVoxels.TryGetValue(next, out int h) && h == height)
        {
            heightOfBorderVoxels.Remove(next);
            leftIndex = next;
            next = Add(next, tangent);
        }

        // scan right
        var rightIndex = startIndex;
        next = Add(rightIndex, negativeTangent);
        while (heightOfBorderVoxels.TryGetValue(next, out int h) && h == height)
        {
            heightOfBorderVoxels.Remove(next);
            rightIndex = next;
            next = Add(next, negativeTangent);
        }

        return (leftIndex, rightIndex);
    }

    /// <summary>
    /// Calculate the height of a voxel
    /// </summary>
    /// <param name="index"></param>
    /// <param name="filledSpaces"></param>
    /// <param name="maxHeight"></param>
    /// <returns></returns>
    private int CalcHeightOfVoxel(
        (int x, int y, int z) index,
        HashSet<(int x, int y, int z)> filledSpaces,
        int maxHeight)
    {
        for (int i = 1; i < maxHeight; i++)
        {
            var testIndex = (index.x, index.y + i, index.z);
            if (filledSpaces.Contains(testIndex))
            {
                return i;
            }
        }
        return maxHeight;
    }
    #endregion

    #region Get Nav Graph
    /// <summary>
    /// Convert the voxel graph to a nav graph
    /// </summary>
    /// <param name="voxelGraph"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    private (NavGraph graph, Dictionary<(int x, int y, int z), NavQuad> voxelsToQuadMap)
        GetNavGraph(VoxelGraph<VoxelType> voxelGraph, NavMeshGenerationSettings settings)
    {
        var surfaceVoxels =
            new HashSet<(int x, int y, int z)>(
                voxelGraph
                .GetGraphElements()
                .Select(x => x.index));

        var voxelsToQuadMap = GetQuadsFromGraph(voxelGraph, settings, surfaceVoxels);

        return (LinkNavQuads(voxelGraph, voxelsToQuadMap), voxelsToQuadMap);
    }

    /// <summary>
    /// Construct a dictionary to map voxels to nav quads
    /// </summary>
    /// <param name="voxelGraph"></param>
    /// <param name="settings"></param>
    /// <param name="surfaceVoxels"></param>
    /// <returns></returns>
    private Dictionary<(int x, int y, int z), NavQuad> GetQuadsFromGraph(
                VoxelGraph<VoxelType> voxelGraph,
                NavMeshGenerationSettings settings,
                HashSet<(int x, int y, int z)> surfaceVoxels)
    {
        Dictionary<(int x, int y, int z), NavQuad> voxelsToQuadMap
            = new Dictionary<(int x, int y, int z), NavQuad>();

        while (surfaceVoxels.Count > 0)
        {
            var first = surfaceVoxels.FirstOrDefault();
            var result
                = GraphSearch.SpiralSearch(
                    first,
                    (x, y) => GetAdjacentFromNode(x, y, voxelGraph),
                    surfaceVoxels);

            foreach (var r in result)
            {
                surfaceVoxels.Remove(r);
            }

            var quad = BuildQuad(result, settings);

            foreach (var r in result)
            {
                voxelsToQuadMap.Add(r, quad);
            }
        }

        return voxelsToQuadMap;
    }

    /// <summary>
    /// Used by spiral search to find contiguous nodes
    /// </summary>
    /// <param name="index"></param>
    /// <param name="direction"></param>
    /// <param name="graph"></param>
    /// <returns></returns>
    private (int x, int y, int z)? GetAdjacentFromNode(
        (int x, int y, int z) index,
        int direction,
        VoxelGraph<VoxelType> graph)
    {
        int targetX, targetZ;

        switch (direction)
        {
            case 0:
                targetX = index.x + 1;
                targetZ = index.z;
                break;
            case 1:
                targetX = index.x;
                targetZ = index.z + 1;
                break;
            case 2:
                targetX = index.x - 1;
                targetZ = index.z;
                break;
            case 3:
                targetX = index.x;
                targetZ = index.z - 1;
                break;
            default:
                throw new ArgumentException("The passed direction was not valid.");
        }

        var adjacentList = graph.GetAdjacent(index);

        foreach (var adjacent in adjacentList)
        {
            if (adjacent.index == (targetX, index.y, targetZ))
            {
                return adjacent.index;
            }
        }
        return null;
    }

    /// <summary>
    /// Build a nav quad from a set of voxels
    /// </summary>
    /// <param name="result"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    private static NavQuad BuildQuad(IEnumerable<(int x, int y, int z)> result, NavMeshGenerationSettings settings)
    {
        int
            minX = int.MaxValue,
            minZ = int.MaxValue,
            maxX = int.MinValue,
            maxZ = int.MinValue;

        int y = 0;

        foreach (var r in result)
        {
            minX = Math.Min(r.x, minX);
            minZ = Math.Min(r.z, minZ);
            maxX = Math.Max(r.x, maxX);
            maxZ = Math.Max(r.z, maxZ);
            y = r.y;
        }

        Vector3 halfVoxelSize = Vector3.one * settings.orientation.VoxelSize / 2f;
        halfVoxelSize.y = 0f;

        Vector3 min = settings.orientation.GetLocalPointOfVoxel(minX, y, minZ) - halfVoxelSize;
        Vector3 max = settings.orientation.GetLocalPointOfVoxel(maxX, y, maxZ) + halfVoxelSize;
        Vector3 center = Vector3.Lerp(min, max, 0.5f);

        return new NavQuad(center, max - min, Quaternion.identity);
    }

    /// <summary>
    /// Link all nav quads together
    /// </summary>
    /// <param name="voxelGraph"></param>
    /// <param name="voxelsToQuadMap"></param>
    /// <returns></returns>
    private static NavGraph LinkNavQuads(
                VoxelGraph<VoxelType> voxelGraph,
                Dictionary<(int x, int y, int z), NavQuad> voxelsToQuadMap)
    {
        var output = new NavGraph();

        foreach ((var node, var adjacentNodes) in voxelGraph.GetGraphElements())
        {
            if (voxelsToQuadMap.TryGetValue(node, out NavQuad a))
            {
                foreach (var adjacent in adjacentNodes)
                {
                    if (voxelsToQuadMap.TryGetValue(adjacent, out NavQuad b))
                    {
                        output.AddLink(a, b);
                    }
                }
            }
        }

        return output;
    }
    #endregion

    #region Link Boundries
    private void LinkBoundries(
       NavMeshGenerationSettings settings,
       NavCellBoundry leftBoundry,
       NavCellBoundry rightBoundry,
       NavCellBoundry backwardBoundry,
       NavCellBoundry forwardBoundry)
    {
        NavMeshCell
            rightCell = mesh.GetCell(xPosition + 1, yPosition, false),
            leftCell = mesh.GetCell(xPosition - 1, yPosition, false),
            forwardCell = mesh.GetCell(xPosition, yPosition + 1, false),
            backwardCell = mesh.GetCell(xPosition, yPosition - 1, false);

        leftBoundry.SetAdjacentBoundry(leftCell?.rightBoundry, settings);
        rightBoundry.SetAdjacentBoundry(rightCell?.leftBoundry, settings);
        backwardBoundry.SetAdjacentBoundry(backwardCell?.forwardBoundry, settings);
        forwardBoundry.SetAdjacentBoundry(forwardCell?.backwardBoundry, settings);
    }
    #endregion
    #endregion
    #endregion

    #region Static Methods
    /// <summary>
    /// Create a new nav cell.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static NavMeshCell CreateNewCell(int x, int y, Vector3 position, Quaternion rotation, Transform parent)
    {
        var newCell =
                  GameObjectFactory.Create<NavMeshCell>(
                      $"NavMeshCell({x}, {y})",
                      position,
                      rotation,
                      parent);

        newCell.xPosition = x;
        newCell.yPosition = y;

        return newCell;
    }

    /// <summary>
    /// Get all the cells within the specified bounds
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public static IEnumerable<NavMeshCell> GetCellsInBounds(NavigationMesh mesh, Bounds bounds)
    {
        return mesh.GetCellsInBounds(bounds, Quaternion.identity, true);
    }

    public static IEnumerable<NavMeshCell> GetCellsInBounds(NavigationMesh mesh, Collider collider)
    {
        return mesh.GetCellsInBounds(collider, true);
    }

    public static NavQuad GetQuadForPosition(NavigationMesh space, Vector3 position)
    {
        return space.GetCell(position, false)?.graph?.GetQuadForPosition(position);
    }

    public static NavigationIsland GetIslandFromPosition(NavigationMesh space, Vector3 position)
    {
        var cell = space.GetCell(position, false);
        if (cell != null)
        {
            var quad = cell?.graph?.GetQuadForPosition(position);
            if (quad != null)
            {
                return cell?.graph?.GetIslandFromQuad(quad);
            }
        }
        return null;
    }
    #endregion

    /// <summary>
    /// The type of the voxel to be linked
    /// </summary>
    private enum VoxelType
    {
        standing,
        crouching,
        none
    }

    public IEnumerable<NavigationIsland> GetIslands()
    {
        return graph.GetIslands();
    }

    public IEnumerable<NavigationIsland> GetConnectedIslands(NavigationIsland island)
    {
        foreach (var other in island.links)
        {
            yield return other;
        }

        foreach (var result in GetConnectedIslandsInBoundry(island, leftBoundry))
            yield return result;
        foreach (var result in GetConnectedIslandsInBoundry(island, rightBoundry))
            yield return result;
        foreach (var result in GetConnectedIslandsInBoundry(island, forwardBoundry))
            yield return result;
        foreach (var result in GetConnectedIslandsInBoundry(island, backwardBoundry))
            yield return result;
    }

    private IEnumerable<NavigationIsland> GetConnectedIslandsInBoundry(NavigationIsland island, NavCellBoundry boundry)
    {
        if (boundry != null)
        {
            foreach (var other in boundry.GetConnectedIslands(island) ?? new NavigationIsland[] { })
            {
                yield return other;
            }
        }
    }

    public void Invalidate()
    {

    }

    public void DrawBounds(float time = -1f)
        => DrawBounds(Color.white, time);

    public void DrawBounds(Color color, float time = -1f)
    {
        Vector3 localPos = new Vector3(xPosition * X_SIZE, 0f, yPosition * Y_SIZE);
        Vector3 localScale = new Vector3(X_SIZE, 0f, Y_SIZE);

        Vector3 ll = controller.mesh.transform.localToWorldMatrix.MultiplyPoint(localPos + Vector3.Scale(localScale, -Vector3.one));
        Vector3 ul = controller.mesh.transform.localToWorldMatrix.MultiplyPoint(localPos + Vector3.Scale(localScale, new Vector3(-1f, 0f, 1f)));
        Vector3 lr = controller.mesh.transform.localToWorldMatrix.MultiplyPoint(localPos + Vector3.Scale(localScale, new Vector3(1f, 0f, -1f)));
        Vector3 ur = controller.mesh.transform.localToWorldMatrix.MultiplyPoint(localPos + Vector3.Scale(localScale, Vector3.one));

        Debug.DrawLine(ll, ul, color, time);
        Debug.DrawLine(ll, lr, color, time);
        Debug.DrawLine(ur, ul, color, time);
        Debug.DrawLine(ur, lr, color, time);
    }
}
