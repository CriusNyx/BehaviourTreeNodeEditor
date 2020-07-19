using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a set of convex clipping planes
/// </summary>
public class ClippingPlaneSet : IEnumerable<MeshClippingPlane> 
{
    MeshClippingPlane[] planes; 

    public ClippingPlaneSet(params MeshClippingPlane[] planes)
    {
        this.planes = planes;
    }

    public ClippingPlaneSet(IEnumerable<MeshClippingPlane> planes) 
        : this(planes.ToArray())
    {
        
    }

    public ClippingPlaneSet(Matrix4x4 matrix, Mesh source)
    {
        UniqueList<(Vector3 normal, float distanceToOrigin)> uniqueList 
            = new UniqueList<(Vector3 normal, float distanceToOrigin)>(
                (x, y) =>
                {
                    if(Vector3.Dot(x.normal, y.normal) >= 1 - 0.0001f && Mathf.Abs(x.distanceToOrigin - y.distanceToOrigin) < 0.0001f)
                    {
                        return true;
                    }
                    return false;
                });

        for(int i = 0; i < source.triangles.Length; i += 3)
        {
            Vector3 a = matrix.MultiplyPoint(source.vertices[source.triangles[i]]);
            Vector3 b = matrix.MultiplyPoint(source.vertices[source.triangles[i + 1]]);
            Vector3 c = matrix.MultiplyPoint(source.vertices[source.triangles[i + 2]]);
            Vector3 normal = Vector3.Cross(b - a, c - b).normalized;
            float distanceToOrigin = Vector3.Dot(a, normal);
            uniqueList.TryAdd((normal, distanceToOrigin));
        }

        List<MeshClippingPlane> planes = new List<MeshClippingPlane>();

        foreach(var plane in uniqueList.Elements)
        {
            planes.Add(new MeshClippingPlane(plane.normal * plane.distanceToOrigin, plane.normal));
        }

        this.planes = planes.ToArray();
    }

    public IEnumerator<MeshClippingPlane> GetEnumerator()
    {
        return ((IEnumerable<MeshClippingPlane>)planes).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<MeshClippingPlane>)planes).GetEnumerator();
    }
}