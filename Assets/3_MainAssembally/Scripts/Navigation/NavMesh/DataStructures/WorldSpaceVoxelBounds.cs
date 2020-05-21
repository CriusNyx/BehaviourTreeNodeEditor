using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceVoxelBounds3 : IVoxelBounds3
{
    public readonly Vector3 min, max;
    public readonly VoxelOrientation voxelOrientation;

    public (int x, int y, int z) Min => voxelOrientation.GetVoxelIndexOfPoint(min);
    public (int x, int y, int z) Max => voxelOrientation.GetVoxelIndexOfPoint(max);

    public WorldSpaceVoxelBounds3(Vector3 min, Vector3 max, VoxelOrientation voxelOrientation)
    {
        this.min = min;
        this.max = max;
        this.voxelOrientation = voxelOrientation;
    }

    public WorldSpaceVoxelBounds3(Bounds bounds, VoxelOrientation voxelOrientation)
    {
        this.min = bounds.min;
        this.max = bounds.max;
        this.voxelOrientation = voxelOrientation;
    }

    public IEnumerable<(int x, int y, int z)> GetVoxelsInBounds()
    {
        (int x, int y, int z) minIndex = voxelOrientation.GetVoxelIndexOfPoint(min, Mathf.RoundToInt);
        (int x, int y, int z) maxIndex = voxelOrientation.GetVoxelIndexOfPoint(max, Mathf.RoundToInt);

        return voxelOrientation.GetIndexesInBounds(minIndex, maxIndex);
    }

    public IVoxelBounds3 Intersect(IVoxelBounds3 other)
    {
        switch (other)
        {
            case WorldSpaceVoxelBounds3 worldSpaceBounds:
                return new WorldSpaceVoxelBounds3(
                    Vector3.Max(min, worldSpaceBounds.min), 
                    Vector3.Min(max, worldSpaceBounds.max), 
                    voxelOrientation);
            default:
                throw new NotImplementedException();
        }
    }

    public IVoxelBounds3 Union(IVoxelBounds3 other)
    {
        switch (other)
        {
            case WorldSpaceVoxelBounds3 worldSpaceBounds:
                return new WorldSpaceVoxelBounds3(
                    Vector3.Min(min, worldSpaceBounds.min),
                    Vector3.Max(max, worldSpaceBounds.max),
                    voxelOrientation);
            default:
                throw new NotImplementedException();
        }
    }
}

public class WorldSpaceVoxelBounds2 : IVoxelBounds2
{
    public readonly Vector2 min, max;
    public readonly VoxelOrientation voxelOrientation;

    public (int x, int y) Min
    {
        get
        {
            (int x, int y, int z) = voxelOrientation.GetVoxelIndexOfPoint(min);
            return (x, y);
        }
    }

    public (int x, int y) Max
    {
        get
        {
            (int x, int y, int z) = voxelOrientation.GetVoxelIndexOfPoint(max);
            return (x, y);
        }
    }

    public WorldSpaceVoxelBounds2(Vector2 min, Vector2 max, VoxelOrientation voxelOrientation)
    {
        this.min = min;
        this.max = max;
        this.voxelOrientation = voxelOrientation;
    }

    public IEnumerable<(int x, int y)> GetVoxelsInBounds()
    {
        (int x, int y, int z) minIndex = voxelOrientation.GetVoxelIndexOfPoint(min, Mathf.RoundToInt);
        (int x, int y, int z) maxIndex = voxelOrientation.GetVoxelIndexOfPoint(max, Mathf.RoundToInt);

        foreach(var output in voxelOrientation.GetIndexesInBounds(minIndex, maxIndex))
        {
            yield return (output.x, output.y);
        }
    }

    public IVoxelBounds2 Intersect(IVoxelBounds2 other)
    {
        switch (other)
        {
            case WorldSpaceVoxelBounds2 worldSpaceBounds:
                return new WorldSpaceVoxelBounds2(
                    Vector2.Max(min, worldSpaceBounds.min),
                    Vector2.Min(max, worldSpaceBounds.max),
                    voxelOrientation);
            default:
                throw new NotImplementedException();
        }
    }

    public IVoxelBounds2 Union(IVoxelBounds2 other)
    {
        switch (other)
        {
            case WorldSpaceVoxelBounds2 worldSpaceBounds:
                return new WorldSpaceVoxelBounds2(
                    Vector2.Min(min, worldSpaceBounds.min),
                    Vector2.Max(max, worldSpaceBounds.max),
                    voxelOrientation);
            default:
                throw new NotImplementedException();
        }
    }
}