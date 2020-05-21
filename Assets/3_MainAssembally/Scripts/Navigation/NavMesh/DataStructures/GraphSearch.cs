using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphSearch
{
    /// <summary>
    /// Find a set of contiguous nodes be searching through the input space in a spiral pattern.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="seed"></param>
    /// <param name="getAdjacent">Should return the adjacent node in the specified direction.
    /// Direction will be between 0, and 3 [inclusive].
    /// The mapping of ints to directions does not matter, but adjacent integers should represent adjacent direction (counting 3 and 0 as adjacent)
    /// Must have the following signature (T input, int direction) => T adjacent </param>
    /// <returns></returns>
    public static IEnumerable<T> SpiralSearch<T>(T seed, Func<T, int, T?> getAdjacent, HashSet<T> searchSpace) where T : struct
    {
        HashSet<T> output = new HashSet<T>();

        output.Add(seed);

        List<T>[] frontiers = new List<T>[]
        {
            new List<T>(){seed},
            new List<T>(){seed},
            new List<T>(){seed},
            new List<T>(){seed}
        };

        Queue<int> directionsToPush = new Queue<int>(new int[] { 0, 1, 2, 3} );

        while(directionsToPush.Count != 0)
        {
            var nextDirection = directionsToPush.Dequeue();

            // Check if frontiers can be pushed
            (var success, var newList) = PushFrontier(frontiers[nextDirection], nextDirection, getAdjacent, searchSpace);

            if (success)
            {
                // Push direction back into queue to be processed on the next itteration
                directionsToPush.Enqueue(nextDirection);

                // Get the nodes at the left and right of the frontier
                var left = newList[0];
                var right = newList[newList.Count - 1];

                // Get the indicies of the adjacet frontiers
                int leftListIndex = (nextDirection + 3) % 4;
                int rightListIndex = (nextDirection + 1) % 4;

                // Add the new corners to the adjacent frontier lists
                frontiers[leftListIndex].Insert(0, right);
                frontiers[rightListIndex].Add(left);

                // Add results to output
                foreach(var result in newList)
                {
                    output.Add(result);
                }

                frontiers[nextDirection] = newList;
            }
        }

        return output;
    }

    private static (bool success, List<T> output) PushFrontier<T>(
        List<T> frontier, int indexToPush, 
       Func<T, int, T?> getAdjacent,
       HashSet<T> searchSpace) where T : struct
    {
        List<T> output = new List<T>();
        foreach(var element in frontier)
        {
            var next = getAdjacent(element, indexToPush);
            if(next == null || !searchSpace.Contains(next.Value))
            {
                return (false, output);
            }
            else
            {
                output.Add(next.Value);
            }
        }

        return (true, output);
    }
}
