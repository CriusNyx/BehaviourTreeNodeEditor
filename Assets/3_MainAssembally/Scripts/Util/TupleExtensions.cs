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
}