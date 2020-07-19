using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FastRemovePriorityQueue<T, U> where T : IComparable<T>
{
    FastRemoveMinHeap<T, U> minHeap = new FastRemoveMinHeap<T, U>();

    public int Count => minHeap.Length;

    public void Enqueue(T key, U value)
        => minHeap.Add(key, value);

    public (T key, U value) Dequeue()
        => minHeap.Remove();

    public (T key, U value) Remove(U value)
        => minHeap.Remove(value);

    public bool Contains(U value) 
        => minHeap.Contains(value);

    public void Clear()
        => minHeap.Clear();
}