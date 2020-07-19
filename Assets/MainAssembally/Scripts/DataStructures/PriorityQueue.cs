using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T, U> where T : IComparable<T>
{
    private MinHeap<T, U> minHeap = new MinHeap<T, U>();

    public int Count => minHeap.Length;

    public void Enqueue(T key, U value)
    {
        minHeap.Add(key, value);
    }

    public (T key, U value) Dequeue()
    {
        var output = minHeap.Remove();
        return output;
    }
}