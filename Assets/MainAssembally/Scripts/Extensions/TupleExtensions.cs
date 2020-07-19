using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TupleExtensions
{
    public static (U, U) Map<T, U>(this (T, T) a, Func<T, U> func)
    {
        return (
            func(a.Item1), 
            func(a.Item2));
    }

    public static (U, U, U) Map<T, U>(this (T, T, T) a, Func<T, U> func)
    {
        return (
            func(a.Item1), 
            func(a.Item2), 
            func(a.Item3));
    }

    public static (U, U, U, U) Map<T, U>(this (T, T, T, T) a, Func<T, U> func)
    {
        return (
            func(a.Item1), 
            func(a.Item2), 
            func(a.Item3), 
            func(a.Item4));
    }

    public static (U, U) Zip<T, U>(this (T, T) a, (T, T) b, Func<T, T, U> func)
    {
        return (
            func(a.Item1, b.Item1), 
            func(a.Item2, b.Item2));
    }

    public static (U, U, U) Zip<T, U>(this (T, T, T) a, (T, T, T) b, Func<T, T, U> func)
    {
        return (
            func(a.Item1, b.Item1), 
            func(a.Item2, b.Item2), 
            func(a.Item3, b.Item3));
    }

    public static (U, U, U, U) Zip<T, U>(this (T, T, T, T) a, (T, T, T, T) b, Func<T, T, U> func)
    {
        return (
            func(a.Item1, b.Item1), 
            func(a.Item2, b.Item2), 
            func(a.Item3, b.Item3), 
            func(a.Item4, b.Item4));
    }

    public static IEnumerable<(T, U)> Zip<T, U>(this IEnumerable<T> a, IEnumerable<U> other)
    {
        var enumeratorA = a.GetEnumerator();
        var enumeratorB = other.GetEnumerator();
        while(enumeratorA.MoveNext() && enumeratorB.MoveNext())
        {
            yield return (enumeratorA.Current, enumeratorB.Current);
        }
    }

    public static IEnumerable<(T, U, V)> Zip<T, U, V>(this IEnumerable<(T, U)> a, IEnumerable<V> other)
    {
        var enumeratorA = a.GetEnumerator();
        var enumeratorB = other.GetEnumerator();
        while (enumeratorA.MoveNext() && enumeratorB.MoveNext())
        {
            var (t, u) = enumeratorA.Current;
            var v = enumeratorB.Current;
            yield return (t, u, v);
        }
    }

    public static IEnumerable<(T, U, V)> Zip<T, U, V>(this IEnumerable<T> a, IEnumerable<(U, V)> other)
    {
        var enumeratorA = a.GetEnumerator();
        var enumeratorB = other.GetEnumerator();
        while (enumeratorA.MoveNext() && enumeratorB.MoveNext())
        {
            var t = enumeratorA.Current;
            var (u, v) = enumeratorB.Current;
            yield return (t, u, v);
        }
    }

    public static IEnumerable<(T, U, V, W)> Zip<T, U, V, W>(this IEnumerable<(T, U, V)> a, IEnumerable<W> other)
    {
        var enumeratorA = a.GetEnumerator();
        var enumeratorB = other.GetEnumerator();
        while (enumeratorA.MoveNext() && enumeratorB.MoveNext())
        {
            var (t, u, v) = enumeratorA.Current;
            var w = enumeratorB.Current;
            yield return (t, u, v, w);
        }
    }

    public static IEnumerable<(T, U, V, W)> Zip<T, U, V, W>(this IEnumerable<(T, U)> a, IEnumerable<(V, W)> other)
    {
        var enumeratorA = a.GetEnumerator();
        var enumeratorB = other.GetEnumerator();
        while (enumeratorA.MoveNext() && enumeratorB.MoveNext())
        {
            var (t, u) = enumeratorA.Current;
            var (v, w) = enumeratorB.Current;
            yield return (t, u, v, w);
        }
    }

    public static IEnumerable<(T, U, V, W)> Zip<T, U, V, W>(this IEnumerable<T> a, IEnumerable<(U, V, W)> other)
    {
        var enumeratorA = a.GetEnumerator();
        var enumeratorB = other.GetEnumerator();
        while (enumeratorA.MoveNext() && enumeratorB.MoveNext())
        {
            var t = enumeratorA.Current;
            var (u, v, w) = enumeratorB.Current;
            yield return (t, u, v, w);
        }
    }
}