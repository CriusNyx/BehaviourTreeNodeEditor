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

    public static IEnumerable<(T element, T nextElement)> ForeachElementAndNext<T>(this IEnumerable<T> list)
    {
        var enumerator = list.GetEnumerator();
        if (enumerator.MoveNext())
        {
            var last = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return (last, enumerator.Current);
                last = enumerator.Current;
            }
        }
    }
}