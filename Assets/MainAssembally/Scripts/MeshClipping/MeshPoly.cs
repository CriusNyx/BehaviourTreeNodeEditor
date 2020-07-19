using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshPoly
{
    public readonly List<IMeshVertex> vertices = new List<IMeshVertex>();

    public MeshPoly()
    {

    }

    public MeshPoly(IEnumerable<IMeshVertex> vertices)
    {
        this.vertices = vertices.ToList();
    }

    public void Draw(float time = -1f) => Draw(Color.white, time);

    public void Draw(Color color, float time = -1f)
    {
        foreach((var a, var b) in vertices.ForeachElementAndNextCircular())
        {
            Debug.DrawLine(a.position, b.position, color, time);
        }
    }

    public static Mesh ContructMesh(IEnumerable<MeshPoly> polys, Mesh source)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Color> colors = new List<Color>();
        List<int> indices = new List<int>();

        bool hasNormals = true;
        bool hasTangents = true;
        bool hasColors = true;

        foreach(var poly in polys)
        {
            int startIndex = vertices.Count;
            foreach (var vertex in poly.vertices)
            {
                vertices.Add(vertex.position);
                uvs.Add(vertex.uv);
                if (vertex.normal != null)
                {
                    normals.Add(vertex.normal.Value);
                }
                else
                {
                    hasNormals = false;
                }
                if (vertex.tangent != null)
                {
                    tangents.Add(vertex.tangent.Value);
                }
                else
                {
                    hasTangents = false;
                }
                if(vertex.color != null)
                {
                    colors.Add(vertex.color.Value);
                }
                else
                {
                    hasColors = false;
                }
            }
            for(int i = 1; i < poly.vertices.Count - 1; i++)
            {
                indices.Add(startIndex);
                indices.Add(startIndex + i);
                indices.Add(startIndex + i + 1);
            }
        }

        if(source == null)
        {
            source = new Mesh();
        }
        else
        {
            source.Clear();
        }

        source.vertices = vertices.ToArray();
        source.uv = uvs.ToArray();
        source.triangles = indices.ToArray();

        if (hasNormals)
            source.normals = normals.ToArray();
        if (hasTangents)
            source.tangents = tangents.ToArray();
        if (hasColors)
            source.colors = colors.ToArray();

        return source;
    }

    public Vector3 Normal
    {
        get
        {
            if(vertices.Count < 3)
            {
                throw new System.InvalidOperationException("Not enough vertices to compute the normal.");
            }
            else
            {
                return Vector3.Normalize(
                    Vector3.Cross(
                        vertices[1].position - vertices[0].position, 
                        vertices[2].position - vertices[1].position));
            }
        }
    }
}