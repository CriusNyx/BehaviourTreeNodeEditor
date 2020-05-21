using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VoxelOrientation
{
    public readonly float voxelSize;
    public readonly Vector3 center;

    public VoxelOrientation(float voxelSize, Vector3 center)
    {
        this.voxelSize = voxelSize;
        this.center = center;
    }

    /// <summary>
    /// Returns the closest voxel index that matches this point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public (int x, int y, int z) GetVoxelIndexOfPoint(Vector3 point)
        => GetVoxelIndexOfPoint(point, Mathf.RoundToInt);

    /// <summary>
    /// Returns the closest voxel index that matches this point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public (int x, int y, int z) GetVoxelIndexOfPoint(Vector3 point, Func<float, int> floatToIntMapping)
    {
        Vector3 vertex = (point - center) / voxelSize;
        int x = floatToIntMapping(vertex.x);
        int y = floatToIntMapping(vertex.y);
        int z = floatToIntMapping(vertex.z);

        return (x, y, z);
    }

    /// <summary>
    /// Returns the position that matches this index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 GetPointOfVoxel((int x, int y, int z) index)
    {
        return GetPointOfVoxel(index.x, index.y, index.z);
    }

    /// <summary>
    /// Returns the position that matches this index
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public Vector3 GetPointOfVoxel(int x, int y, int z)
    {
        return (new Vector3(x, y, z) * voxelSize) + center;
    }

    public IEnumerable<(int x, int y, int z)> GetIndexesInBounds((int x, int y, int z) min, (int x, int y, int z) max)
    {
        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                    yield return (x, y, z);
    }
}
