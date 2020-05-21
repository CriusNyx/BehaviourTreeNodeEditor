using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavCellBoundry
{
    public readonly NavQuad[] quads;
    public NavCellBoundry connectedBoundry { get; private set; }

    private Dictionary<NavQuad, int[]> connections = null;

    private object adjacentBoundryCalculationLock = new object();

    public NavCellBoundry(IEnumerable<NavQuad> quads = null)
    {
        this.quads = quads.ToArray() ?? new NavQuad[] { };
    }

    public void SetAdjacentBoundry(NavCellBoundry adjacent, NavMeshGenerationSettings settings)
    {
        if (adjacent == null)
        {
            return;
        }
        else
        {
            lock (adjacentBoundryCalculationLock) lock (adjacent.adjacentBoundryCalculationLock)
                {
                    // set adjacent connection
                    this.connectedBoundry = adjacent;
                    adjacent.connectedBoundry = this;

                    // create maps for algorithm
                    Dictionary<NavQuad, List<int>> thisToThatMap = new Dictionary<NavQuad, List<int>>();
                    Dictionary<NavQuad, List<int>> thatToThisMap = new Dictionary<NavQuad, List<int>>();

                    // intialize maps
                    foreach (var quad in this.quads)
                    {
                        thisToThatMap.Add(quad, new List<int>());
                    }
                    foreach (var quad in adjacent.quads)
                    {
                        thatToThisMap.Add(quad, new List<int>());
                    }

                    // The multiplication by 0.999999 helps combat floating point rounding errors
                    float requiredWidth = settings.orientation.voxelSize * 0.5f;
                    float requiredHeight = settings.crouchHeightInVoxels * settings.orientation.voxelSize * 0.5f;

                    for (int i = 0; i < quads.Length; i++)
                    {
                        NavQuad thisQuad = quads[i];

                        for (int j = 0; j < adjacent.quads.Length; j++)
                        {
                            NavQuad otherQuad = adjacent.quads[j];

                            if (NavQuad.IsOverlappingVertical(thisQuad, otherQuad, requiredWidth, requiredHeight))
                            {
                                float aHeight = thisQuad.position.y - thisQuad.scale.y * 0.5f;
                                float bHeight = otherQuad.position.y - otherQuad.scale.y * 0.5f;

                                if (settings.CanClimbTo(aHeight, bHeight))
                                {
                                    thisToThatMap[thisQuad].Add(j);
                                }
                                else if (settings.CanDropTo(aHeight, bHeight))
                                {
                                    thisToThatMap[thisQuad].Add(j);
                                }

                                if (settings.CanClimbTo(bHeight, aHeight))
                                {
                                    thatToThisMap[otherQuad].Add(i);
                                }
                                else if (settings.CanDropTo(bHeight, aHeight))
                                {
                                    thatToThisMap[otherQuad].Add(i);
                                }
                            }
                        }
                    }

                    connections = thisToThatMap.ToDictionary(x => x.Key, x => x.Value.ToArray());
                    adjacent.connections = thatToThisMap.ToDictionary(x => x.Key, x => x.Value.ToArray());
                }
        }
    }

    public IEnumerable<NavQuad> GetConnectedQuads(NavQuad quad)
    {
        if(connections != null)
        {
            if(connections.TryGetValue(quad, out int[] adjacentIndicies))
            {
                foreach(var i in adjacentIndicies)
                {
                    yield return connectedBoundry.quads[i];
                }
            }
        }
    }
}