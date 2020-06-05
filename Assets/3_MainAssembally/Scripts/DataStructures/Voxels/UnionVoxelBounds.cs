using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnionVoxelBounds2 : IVoxelBounds2
{
    public readonly IVoxelBounds2 a, b;

    public UnionVoxelBounds2(IVoxelBounds2 a, IVoxelBounds2 b)
    {
        this.a = a;
        this.b = b;
    }

    public ((int x, int y) min, (int x, int y) max) GetMinAndMax(IVoxelOrientation orientation)
    {
        var (minA, maxA) = a.GetMinAndMax(orientation);
        var (minB, maxB) = a.GetMinAndMax(orientation);

        return (minA.Zip(minB, Math.Min), maxA.Zip(maxB, Math.Max));
    }
}

public class UnionVoxelBounds3 : IVoxelBounds3
{
    public readonly IVoxelBounds3 a, b;

    public UnionVoxelBounds3(IVoxelBounds3 a, IVoxelBounds3 b)
    {
        this.a = a;
        this.b = b;
    }

    public ((int x, int y, int z) min, (int x, int y, int z) max) GetMinAndMax(IVoxelOrientation orientation)
    {
        var (minA, maxA) = a.GetMinAndMax(orientation);
        var (minB, maxB) = a.GetMinAndMax(orientation);

        return (minA.Zip(minB, Math.Min), maxA.Zip(maxB, Math.Max));
    }
}