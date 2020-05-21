using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDraw
{
    public static void Box(Vector3 position, Quaternion rotation, Vector3 scale, float time = -1f)
        => Box(position, rotation, scale, Color.white, time);

    public static void Box(Vector3 position, Quaternion rotation, Vector3 scale, Color color, float time = -1f)
    {
        scale = scale / 2f;

        Vector3[,,] points = new Vector3[2, 2, 2];

        for(int x = 0; x <= 1; x++)
            for(int y = 0; y <= 1; y++)
                for(int z = 0; z <= 1; z++)
                {
                    float xSign = x == 0 ? -1f : 1f;
                    float ySign = y == 0 ? -1f : 1f;
                    float zSign = z == 0 ? -1f : 1f;

                    points[x, y, z] = position + rotation * Vector3.Scale(scale, new Vector3(xSign, ySign, zSign));
                }

        Debug.DrawLine(points[0, 0, 0], points[1, 0, 0], color, time);
        Debug.DrawLine(points[0, 1, 0], points[1, 1, 0], color, time);
        Debug.DrawLine(points[0, 0, 1], points[1, 0, 1], color, time);
        Debug.DrawLine(points[0, 1, 1], points[1, 1, 1], color, time);

        Debug.DrawLine(points[0, 0, 0], points[0, 1, 0], color, time);
        Debug.DrawLine(points[1, 0, 0], points[1, 1, 0], color, time);
        Debug.DrawLine(points[0, 0, 1], points[0, 1, 1], color, time);
        Debug.DrawLine(points[1, 0, 1], points[1, 1, 1], color, time);

        Debug.DrawLine(points[0, 0, 0], points[0, 0, 1], color, time);
        Debug.DrawLine(points[1, 0, 0], points[1, 0, 1], color, time);
        Debug.DrawLine(points[0, 1, 0], points[0, 1, 1], color, time);
        Debug.DrawLine(points[1, 1, 0], points[1, 1, 1], color, time);
    }

    public static void Cross(Vector3 position, Quaternion rotation, float size, float time = -1f)
        => Cross(position, rotation, size, Color.white, time);

    public static void Cross(Vector3 position, Quaternion rotation, float size, Color color, float time = -1f)
    {
        Debug.DrawLine(
            position - rotation * Vector3.up * size / 2f,
            position + rotation * Vector3.up * size / 2f,
            color,
            time);

        Debug.DrawLine(
            position - rotation * Vector3.right * size / 2f,
            position + rotation * Vector3.right * size / 2f,
            color,
            time);

        Debug.DrawLine(
            position - rotation * Vector3.forward * size / 2f,
            position + rotation * Vector3.forward * size / 2f,
            color,
            time);
    }

    public static void LineLerp(Vector3 start, Vector3 end, float t, float time = -1f)
        => LineLerp(start, end, t, Color.white, time);

    public static void LineLerp(Vector3 start, Vector3 end, float t, Color color, float time = -1f)
    {
        Debug.DrawLine(start, Vector3.Lerp(start, end, t), color, time);
    }
}
