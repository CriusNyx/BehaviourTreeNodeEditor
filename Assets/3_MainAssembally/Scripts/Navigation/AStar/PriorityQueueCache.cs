using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PriorityQueueCache<T, U> where T : IComparable<T>
{
    private static Queue<CachedPriorityQueue<T, U>> cache = new Queue<CachedPriorityQueue<T, U>>();

    public static void AddQueue(CachedPriorityQueue<T, U> queue)
    {
        cache.Enqueue(queue);
    }

    public static CachedPriorityQueue<T, U> GetQueue()
    {
        if(cache.Count > 0)
        {
            return cache.Dequeue();
        }
        else
        {
            return new CachedPriorityQueue<T, U>();
        }
    }
}