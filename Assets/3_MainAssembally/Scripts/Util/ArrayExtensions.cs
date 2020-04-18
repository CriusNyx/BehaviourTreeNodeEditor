using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions
{
    public static IEnumerable<(T element, T nextElement)> ForeachElementAndNext<T>(this T[] array)
    {
        for(int i = 0; i < array.Length - 1; i++)
        {
            yield return (array[i], array[i + 1]);
        }
    }
}