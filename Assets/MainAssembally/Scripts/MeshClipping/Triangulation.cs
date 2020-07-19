using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using UnityEngine;

public static class Triangulation
{
    public static IEnumerable<(Vector3 a, Vector3 b, Vector3 c)> GetTriangulation((Vector3[] points, bool hole)[] shapes, Vector3 normal)
    {
        if (shapes.Length >= 1)
        {
            Vector3 offset = shapes[0].points[0];

            Quaternion rot = Quaternion.FromToRotation(normal, Vector3.back);
            Polygon polygon = new Polygon();
            foreach (var shape in shapes)
            {
                var vertices = shape.points.Select(x =>
                {
                    x = rot * (x - offset);
                    return new Vertex(x.x, x.y);
                });
                polygon.Add(new Contour(vertices), shape.hole);
            }

            var outputMesh = polygon.Triangulate();

            Quaternion inverseRot = Quaternion.Inverse(rot);

            Func<Vertex, Vector3> getVector = (x) => (inverseRot * new Vector3((float)x.X, (float)x.Y, 0f)) + offset;
            foreach (var tri in outputMesh.Triangles)
            {
                var vertex1 = tri.GetVertex(0);
                var vertex2 = tri.GetVertex(1);
                var vertex3 = tri.GetVertex(2);

                yield return (getVector(vertex1), getVector(vertex2), getVector(vertex3));
            }
        }
    }
}