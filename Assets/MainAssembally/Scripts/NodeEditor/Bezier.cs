using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    public static Vector2[] ConstructBezierFromNormal2(Vector2 start, Vector2 end, Vector2 normal, int resolution)
    {
        Vector2 delta = end - start;
        Vector2 halfDelta = delta / 2f;
        Vector2 key1 = start + (Vector2)Vector3.Project(halfDelta, normal);
        Vector2 key2 = end + (Vector2)Vector3.Project(-halfDelta, -normal);

        return GetBezier2(new Vector2[] { start, key1, key2, end }, resolution);
    }

    public static Vector3[] GetBezier3(Vector3[] keys, int resolution)
        => GetBezier(keys, resolution, GetPoint3);

    public static Vector2[] GetBezier2(Vector2[] keys, int resolution) 
        => GetBezier(keys, resolution, GetPoint2);

    private static T[] GetBezier<T>(T[] keys, int resolution, Func<T[], float, T> GetPoint)
    {
        T[] output = new T[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float max = resolution - 1;
            output[i] = GetPoint(keys, i / max);
        }

        return output;
    }

    public static Vector3 GetPoint3(Vector3[] keys, float t)
    {
        return GetPoint(
            keys.Clone() as Vector3[],
            t,
            Vector3.Lerp,
            keys.Length);
    }

    public static Vector2 GetPoint2(Vector2[] keys, float t)
    {
        return GetPoint(
            keys.Clone() as Vector2[],
            t,
            Vector2.Lerp,
            keys.Length);
    }

    private static T GetPoint<T>(T[] keys, float t, Func<T, T, float, T> lerp, int pointLength)
    {
        if (pointLength == 1)
        {
            return keys[0];
        }
        else
        {
            for (int i = 0; i < pointLength - 1; i++)
            {
                keys[i] = lerp(keys[i], keys[i + 1], t);
            }
            return GetPoint(keys, t, lerp, pointLength - 1);
        }
    }
}