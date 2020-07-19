using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelGraph<T>
{
    private class VoxelGraphNode
    {
        public readonly T t;
        public readonly List<(int x, int y, int z)> links = new List<(int x, int y, int z)>();

        public VoxelGraphNode(T t)
        {
            this.t = t;
        }
    }

    private VoxelSet<VoxelGraphNode> set;

    public IVoxelOrientation orientation => set.orientation;

    public VoxelGraph(float voxelSize, Vector3 position)
    {
        set = new VoxelSet<VoxelGraphNode>(voxelSize, position);
    }

    public VoxelGraph(IVoxelOrientation orientation)
    {
        set = new VoxelSet<VoxelGraphNode>(orientation);
    }

    public void Link(
        (int x, int y, int z) startIndex,
        T startValue,
        (int x, int y, int z) endIndex,
        T endValue)
    {
        var startNode = GetNode(startIndex, startValue);

        // ensures end has a node
        GetNode(endIndex, endValue);

        startNode.links.Add(endIndex);
    }

    private VoxelGraphNode GetNode((int x, int y, int z) index, T t)
    {
        if (!set.ContainsKey(index))
        {
            set[index] = new VoxelGraphNode(t);
        }
        return set[index];
    }

    public IEnumerable<((int x, int y, int z) index, T value)> GetElements()
    {
        foreach(var node in set)
        {
            yield return (node.Key, node.Value.t);
        }
    }

    public IEnumerable<((int x, int y, int z) index, IEnumerable<(int x, int y, int z)> adjacent)> GetGraphElements()
    {
        foreach (var pair in set)
            yield return (pair.Key, pair.Value.links);
    }

    public IEnumerable<((int x, int y, int z) index, T t)> GetAdjacent((int x, int y, int z) index)
    {
        if (!set.ContainsKey(index))
        {
            throw new ArgumentException($"The index ({index.x}, {index.y}, {index.z}) is not part of this graph.");
        }

        var node = set[index];

        return
            node.links.Select(
                x =>
                {
                    if (!set.ContainsKey(x))
                    {
                        throw new InvalidOperationException("The state of the graph is damaged.");
                    }
                    return (x, set[x].t);
                });
    }
}
