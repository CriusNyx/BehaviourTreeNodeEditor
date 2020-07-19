using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelBoundsExtensions
{
    public static IEnumerable<(int x, int y)> GetVoxelsInBounds(this IVoxelBounds2 bounds, IVoxelOrientation orientation)
    {
        var (min, max) = bounds.GetMinAndMax(orientation);

        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
            {
                yield return (x, y);
            }
    }

    public static IEnumerable<(int x, int y, int z)> GetVoxelsInBounds(this IVoxelBounds3 bounds, IVoxelOrientation orientation)
    {
        var (min, max) = bounds.GetMinAndMax(orientation);

        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                {
                    yield return (x, y, z);
                }
    }
}