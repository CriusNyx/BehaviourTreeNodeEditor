using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using UnityEngine;

public class LibTest : MonoBehaviour
{
    public void Start()
    {
        Polygon polygon = new Polygon();
        polygon.Add(new Contour(new Vertex[]
        {
            new Vertex(-0.5f, -0.5f),
            new Vertex(-0.5f, 0.5f),
            new Vertex(0.5f, 0.5f),
            new Vertex(0.5f, -0.5f)
        }));

        polygon.Add(new Contour(new Vertex[]
        {
            new Vertex(1.5f, -0.5f),
            new Vertex(1.5f, 0.5f),
            new Vertex(2.5f, 0.5f),
            new Vertex(2.5f, -0.5f)
        }));

        var mesh = polygon.Triangulate();
        foreach(var tri in mesh.Triangles)
        {
            var a = tri.GetVertex(0);
            var b = tri.GetVertex(1);
            var c = tri.GetVertex(2);
            Vector3 aPos = new Vector3((float)a.X, (float)a.Y, 0f);
            Vector3 bPos = new Vector3((float)b.X, (float)b.Y, 0f);
            Vector3 cPos = new Vector3((float)c.X, (float)c.Y, 0f);

            Debug.DrawLine(aPos, bPos);
            Debug.DrawLine(bPos, cPos);
            Debug.DrawLine(cPos, aPos);
        }
        Debug.Break();
    }
}