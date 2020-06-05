using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionVoxelBounds2 : IVoxelBounds2
{
    public readonly IVoxelBounds2 a, b;

    public IntersectionVoxelBounds2(IVoxelBounds2 a, IVoxelBounds2 b)
    {
        this.a = a;
        this.b = b;
    }

    public ((int x, int y) min, (int x, int y) max) GetMinAndMax(IVoxelOrientation orientation)
    {
        var (minA, maxA) = a.GetMinAndMax(orientation);
        var (minB, maxB) = b.GetMinAndMax(orientation);

        return (minA.Zip(minB, Math.Max), maxA.Zip(maxB, Math.Min));
    }
}

public class IntersectionVoxelBounds3 : IVoxelBounds3
{
    public readonly IVoxelBounds3 a, b;

    public IntersectionVoxelBounds3(IVoxelBounds3 a, IVoxelBounds3 b)
    {
        this.a = a;
        this.b = b;
    }

    public ((int x, int y, int z) min, (int x, int y, int z) max) GetMinAndMax(IVoxelOrientation orientation)
    {
        var (minA, maxA) = a.GetMinAndMax(orientation);
        var (minB, maxB) = b.GetMinAndMax(orientation);

        return (minA.Zip(minB, Math.Max), maxA.Zip(maxB, Math.Min));
    }
}