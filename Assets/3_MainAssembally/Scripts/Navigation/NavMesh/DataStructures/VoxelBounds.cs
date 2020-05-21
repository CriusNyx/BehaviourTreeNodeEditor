using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct VoxelBounds3 : IVoxelBounds3
{
    public readonly (int x, int y, int z)
        min,
        max;

    public (int x, int y, int z) Min => min;
    public (int x, int y, int z) Max => max;

    public VoxelBounds3(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        : this((minX, minY, minZ), (maxX, maxY, maxZ))
    {

    }

    public VoxelBounds3((int x, int y, int z) min, (int x, int y, int z) max)
    {
        this.min = min;
        this.max = max;
    }

    public IEnumerable<(int x, int y, int z)> GetVoxelsInBounds()
    {
        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                    yield return (x, y, z);
    }

    public static VoxelBounds3 Intersect(VoxelBounds3 a, VoxelBounds3 b)
    {
        return new VoxelBounds3(ComponentMax(a.min, b.min), ComponentMin(a.max, b.max));
    }

    public static VoxelBounds3 Union(VoxelBounds3 a, VoxelBounds3 b)
    {
        return new VoxelBounds3(ComponentMin(a.min, b.min), ComponentMax(a.max, b.max));
    }

    private static (int x, int y, int z) ComponentMin(
        (int x, int y, int z) a,
        (int x, int y, int z) b)
    {
        return a.Zip(b, Math.Min);
    }

    private static (int x, int y, int z) ComponentMax(
        (int x, int y, int z) a,
        (int x, int y, int z) b)
    {
        return a.Zip(b, Math.Max);
    }

    public IVoxelBounds3 Union(IVoxelBounds3 other)
    {
        switch (other)
        {
            case VoxelBounds3 bounds:
                return Union(this, bounds);
            default:
                throw new NotImplementedException();
        }
    }

    public IVoxelBounds3 Intersect(IVoxelBounds3 other)
    {
        switch (other)
        {
            case VoxelBounds3 bounds:
                return Intersect(this, bounds);
            default:
                throw new NotImplementedException();
        }
    }
}

public struct VoxelBounds2 : IVoxelBounds2
{
    public readonly (int x, int y)
        min,
        max;

    public (int x, int y) Min => min;
    public (int x, int y) Max => max;

    public VoxelBounds2(int minX, int minY, int maxX, int maxY)
        : this((minX, minY), (maxX, maxY))
    {

    }

    public VoxelBounds2((int x, int y) min, (int x, int y) max)
    {
        this.min = min;
        this.max = max;
    }

    public IEnumerable<(int x, int y)> GetVoxelsInBounds()
    {
        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                yield return (x, y);
    }

    public static VoxelBounds2 Intersect(VoxelBounds2 a, VoxelBounds2 b)
    {
        return new VoxelBounds2(ComponentMax(a.min, b.min), ComponentMin(a.max, b.max));
    }

    public static VoxelBounds2 Union(VoxelBounds2 a, VoxelBounds2 b)
    {
        return new VoxelBounds2(ComponentMin(a.min, b.min), ComponentMax(a.max, b.max));
    }

    private static (int x, int y) ComponentMin(
        (int x, int y) a,
        (int x, int y) b)
    {
        return a.Zip(b, Math.Min);
    }

    private static (int x, int y) ComponentMax(
        (int x, int y) a,
        (int x, int y) b)
    {
        return a.Zip(b, Math.Max);
    }

    public IVoxelBounds2 Union(IVoxelBounds2 other)
    {
        switch (other)
        {
            case VoxelBounds2 bounds:
                return Union(this, bounds);
            default:
                throw new NotImplementedException();
        }
    }

    public IVoxelBounds2 Intersect(IVoxelBounds2 other)
    {
        switch (other)
        {
            case VoxelBounds2 bounds:
                return Intersect(this, bounds);
            default:
                throw new NotImplementedException();
        }
    }
}
