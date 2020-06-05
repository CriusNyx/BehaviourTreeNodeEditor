using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavCellBoundry
{
    public readonly NavQuad[] quads;
    public NavCellBoundry connectedBoundry { get; private set; }

    private Dictionary<NavQuad, NavigationIsland> boundryQuadsToIslands = new Dictionary<NavQuad, NavigationIsland>();

    // adjacent connections
    private Dictionary<NavQuad, int[]> connections = null;
    private Dictionary<NavigationIsland, List<NavigationIsland>> islandsToIslands = null;


    private object adjacentBoundryCalculationLock = new object();

    public NavCellBoundry(IEnumerable<NavQuad> quads, Dictionary<NavQuad, NavigationIsland> quadsToIslands)
    {
        this.quads = quads.ToArray() ?? new NavQuad[] { };

        this.boundryQuadsToIslands = quadsToIslands;
    }

    public void SetAdjacentBoundry(NavCellBoundry adjacent, NavMeshGenerationSettings settings)
    {
        if (adjacent == null)
        {
            connections = null;
            islandsToIslands = null;
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

                    Dictionary<NavigationIsland, List<NavigationIsland>> thisToThatIslands
                        = new Dictionary<NavigationIsland, List<NavigationIsland>>();
                    Dictionary<NavigationIsland, List<NavigationIsland>> thatToThisIslands
                        = new Dictionary<NavigationIsland, List<NavigationIsland>>();

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
                    float requiredWidth = settings.orientation.VoxelSize * 0.5f;
                    float requiredHeight = settings.crouchHeightInVoxels * settings.orientation.VoxelSize * 0.5f;

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

                    // link this boundries islands to that boundries islands
                    foreach (var quad in this.quads)
                    {
                        var thisIsland = boundryQuadsToIslands[quad];

                        // ensure map contains island
                        if (!thisToThatIslands.ContainsKey(thisIsland))
                        {
                            thisToThatIslands.Add(thisIsland, new List<NavigationIsland>());
                        }

                        // add link in map
                        var thisList = thisToThatIslands[thisIsland];

                        foreach (var other in GetConnectedQuads(quad))
                        {
                            // get islands
                            var otherIsland = adjacent.boundryQuadsToIslands[other];


                            if (!thisList.Contains(otherIsland))
                            {
                                thisList.Add(otherIsland);
                            }
                        }
                    }

                    // link that boundries islands to this boundries islands
                    foreach (var other in adjacent.quads)
                    {
                        var otherIsland = adjacent.boundryQuadsToIslands[other];

                        // ensure map contains island
                        if (!thatToThisIslands.ContainsKey(otherIsland))
                        {
                            thatToThisIslands.Add(otherIsland, new List<NavigationIsland>());
                        }

                        var otherList = thatToThisIslands[otherIsland];

                        foreach (var quad in adjacent.GetConnectedQuads(other))
                        {
                            // get islands
                            var thisIsland = boundryQuadsToIslands[quad];


                            // add link in map
                            if (!otherList.Contains(thisIsland))
                            {
                                otherList.Add(thisIsland);
                            }
                        }
                    }

                    this.islandsToIslands = thisToThatIslands;
                    adjacent.islandsToIslands = thatToThisIslands;
                }
        }
    }

    public IEnumerable<NavQuad> GetConnectedQuads(NavQuad quad)
    {
        if (connections != null)
        {
            if (connections.TryGetValue(quad, out int[] adjacentIndicies))
            {
                foreach (var i in adjacentIndicies)
                {
                    if (connectedBoundry.quads.IsInBounds(i))
                    {
                        yield return connectedBoundry.quads[i];
                    }
                }
            }
        }
    }

    public IEnumerable<NavigationIsland> GetConnectedIslands(NavigationIsland island)
    {
        lock (adjacentBoundryCalculationLock)
        {
            if (islandsToIslands != null)
            {
                if (islandsToIslands.TryGetValue(island, out var list))
                {
                    return list;
                }
            }
            return new NavigationIsland[] { };
        }
    }
}