using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VoxelOrientation : IVoxelOrientation
{
    public readonly float voxelSize;
    public readonly Vector3 center;
    public readonly Quaternion rotation;

    public float VoxelSize => voxelSize;
    public Vector3 Center => center;
    public Quaternion Rotation => rotation;

    public VoxelOrientation(float voxelSize, Vector3 center) 
        : this(voxelSize, center, Quaternion.identity)
    {

    }

    public VoxelOrientation(float voxelSize, Vector3 center, Quaternion rotation)
    {
        this.voxelSize = voxelSize;
        this.center = center;
        this.rotation = rotation;
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
        Vector3 lovalPoint = (Quaternion.Inverse(rotation) * (point - center)) / voxelSize;
        int x = floatToIntMapping(lovalPoint.x);
        int y = floatToIntMapping(lovalPoint.y);
        int z = floatToIntMapping(lovalPoint.z);

        return (x, y, z);
    }

    /// <summary>
    /// Returns the position that matches this index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 GetWorldPointOfVoxel((int x, int y, int z) index)
    {
        return GetWorldPointOfVoxel(index.x, index.y, index.z);
    }

    /// <summary>
    /// Returns the position that matches this index
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public Vector3 GetWorldPointOfVoxel(int x, int y, int z)
    {
        return rotation * (new Vector3(x, y, z) * voxelSize) + center;
    }

    /// <summary>
    /// Return all indexes within the specified bounds
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public IEnumerable<(int x, int y, int z)> GetIndexesInBounds(
        (int x, int y, int z) min, 
        (int x, int y, int z) max)
    {
        for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
                for (int z = min.z; z <= max.z; z++)
                    yield return (x, y, z);
    }

    public Vector3 GetLocalPointOfVoxel((int x, int y, int z) index)
        => GetWorldPointOfVoxel(index);

    public Vector3 GetLocalPointOfVoxel(int x, int y, int z)
        => GetWorldPointOfVoxel(x, y, z);
}
