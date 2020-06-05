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

    public static IEnumerable<(T element, T nextElement)> ForeachElementAndNextCircular<T>(this T [] array)
    {
        if(array.Length > 1)
        {
            for(int i = 0; i < array.Length - 1; i++)
            {
                yield return (array[i], array[i + 1]);
            }
            yield return (array[array.Length - 1], array[0]);
        }
    }

    public static IEnumerable<(T element, T nextElement)> ForeachElementAndNextCircular<T>(this IEnumerable<T> list)
    {
        T first = default;
        T previous = default;
        var enumerator = list.GetEnumerator();
        if (enumerator.MoveNext())
        {
            first = enumerator.Current;
            previous = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return (previous, enumerator.Current);
                previous = enumerator.Current;
            }
            yield return (previous, first);
        }
    }

    public static bool IsInBounds<T>(this T[] arr, int index)
    {
        return index >= 0 && index < arr.Length;
    }
}