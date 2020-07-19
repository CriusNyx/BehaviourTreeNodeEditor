using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshVertex : IMeshVertex
{
    Mesh mesh;
    int index;

    public MeshVertex(Mesh mesh, int index)
    {
        if (mesh == null)
            throw new ArgumentException("Mesh cannot be null");
        if (!mesh.vertices.IsInBounds(index))
            throw new IndexOutOfRangeException();

        this.mesh = mesh;
        this.index = index;
    }

    public Vector3 position => mesh.vertices[index];

    public Vector2 uv => mesh.uv[index];

    public Vector3? normal => mesh?.normals[index];

    public Vector4? tangent => mesh?.tangents[index];

    public Color? color
    {
        get
        {
            if (mesh.colors == null)
            {
                return null;
            }
            else if (!mesh.colors.IsInBounds(index))
            {
                return null;
            }
            else
            {
                return mesh.colors[index];
            }
        }
    }
}