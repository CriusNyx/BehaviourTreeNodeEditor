using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArbitraryVertex : IMeshVertex
{
    public ArbitraryVertex(
        Vector3 position, 
        Vector2 uv, 
        Vector3? normal = null, 
        Vector4? tangent = null, 
        Color? color = null)
    {
        this.position = position;
        this.uv = uv;
        this.normal = normal;
        this.tangent = tangent;
        this.color = color;
    }

    public ArbitraryVertex(
        IMeshVertex vertex)
    {
        this.position = vertex.position;
        this.uv = vertex.uv;
        this.normal = vertex.normal;
        this.tangent = vertex.tangent;
        this.color = vertex.color;
    }

    public Vector3 position { get; set; }
    public Vector2 uv { get; set; }
    public Vector3? normal { get; set; }
    public Vector4? tangent { get; set; }
    public Color? color { get; set; }

    public static ArbitraryVertex Lerp(IMeshVertex a, IMeshVertex b, float t)
    {
        Vector3 position = Vector3.Lerp(a.position, b.position, t);
        Vector2 uv = Vector3.Lerp(a.uv, b.uv, t);

        Vector3? normal = null;
        Vector4? tangent = null;
        Color? color = null;

        if(a.normal != null && b.normal != null)
        {
            normal = Vector3.Normalize(Vector3.Lerp(a.normal.Value, b.normal.Value, t));
        }
        if(a.tangent != null && b.tangent != null)
        {
            Vector3 tanDir = Vector3.Lerp(a.tangent.Value, b.tangent.Value, t);
            tangent = new Vector4(tanDir.x, tanDir.y, tanDir.z, a.tangent.Value.w);
        }
        if(a.color != null && b.color != null)
        {
            color = Color.Lerp(a.color.Value, b.color.Value, t);
        }

        return new ArbitraryVertex(position, uv, normal, tangent, color);
    }
}