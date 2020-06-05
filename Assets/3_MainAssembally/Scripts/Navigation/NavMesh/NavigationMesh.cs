using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public class NavigationMesh : MonoBehaviour
{
    public static NavigationMesh mainMesh { get; private set; }
    private static List<NavigationMesh> meshes = new List<NavigationMesh>();
    private Dictionary<(int x, int y), NavMeshCell> cells = new Dictionary<(int x, int y), NavMeshCell>();

    private const int CELL_SIZE = 10;

    public IVoxelOrientation navCellOrientation { get; private set; }

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        mainMesh = new GameObject("Main Navigation Space").AddComponent<NavigationMesh>();
    }

    private void Awake()
    {
        navCellOrientation = new TransformVoxelOrientation(transform, Vector3.zero, Quaternion.identity, Vector3.one, CELL_SIZE);
        meshes.Add(this);
    }

    private void OnDestroy()
    {
        meshes.Remove(this);
    }

    /// <summary>
    /// Get the nav mesh cell for the given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public NavMeshCell GetCell(Vector3 position, bool createNewCell = true)
    {
        return GetCell(
            RoundToInt(position.x / CELL_SIZE),
            RoundToInt(position.z / CELL_SIZE),
            createNewCell);
    }

    /// <summary>
    /// Get the cell with the specified index
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public NavMeshCell GetCell((int x, int y) point, bool createNewCell = true)
        => GetCell(point.x, point.y, createNewCell);

    /// <summary>
    /// Get the cell with the specified index
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public NavMeshCell GetCell(int x, int y, bool createNewCell = true)
    {
        if (!cells.ContainsKey((x, y)))
        {
            if (createNewCell)
            {
                return CreateNewCell(x, y);
            }
            else
            {
                return null;
            }
        }
        else if (cells[(x, y)] == null)
        {
            if (createNewCell)
            {
                return CreateNewCell(x, y);
            }
            else
            {
                return null;
            }
        }
        else
        {
            return cells[(x, y)];
        }
    }

    /// <summary>
    /// Create a new nav cell.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private NavMeshCell CreateNewCell(int x, int y)
    {
        var newCell = NavMeshCell.CreateNewCell(
            x,
            y,
            transform.localToWorldMatrix.MultiplyPoint(new Vector3(x * CELL_SIZE, 0f, y * CELL_SIZE)),
            transform.rotation,
            transform);

        cells.Add((x, y), newCell);

        return newCell;
    }

    public IEnumerable<NavMeshCell> GetCellsInBounds(Collider collider, bool createNewCell = true)
    {
        if(collider is MeshCollider meshCollider)
        {
            Bounds bounds = meshCollider.sharedMesh.bounds;
            bounds = new Bounds(collider.transform.localToWorldMatrix.MultiplyPoint(bounds.center), bounds.size);
            return GetCellsInBounds(bounds, collider.transform.rotation, createNewCell);
        }
        else if(collider is BoxCollider boxCollider)
        {
            Bounds bounds = new Bounds(collider.transform.localToWorldMatrix.MultiplyPoint(boxCollider.center), boxCollider.size);
            return GetCellsInBounds(bounds, collider.transform.rotation, createNewCell);
        }
        else if(collider is SphereCollider sphereCollider)
        {
            Bounds bounds = new Bounds(collider.transform.localToWorldMatrix.MultiplyPoint(sphereCollider.center), Vector3.one * sphereCollider.radius * 2f);
            return GetCellsInBounds(bounds, collider.transform.rotation, createNewCell);
        }
        else
        {
            return GetCellsInBounds(collider.bounds, Quaternion.identity, createNewCell);
        }
    }

    public IEnumerable<NavMeshCell> GetCellsInBounds(Bounds bounds, Quaternion rotation, bool createNewCell = true)
    {
        VoxelOrientation orientation = new VoxelOrientation(CELL_SIZE, transform.position, transform.rotation);

        Vector3 center = bounds.center;
        Vector3 halfSize = bounds.size / 2f;

        (int x, int y) min = (int.MaxValue, int.MaxValue);
        (int x, int y) max = (int.MinValue, int.MinValue);

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
                for (int z = -1; z <= 1; z++)
                {
                    Vector3 point = center + (rotation * Vector3.Scale(halfSize, new Vector3(x, y, z)));
                    DebugDraw.Cross(point, rotation, 0.1f);
                    var index = orientation.GetVoxelIndexOfPoint(point);

                    if (index.x < min.x)
                        min.x = index.x;
                    if (index.z < min.y)
                        min.y = index.z;

                    if (index.x > max.x)
                        max.x = index.x;
                    if (index.z > max.y)
                        max.y = index.z;
                }

        for(int x = min.x; x <= max.x; x++)
            for(int y = min.y; y <= max.y; y++)
            {
                yield return GetCell(x, y, createNewCell);
            }
    }
}