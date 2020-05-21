using UnityEngine;

public class NavQuad
{
    public readonly Vector3 
        position, 
        scale;

    public readonly Quaternion rotation;

    public NavQuad(Vector3 position, Vector3 scale, Quaternion rotation)
    {
        this.position = position;
        scale.x = Mathf.Abs(scale.x);
        scale.y = Mathf.Abs(scale.y);
        scale.z = Mathf.Abs(scale.z);
        this.scale = scale;
        this.rotation = rotation;
    }

    public void DrawGizmos()
    {
        Gizmos.DrawWireCube(position, scale);
    }

    public static bool IsOverlappingVertical(NavQuad a, NavQuad b, float widthOverlap, float heightOverlap)
    {
        Vector3 aHalf = a.scale * 0.5f;
        Vector3 bHalf = b.scale * 0.5f;

        Vector3 aMin = a.position - aHalf;
        Vector3 aMax = a.position + aHalf;

        Vector3 bMin = b.position - bHalf;
        Vector3 bMax = b.position + bHalf;

        // check y
        if((aMax.y - bMin.y) >= heightOverlap && (bMax.y - aMin.y) >= heightOverlap)
        {
            // check x or z
            if(a.scale.x > a.scale.z && b.scale.x > b.scale.z)
            {
                return (aMax.x - bMin.x) >= widthOverlap && (bMax.x - aMin.x) >= widthOverlap;
            }
            else if(a.scale.z > a.scale.x && b.scale.z > b.scale.x)
            {
                return (aMax.z - bMin.z) >= widthOverlap && (bMax.z - aMin.z) >= widthOverlap;
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }
        else
        {
            return false;
        }
    }
}
