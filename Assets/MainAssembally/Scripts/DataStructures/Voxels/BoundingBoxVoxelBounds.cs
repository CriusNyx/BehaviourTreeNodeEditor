using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BoundingBoxVoxelBounds2 : IVoxelBounds2
{
    public readonly Vector2 center;
    public readonly Vector2 size;
    public readonly float rotation;

    public BoundingBoxVoxelBounds2(Vector2 center, Vector2 size, float rotation)
    {
        this.center = center;
        this.size = size;
        this.rotation = rotation;
    }

    /// <summary>
    /// Converts a vector2 to a vector3 from a birds eye perspective
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static Vector3 BirdsEye(Vector2 input)
    {
        return new Vector3(input.x, 0f, input.y);
    }

    /// <summary>
    /// Converts a vector2 to a vector3 from a side on perspective
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static Vector3 SideOn(Vector2 input)
    {
        return (Vector3)input;
    }

    public ((int x, int y) min, (int x, int y) max) GetMinAndMax(IVoxelOrientation orientation) => GetMinAndMax(orientation, SideOn);

    public ((int x, int y) min, (int x, int y) max) GetMinAndMax(IVoxelOrientation orientation, Func<Vector2, Vector3> vector2To3)
    {
        (int x, int y, int z) min = (int.MaxValue, int.MaxValue, int.MaxValue);
        (int x, int y, int z) max = (int.MinValue, int.MinValue, int.MinValue);

        for (float f = 0f; f < 360f; f += 90f)
        {
            Quaternion rot = Quaternion.Euler(0f, 0f, f);
            Vector2 corner = (Vector2)(rot * size) + center;
            var index = orientation.GetVoxelIndexOfPoint(vector2To3(corner));

            min = min.Zip(index, Math.Min);
            max = max.Zip(index, Math.Max);
        }

        return ((min.x, min.y), (max.x, max.y));
    }
}

public struct BoundingBoxVoxelBounds3 : IVoxelBounds3
{
    public enum IgnoreAxis
    {
        none = 0,
        x = 1 << 1,
        y = 1 << 2,
        z = 1 << 3,
        xy = x | y,
        xz = x | z,
        yz = y | z,
        xyz = x | y | z,
    }


    public readonly Vector3 center;
    public readonly Vector3 scale;
    public readonly Quaternion rotation;
    public readonly IgnoreAxis ignoreAxis;

    public BoundingBoxVoxelBounds3(Collider collider, IgnoreAxis ignoreAxis = IgnoreAxis.none)
    {
        if (collider is MeshCollider meshCollider)
        {
            Bounds meshBounds = meshCollider.sharedMesh.bounds;
            center = collider.transform.localToWorldMatrix.MultiplyPoint(meshBounds.center);
            scale = Vector3.Scale(collider.transform.lossyScale, meshBounds.size);
            rotation = collider.transform.rotation;
        }
        else if (collider is BoxCollider boxCollider)
        {
            center = collider.transform.localToWorldMatrix.MultiplyPoint(boxCollider.center);
            scale = Vector3.Scale(collider.transform.lossyScale, boxCollider.size);
            rotation = collider.transform.rotation;
        }
        else if (collider is SphereCollider sphereCollider)
        {
            center = collider.transform.localToWorldMatrix.MultiplyPoint(sphereCollider.center);
            scale = Vector3.Scale(collider.transform.lossyScale, Vector3.one * sphereCollider.radius * 2f);
            rotation = collider.transform.rotation;
        }
        else
        {
            center = collider.bounds.center;
            scale = collider.bounds.size;
            rotation = Quaternion.identity;
        }
        this.ignoreAxis = ignoreAxis;
    }

    public BoundingBoxVoxelBounds3(Vector3 center, Vector3 scale, Quaternion rotation, IgnoreAxis ignoreAxis = IgnoreAxis.none)
    {
        this.center = center;
        this.scale = scale;
        this.rotation = rotation;
        this.ignoreAxis = ignoreAxis;
    }

    public ((int x, int y, int z) min, (int x, int y, int z) max) GetMinAndMax(IVoxelOrientation orientation)
    {
        (int x, int y, int z) min = (int.MaxValue, int.MaxValue, int.MaxValue);
        (int x, int y, int z) max = (int.MinValue, int.MinValue, int.MinValue);

        Vector3 halfScale = scale / 2f;

        if (IsIgnoringAxis(IgnoreAxis.x))
        {
            min.x = int.MinValue;
            max.x = int.MaxValue;
        }
        if (IsIgnoringAxis(IgnoreAxis.y))
        {
            min.y = int.MinValue;
            max.y = int.MaxValue;
        }
        if (IsIgnoringAxis(IgnoreAxis.z))
        {
            min.z = int.MinValue;
            max.z = int.MaxValue;
        }

        // This for loop itterates through each corner
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 pos = (rotation * Vector3.Scale(halfScale, new Vector3(x, y, z))) + center;
                    var index = orientation.GetVoxelIndexOfPoint(pos);
                    min = min.Zip(index, Math.Min);
                    max = max.Zip(index, Math.Max);
                }

        return (min, max);
    }

    public bool IsIgnoringAxis(IgnoreAxis axis)
    {
        return (axis & ignoreAxis) != IgnoreAxis.none;
    }
}