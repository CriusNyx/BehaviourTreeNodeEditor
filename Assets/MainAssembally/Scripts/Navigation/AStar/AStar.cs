using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    /// <summary>
    /// Uses the A* algorithm to find a route from start to end.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="GetHScore"></param>
    /// <param name="GetWScore"></param>
    /// <param name="GetAdjacent"></param>
    /// <returns></returns>
    public static IEnumerable<T> FindRoute<T>(T start, T end, Func<T, T, float> GetHScore, Func<T, T, float> GetWScore, Func<T, IEnumerable<T>> GetAdjacent) where T : class
    {
        AStarSolver<T> solver = new AStarSolver<T>(GetHScore, GetWScore, GetAdjacent);
        return solver.Solve(start, end);
    }

    /// <summary>
    /// Stores the memory needed to solve an AStar search
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private class AStarSolver<T> where T : class
    {
        private (float bestHScore, AStarNode<T> bestAStarNode) bestResultSoFar = (Mathf.Infinity, null);

        private CachedPriorityQueue<float, AStarNode<T>> openSet;
        private HashSet<T> closedSet = new HashSet<T>();

        private Dictionary<T, AStarNode<T>> nodes = new Dictionary<T, AStarNode<T>>();
        private Func<T, T, float> GetHScore;
        private Func<T, T, float> GetWScore;
        private Func<T, IEnumerable<T>> GetAdjacent;

        public AStarSolver(Func<T, T, float> getHScore, Func<T, T, float> getWScore, Func<T, IEnumerable<T>> getAdjacent)
        {
            GetHScore = getHScore ?? throw new ArgumentNullException(nameof(getHScore));
            GetWScore = getWScore ?? throw new ArgumentNullException(nameof(getWScore));
            GetAdjacent = getAdjacent ?? throw new ArgumentNullException(nameof(getAdjacent));
        }

        public IEnumerable<T> Solve(T start, T end)
        {
            using (openSet = PriorityQueueCache<float, AStarNode<T>>.GetQueue())
            {
                AStarNode<T> startNode = new AStarNode<T>(start, 0f, GetHScore(start, end), null);
                nodes[start] = startNode;
                TryOpenNode(start);

                while (openSet.Count != 0)
                {
                    var current = openSet.Dequeue().value;

                    if (current.node == end)
                    {
                        return TracePath(current);
                    }

                    TryCloseNode(current.node);

                    foreach (var adjacent in GetAdjacent(current.node))
                    {
                        UpdateGScore(adjacent, current.node, end);
                        TryOpenNode(adjacent);
                    }
                }

                return TracePath(bestResultSoFar.bestAStarNode);
            }
        }

        private IEnumerable<T> TracePath(AStarNode<T> result)
        {
            Stack<T> stack = new Stack<T>();
            for (var current = result; current != null; current = current.previousNode)
            {
                stack.Push(current.node);
            }

            return stack.ToArray();
        }

        public void UpdateGScore(T target, T previous, T endNode)
        {
            var previousNode = nodes[previous];
            float tentitiveGScore = previousNode.gScore + GetWScore(target, previous);

            if (!nodes.ContainsKey(target))
            {
                nodes[target] = new AStarNode<T>(target, tentitiveGScore, GetHScore(target, endNode), previousNode);
                TryOpenNode(target);
            }
            else
            {
                TryUpdateGScore(target, previous, tentitiveGScore);
            }
        }

        private void TryOpenNode(T target)
        {
            var targetNode = nodes[target];

            if (!openSet.Contains(targetNode) && !closedSet.Contains(target))
            {
                AddOrUpdateOpenSet(targetNode);
            }
        }

        private void TryUpdateGScore(T target, T previous, float newGScore)
        {
            var targetNode = nodes[target];
            if (newGScore < targetNode.gScore)
            {
                targetNode.gScore = newGScore;
                targetNode.previousNode = nodes[previous];

                AddOrUpdateOpenSet(targetNode);
            }
        }

        private void AddOrUpdateOpenSet(AStarNode<T> node)
        {
            if(node.hScore < bestResultSoFar.bestHScore)
            {
                bestResultSoFar = (node.hScore, node);
            }
            
            if (openSet.Contains(node))
            {
                openSet.Remove(node);
            }
            openSet.Enqueue(node.fScore, node);
        }

        private void TryCloseNode(T node)
        {
            if (!closedSet.Contains(node))
            {
                closedSet.Add(node);

                var aStarNode = nodes[node];
                if (openSet.Contains(aStarNode))
                {
                    openSet.Remove(aStarNode);
                }
            }
        }
    }

    private class AStarNode<T>
    {
        public readonly T node;
        public float gScore;
        public readonly float hScore;
        public float fScore => gScore + hScore;

        public AStarNode<T> previousNode;


        public AStarNode(T node, float gScore, float hScore, AStarNode<T> previousNode)
        {
            this.node = node;
            this.gScore = gScore;
            this.hScore = hScore;
            this.previousNode = previousNode;
        }
    }
}