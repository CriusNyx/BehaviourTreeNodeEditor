using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavGraph
{
    private readonly Dictionary<NavQuad, List<NavQuad>> quadsTolinks = new Dictionary<NavQuad, List<NavQuad>>();
    private readonly Dictionary<NavQuad, NavigationIsland> quadsToIslands = new Dictionary<NavQuad, NavigationIsland>();
    private readonly List<NavigationIsland> islands = new List<NavigationIsland>();

    public void AddLink(NavQuad a, NavQuad b)
    {
        GetList(b);
        var aList = GetList(a);
        if (!aList.Contains(b))
        {
            aList.Add(b);
        }
    }

    private List<NavQuad> GetList(NavQuad quad)
    {
        if (!quadsTolinks.ContainsKey(quad))
        {
            quadsTolinks.Add(quad, new List<NavQuad>());
        }
        return quadsTolinks[quad];
    }

    public IEnumerable<(NavQuad node, IEnumerable<NavQuad> adjacent)> GetNodes()
    {
        foreach (var element in quadsTolinks)
        {
            yield return (element.Key, element.Value);
        }
    }

    public Dictionary<NavQuad, NavigationIsland> GenerateIslands(NavMeshCell cell)
    {
        FindIslands(cell);
        LinkIslands();

        return quadsToIslands;
    }

    private void FindIslands(NavMeshCell cell)
    {
        Color[] colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
        int index = 0;

        Queue<NavQuad> nodes = new Queue<NavQuad>(quadsTolinks.Keys);
        HashSet<NavQuad> visitedNodes = new HashSet<NavQuad>();

        while (nodes.Count > 0)
        {
            var next = nodes.Dequeue();
            if (visitedNodes.Contains(next))
            {
                continue;
            }

            NavigationIsland island = new NavigationIsland(cell);
            islands.Add(island);

            foreach (var node in GraphSearch.BreathFirstSearch(next, GetAdjacentNodesForIslandSearch))
            {
                visitedNodes.Add(node);
                quadsToIslands.Add(node, island);
                int i = index;
            }
            index++;
            index %= colors.Length;
        }
    }

    private void LinkIslands()
    {
        Dictionary<NavigationIsland, (Vector3 pos, float influence)> centerMap 
            = new Dictionary<NavigationIsland, (Vector3 pos, float influence)>();

        foreach (var (node, links) in GetNodes())
        {
            var thisIsland = quadsToIslands[node];

            //approximate position
            float size = node.scale.x * node.scale.z;
            Vector3 pos = node.position * size;
            if (centerMap.TryGetValue(thisIsland, out var value))
            {
                centerMap[thisIsland] = (value.pos + pos, value.influence + size);
            }
            else
            {
                centerMap[thisIsland] = (pos, size);
            }

            foreach(var link in links)
            {
                var otherIsland = quadsToIslands[link];
                if(otherIsland != thisIsland && !thisIsland.links.Contains(otherIsland))
                {
                    thisIsland.links.Add(otherIsland);
                }
            }
        }

        foreach(var pair in centerMap)
        {
            pair.Key.center = pair.Value.pos / pair.Value.influence;
        }
    }

    private IEnumerable<NavQuad> GetAdjacentNodesForIslandSearch(NavQuad node)
    {
        foreach (var adj in GetList(node))
        {
            if (GetList(adj).Contains(node))
            {
                yield return adj;
            }
        }
    }

    public IEnumerable<NavigationIsland> GetIslands() => islands;

    public IEnumerable<NavigationIsland> GetConnectedIslands(NavigationIsland island)
    {
        return island.links;
    }

    public NavQuad GetQuadForPosition(Vector3 position)
    {
        NavQuad bestQuad = null;
        float bestQuadDistance = Mathf.Infinity;
        foreach(var quad in quadsTolinks.Keys)
        {
            if(quad.ContainsPoint(position, out float f))
            {
                if(f >= 0 && f < bestQuadDistance)
                {
                    (bestQuadDistance, bestQuad) = (f, quad);
                }
            }
        }
        return bestQuad;
    }

    public NavigationIsland GetIslandFromQuad(NavQuad quad)
    {
        if(quadsToIslands.TryGetValue(quad, out var island))
        {
            return island;
        }
        else
        {
            return null;
        }
    }
}
