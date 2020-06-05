using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static MeshClippingPlane;

public static class MeshClipper
{
    /// <summary>
    /// Create a task to clip a new mesh.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="clippingPlaneSet"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Task<Mesh> Clip(Mesh mesh, ClippingPlaneSet clippingPlaneSet, Matrix4x4 worldToMeshSpace, Mesh source = null)
    {
        return ClipUnpack(mesh, clippingPlaneSet, worldToMeshSpace).ThenOnMainThread((x) => MeshPoly.ContructMesh(x, source));
    }

    /// <summary>
    /// Preform the first step of clipping, which is unpacking the mesh on the main thread
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="clippingPlaneSet"></param>
    /// <returns></returns>
    private static Task<List<MeshPoly>> ClipUnpack(Mesh mesh, ClippingPlaneSet clippingPlaneSet, Matrix4x4 worldToMeshSpace)
    {
        List<MeshPoly> output = new List<MeshPoly>();

        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] triangles = mesh.triangles;
        Vector3[] normals = mesh.normals;
        Vector4[] tangents = mesh.tangents;
        Color[] colors = mesh.colors;

        return Task.Run(() =>
        {
            return ClipEachPolygonByEachClippingPlane(
                ArrayToPolys(vertices, uvs, triangles, normals, tangents, colors),
                clippingPlaneSet, worldToMeshSpace);
        });
    }

    /// <summary>
    /// Convert arrays of unpacked mesh data into polys that can be clipped
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="uvs"></param>
    /// <param name="triangles"></param>
    /// <param name="normals"></param>
    /// <param name="tangents"></param>
    /// <param name="colors"></param>
    /// <returns></returns>
    private static List<MeshPoly> ArrayToPolys(
        Vector3[] vertices,
        Vector2[] uvs,
        int[] triangles,
        Vector3[] normals,
        Vector4[] tangents,
        Color[] colors)
    {
        List<MeshPoly> output = new List<MeshPoly>();
        for (int i = 0; i < triangles.Length / 3; i++)
        {
            int j = i * 3;
            var newPoly = new MeshPoly();

            newPoly.vertices.Add(GetVertexFromArray(triangles[j], vertices, uvs, normals, tangents, colors));
            newPoly.vertices.Add(GetVertexFromArray(triangles[j + 1], vertices, uvs, normals, tangents, colors));
            newPoly.vertices.Add(GetVertexFromArray(triangles[j + 2], vertices, uvs, normals, tangents, colors));

            output.Add(newPoly);
        }

        return output;
    }

    /// <summary>
    /// Get a single vertex from the arrays
    /// </summary>
    /// <param name="index"></param>
    /// <param name="vertices"></param>
    /// <param name="uvs"></param>
    /// <param name="normals"></param>
    /// <param name="tangents"></param>
    /// <param name="colors"></param>
    /// <returns></returns>
    private static IMeshVertex GetVertexFromArray(
        int index,
        Vector3[] vertices,
        Vector2[] uvs,
        Vector3[] normals,
        Vector4[] tangents,
        Color[] colors)
    {
        Vector3 vertex = vertices[index];
        Vector2 uv = uvs[index];
        Vector3? normal = normals.IsInBounds(index) ? normals?[index] : null;
        Vector4? tangent = tangents.IsInBounds(index) ? tangents?[index] : null;
        Color? color = colors.IsInBounds(index) ? colors?[index] : null;

        return new ArbitraryVertex(vertex, uv, normal, tangent, color);
    }

    /// <summary>
    /// Clips each polygon to each clipping plane
    /// </summary>
    /// <param name="polys"></param>
    /// <param name="clippingPlaneSet"></param>
    /// <returns></returns>
    private static List<MeshPoly> ClipEachPolygonByEachClippingPlane(
        List<MeshPoly> polys,
        ClippingPlaneSet clippingPlaneSet,
        Matrix4x4 worldToMeshSpace)
    {
        try
        {
            foreach (var plane in clippingPlaneSet)
            {
                polys = ClipEachPolyByClippingPlane(polys, plane, worldToMeshSpace);
            }

            return polys;
        }
        catch (Exception e)
        {
            MainThreadDispatcher.Dispatch(() => { Debug.LogError(e); });
            throw e;
        }
    }

    private static List<MeshPoly> ClipEachPolyByClippingPlane(List<MeshPoly> polys, MeshClippingPlane plane, Matrix4x4 worldToMeshSpace)
    {
        List<MeshPoly> output = new List<MeshPoly>();

        List<(Vector3 a, Vector3 b)> newEdges = new List<(Vector3 a, Vector3 b)>();

        foreach (var poly in polys)
        {
            var newPoly = ClipPolyByPlane(poly, plane, worldToMeshSpace, newEdges);
            if (newPoly.vertices.Count >= 3)
            {
                output.Add(newPoly);
            }
        }

        output.AddRange(ConstructPolysFromEdges(newEdges, worldToMeshSpace.MultiplyVector(plane.normal)));

        return output;
    }

    private static MeshPoly ClipPolyByPlane(MeshPoly poly, MeshClippingPlane plane, Matrix4x4 worldToMeshSpace, List<(Vector3 a, Vector3 b)> newEdges)
    {
        MeshPoly newPoly = new MeshPoly();

        Vector3? 
            newPointA = null, 
            newPointB = null, 
            newPointC = null;

        IntersectionDirection? firstIntersectionDirection = null;

        foreach ((var a, var b) in poly.vertices.ForeachElementAndNextCircular())
        {
            var intersection = plane.IsIntersecting(a.position, b.position, worldToMeshSpace);
            var (type, direction, tValue) = intersection;

            float aDistance = plane.DistanceToPoint(a.position, worldToMeshSpace);
            if(aDistance < 0f)
            {
                newPoly.vertices.Add(a);
            }

            switch (type)
            {
                case (IntersectionType.touching):
                    newPoly.vertices.Add(a);
                    firstIntersectionDirection = MarkDirection(firstIntersectionDirection, direction);
                    SortNewPosition(a.position, ref newPointA, ref newPointB, ref newPointC);
                    break;

                case (IntersectionType.crossing):
                    var newVertex = ArbitraryVertex.Lerp(a, b, tValue);
                    newPoly.vertices.Add(newVertex);
                    firstIntersectionDirection = MarkDirection(firstIntersectionDirection, direction);
                    SortNewPosition(newVertex.position, ref newPointA, ref newPointB, ref newPointC);
                    break;
            }
        }

        if(newPointA != null && newPointB != null)
        {
            if (newPointC == null)
            {
                if (firstIntersectionDirection == IntersectionDirection.intoPlane)
                {
                    newEdges.Add((newPointA.Value, newPointB.Value));
                }
                else if (firstIntersectionDirection == IntersectionDirection.outofPlane)
                {
                    newEdges.Add((newPointB.Value, newPointA.Value));
                }
            }
            else
            {
                if(Vector3.Dot(plane.normal, poly.Normal) > 0)
                {
                    newEdges.Add((newPointA.Value, newPointB.Value));
                    newEdges.Add((newPointB.Value, newPointC.Value));
                    newEdges.Add((newPointC.Value, newPointA.Value));
                }
                else
                {
                    newEdges.Add((newPointB.Value, newPointA.Value));
                    newEdges.Add((newPointC.Value, newPointB.Value));
                    newEdges.Add((newPointA.Value, newPointC.Value));
                }
            }
        }

        return newPoly;
    }

    private static IntersectionDirection? 
        MarkDirection(IntersectionDirection? firstIntersectionDirection, IntersectionDirection direction)
    {
        if (firstIntersectionDirection == null
            && (direction == IntersectionDirection.intoPlane || direction == IntersectionDirection.outofPlane))
        {
            firstIntersectionDirection = direction;
        }

        return firstIntersectionDirection;
    }

    private static void SortNewPosition(
        Vector3 position,
        ref Vector3? newPointA,
        ref Vector3? newPointB,
        ref Vector3? newPointC)
    {
        if (newPointA == null)
        {
            newPointA = position;
        }
        else if (newPointB == null)
        {
            newPointB = position;
        }
        else if(newPointC == null)
        {
            newPointC = position;
        }
    }

    private static IEnumerable<MeshPoly> ConstructPolysFromEdges(
        List<(Vector3 a, Vector3 b)> newEdges,
        Vector3 normal,
        float resolution = 0.000001f)
    {
        List<(Vector3[] points, bool hole)> contours = new List<(Vector3[] points, bool hole)>();

        VoxelSet<(Vector3 a, Vector3 b)> set = new VoxelSet<(Vector3 a, Vector3 b)>(resolution, Vector3.zero);

        foreach (var edge in newEdges)
        {
            if (!set.ContainsKey(edge.a))
            {
                set.Add(edge.a, edge);
            }
        }

        Queue<(Vector3 a, Vector3 b)> queue = new Queue<(Vector3 a, Vector3 b)>(newEdges);

        while (queue.Count > 0)
        {
            var next = queue.Dequeue();

            List<Vector3> points = new List<Vector3>();

            if (set.ContainsKey(next.a))
            {
                points.Add(next.a);
                set.Remove(next.a);
                while (set.TryGetValueOrAdjacent(next.b, out next))
                {
                    points.Add(next.a);
                    set.Remove(next.a);
                }
            }

            if(points.Count >= 3)
            {
                Vector3 newNormal = Vector3.Normalize(Vector3.Cross(points[1] - points[0], points[2] - points[1]));
                bool isHole = Mathf.Sign(Vector3.Dot(newNormal, normal)) == -1;
                contours.Add((points.ToArray(), isHole));
            }
        }

        var triangulation = Triangulation.GetTriangulation(contours.ToArray(), normal);

        List<MeshPoly> output = new List<MeshPoly>();

        foreach(var (a, b, c) in triangulation)
        {

            MeshPoly outputPoly = new MeshPoly();
            outputPoly.vertices.Add(new ArbitraryVertex(a, Vector2.zero, normal));
            outputPoly.vertices.Add(new ArbitraryVertex(c, Vector2.zero, normal));
            outputPoly.vertices.Add(new ArbitraryVertex(b, Vector2.zero, normal));

            output.Add(outputPoly);
        }

        return output;
    }
}
