using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachedPriorityQueue<T, U> : FastRemovePriorityQueue<T, U>, IDisposable where T : IComparable<T>
{
    public void Dispose()
    {
        Clear();
        PriorityQueueCache<T, U>.AddQueue(this);
    }
}