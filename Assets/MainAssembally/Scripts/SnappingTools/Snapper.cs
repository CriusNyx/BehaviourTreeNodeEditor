using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Includes methods which calculate and do snapping for snap points.
/// </summary>
public class Snapper
{
    public readonly SnappingSetOwner handle;
    public readonly MeshFilter selectedFilter;
    public readonly int selectedIndex;
    public readonly (float distance, MeshFilter filter, int index)[] options;
    public int currentOption { get; private set; } = -1;

    public Snapper(SnappingSetOwner handle, MeshFilter selectedFilter, int selectedIndex)
    {
        this.handle = handle;
        this.selectedFilter = selectedFilter;
        this.selectedIndex = selectedIndex;

        var mesh = selectedFilter.sharedMesh;

        var (vertex, normal) = GetSnapPoint(selectedFilter, selectedIndex);

        List<(float distance, MeshFilter filter, int index)> options = new List<(float distance, MeshFilter filter, int index)>();


        foreach (SnappingSetOwner otherHandle in Object.FindObjectsOfType<SnappingSetOwner>())
        {
            if (otherHandle == handle)
                continue;

            foreach (SnappingSet set in otherHandle.GetComponentsInChildren<SnappingSet>())
            {
                foreach (MeshFilter otherFilter in set.GetComponentsInChildren<MeshFilter>())
                {
                    var otherMesh = otherFilter.sharedMesh;
                    for (int i = 0; i < otherMesh.vertexCount; i++)
                    {
                        var (otherVertex, otherNormal) = GetSnapPoint(otherFilter, i);

                        float distance = Vector3.Distance(vertex, otherVertex);
                        options.Add((distance, otherFilter, i));
                    }
                }
            }
        }

        this.options = options.OrderBy(x => x.distance).ToArray();
    }

    public static (Vector3 point, Vector3 normal) GetSnapPoint(MeshFilter filter, int index)
    {
        Mesh mesh = filter.sharedMesh;
        Matrix4x4 transform = filter.transform.localToWorldMatrix;

        return
            (transform.MultiplyPoint(mesh.vertices[index]),
            transform.MultiplyVector(mesh.normals[index]));
    }

    public void Itterate()
    {
        currentOption = (currentOption + 1) % options.Length;
    }

    public enum SnapMode
    {
        none = 0,
        position = 1 << 0,
        rotation = 1 << 1,
        both = position | rotation
    }

    public void Snap(GameObject gameObject, SnapMode snapMode)
    {
        var target = options[currentOption];
        Snap(gameObject, snapMode, selectedFilter, selectedIndex, target.filter, target.index);
    }

    public static void Snap(GameObject gameObject, SnapMode snapMode, MeshFilter sourceFilter, int sourceIndex, MeshFilter targetFilter, int targetIndex)
    {
        switch (snapMode)
        {
            case SnapMode.position:
                SnapPosition(gameObject, sourceFilter, sourceIndex, targetFilter, targetIndex);
                break;
            case SnapMode.rotation:
                SnapRotation(gameObject, sourceFilter, sourceIndex, targetFilter, targetIndex);
                break;
            case SnapMode.both:
                SnapPosition(gameObject, sourceFilter, sourceIndex, targetFilter, targetIndex);
                SnapRotation(gameObject, sourceFilter, sourceIndex, targetFilter, targetIndex);
                break;
        }
    }

    public static void SnapPosition(GameObject gameObject, MeshFilter sourceFilter, int sourceIndex, MeshFilter targetFilter, int targetIndex)
    {
        var sourcePoint = GetSnapPoint(sourceFilter, sourceIndex);
        var targetPoint = GetSnapPoint(targetFilter, targetIndex);

        Vector3 delta = targetPoint.point - sourcePoint.point;
        gameObject.transform.position += delta;
    }

    public static void SnapRotation(GameObject gameObject, MeshFilter sourceFilter, int sourceIndex, MeshFilter targetFilter, int targetIndex)
    {
        var sourcePoint = GetSnapPoint(sourceFilter, sourceIndex);
        var targetPoint = GetSnapPoint(targetFilter, targetIndex);

        Quaternion diff = Quaternion.FromToRotation(sourcePoint.normal, -targetPoint.normal);
        gameObject.transform.rotation = diff * gameObject.transform.rotation;

        var newSourcePoint = GetSnapPoint(sourceFilter, sourceIndex);
        Vector3 delta = sourcePoint.point - newSourcePoint.point;
        gameObject.transform.position += delta;
    }
}