using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformVoxelOrientation : IVoxelOrientation
{
    private OffThreadTransform transform;
    public Vector3 offset;
    public Quaternion rotation;
    public Vector3 scale;

    public float VoxelSize { get; private set; }

    public Vector3 Center => transform.Rotation * offset + transform.Position;

    public TransformVoxelOrientation(Transform transform, Vector3 offset, Quaternion rotation, Vector3 scale, float voxelSize)
    {
        this.transform = transform;

        this.offset = offset;
        this.rotation = rotation;
        this.scale = scale;
        this.VoxelSize = voxelSize;
    }

    public (int x, int y, int z) GetVoxelIndexOfPoint(Vector3 point) 
        => GetVoxelIndexOfPoint(point, Mathf.RoundToInt);

    public (int x, int y, int z) GetVoxelIndexOfPoint(Vector3 point, Func<float, int> floatToIntMapping)
    {
        Matrix4x4 transformToWorldMatrix = Matrix4x4.TRS(transform.Position, transform.Rotation, transform.LocalScale);
        Matrix4x4 localToTransformMatrix = Matrix4x4.TRS(offset, rotation, scale);
        Vector3 localPos = localToTransformMatrix.inverse.MultiplyPoint(transformToWorldMatrix.inverse.MultiplyPoint(point));
        localPos /= VoxelSize;
        return (floatToIntMapping(localPos.x), floatToIntMapping(localPos.y), floatToIntMapping(localPos.z));
    }

    public Vector3 GetWorldPointOfVoxel((int x, int y, int z) index) 
        => GetWorldPointOfVoxel(index.x, index.y, index.z);

    public Vector3 GetWorldPointOfVoxel(int x, int y, int z)
    {
        Matrix4x4 transformToWorldMatrix = Matrix4x4.TRS(transform.Position, transform.Rotation, transform.LocalScale);

        return transformToWorldMatrix.MultiplyPoint(GetLocalPointOfVoxel(x, y, z));
    }

    public Vector3 GetLocalPointOfVoxel((int x, int y, int z) index)
        => GetLocalPointOfVoxel(index.x, index.y, index.z);

    public Vector3 GetLocalPointOfVoxel(int x, int y, int z)
    {
        Matrix4x4 localToTransformMatrix = Matrix4x4.TRS(offset, rotation, scale);
        Vector3 localPos = new Vector3(x, y, z);
        localPos *= VoxelSize;

        return localToTransformMatrix.MultiplyPoint(localPos);
    }
}
