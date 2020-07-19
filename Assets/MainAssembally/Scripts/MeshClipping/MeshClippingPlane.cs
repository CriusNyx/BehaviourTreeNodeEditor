using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public struct MeshClippingPlane
{
    public Vector3 position, normal;

    public MeshClippingPlane(Vector3 position, Vector3 normal)
    {
        this.position = position;
        this.normal = normal;
    }

    public (IntersectionType type, IntersectionDirection direction, float tValue)
        IsIntersecting(Vector3 a, Vector3 b, Matrix4x4 worldToMeshSpace)
    {
        float aValue = DistanceToPoint(a, worldToMeshSpace);
        float bValue = DistanceToPoint(b, worldToMeshSpace);

        if(aValue == 0)
        {
            if(bValue == 0)
            {
                return (IntersectionType.touching, IntersectionDirection.onPlaneSurface, 0f);
            }
            else if(bValue < 0)
            {
                return (IntersectionType.touching, IntersectionDirection.intoPlane, 0f);
            }
            else if(bValue > 0)
            {
                return (IntersectionType.touching, IntersectionDirection.outofPlane, 0f);
            }
        }
        else if(bValue == 0)
        {
            return (IntersectionType.none, IntersectionDirection.none, -1f);
        }
        else if(Sign(aValue * bValue) == -1)
        {
            float tValue = Abs(aValue) / (Abs(aValue) + Abs(bValue));
            if(bValue < 0)
            {
                return (IntersectionType.crossing, IntersectionDirection.intoPlane, tValue);
            }
            else if(bValue > 0)
            {
                return (IntersectionType.crossing, IntersectionDirection.outofPlane, tValue);
            }
        }
        else
        {
            return (IntersectionType.none, IntersectionDirection.none, -1f);
        }
        throw new System.NotImplementedException(
            "The arguments presented did not match any known intersection types. Check Method Logic.");
    }

    public float DistanceToPoint(Vector3 point, Matrix4x4 worldToMeshSpace)
    {
        return Vector3.Dot(point - worldToMeshSpace.MultiplyPoint(position), worldToMeshSpace.MultiplyVector(normal));
    }

    public enum IntersectionType
    {
        none = 0,
        touching = 0b1,
        crossing = 0b10
    }

    public enum IntersectionDirection
    {
        none = 0,
        intoPlane = 0b1,
        outofPlane = 0b10,
        onPlaneSurface = 0b100,
    }
}