using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVoxelOrientation
{
    float VoxelSize { get; }
    Vector3 Center { get; }

    /// <summary>
    /// Returns the closest voxel index that matches this point.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    (int x, int y, int z) GetVoxelIndexOfPoint(Vector3 point);

    /// <summary>
    /// Returns the closest voxel index that matches this point.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    (int x, int y, int z) GetVoxelIndexOfPoint(Vector3 point, Func<float, int> floatToIntMapping);

    /// <summary>
    /// Returns the position that matches this index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    Vector3 GetWorldPointOfVoxel((int x, int y, int z) index);

    /// <summary>
    /// Returns the position that matches this index.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    Vector3 GetWorldPointOfVoxel(int x, int y, int z);

    /// <summary>
    /// Return the point of the voxel in local space
    /// For most types of orientations, this will be the same as world space, 
    /// 
    /// Transform based orientations do distinguish between global and local.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    Vector3 GetLocalPointOfVoxel((int x, int y, int z) index);

    /// <summary>
    /// Return the point of the voxel in local space
    /// For most types of orientations, this will be the same as world space, 
    /// 
    /// Transform based orientations do distinguish between global and local.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    Vector3 GetLocalPointOfVoxel(int x, int y, int z);
}
