using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MinHeapTest : MonoBehaviour
{
    private void Start()
    {
        FastRemoveMinHeap<int, object> heap = new FastRemoveMinHeap<int, object>();
        List<int> values = new List<int>() { 1, 1, 1, 1, 1, 2, 2, 2, 2, 2 };
        int[] arr = new int[10];
        List<object> objects = new List<object>();

        while (values.Count > 0)
        {
            int index = Random.Range(0, values.Count);
            var value = new object();
            heap.Add(values[index], value);
            values.RemoveAt(index);
            objects.Add(value);
            heap.CheckInvariant();
        }

        while (heap.Length > 0)
        {
            Debug.Log(heap.ToString());
            var element = heap.Remove();
            objects.Remove(element.value);
            Debug.Log(element);
            heap.CheckInvariant();

            if (heap.Length > 0)
            {
                int removeIndex = Random.Range(0, objects.Count);

                Debug.Log(heap.ToString());
                Debug.Log(heap.Remove(objects[removeIndex]));
                heap.CheckInvariant();

                objects.RemoveAt(removeIndex);
            }
        }
    }

    public string Print(int[] arr)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("[");
        for (int i = 0; i < arr.Length; i++)
        {
            builder.Append($"{arr[i]}, ");
        }
        return $"{builder.ToString().Trim(',', ' ')}]";
    }
}