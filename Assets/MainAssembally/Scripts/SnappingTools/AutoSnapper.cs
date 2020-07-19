using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutoSnapper : ISnapper
{
    public readonly SnappingSetOwner owner;
    public readonly float range;
    private (MeshFilter sourceFilter, int sourceIndex, MeshFilter targetFilter, int targetIndex)[] options;
    private int currentIndex = 0;

    public AutoSnapper(SnappingSetOwner owner, float range = 1f)
    {
        this.owner = owner;
        this.range = range;

        GenerateOptions();
    }

    private void GenerateOptions()
    {
        Bounds? myBounds = CalculateBounds(owner);

        if (myBounds == null)
        {
            options = new (MeshFilter sourceFilter, int sourceIndex, MeshFilter targetFilter, int targetIndex)[] { };
        }
        else
        {
            var validOwners = FindValidSetOwners(myBounds);

            List<(float distance, MeshFilter sourceFilter, int sourceIndex, MeshFilter targetFilter, int targetIndex)> options
                = new List<(float distance, MeshFilter sourceFilter, int sourceIndex, MeshFilter targetFilter, int targetIndex)>();

            foreach (var other in validOwners)
            {
                ProcessOptions(owner, other, options);
            }

            this.options =
                options
                .OrderBy(x => x.distance)
                .Select(x => (x.sourceFilter, x.sourceIndex, x.targetFilter, x.targetIndex))
                .ToArray();
        }
    }

    private void ProcessOptions(
        SnappingSetOwner a,
        SnappingSetOwner b,
        List<(float distance, MeshFilter sourceFilter, int sourceIndex, MeshFilter targetFilter, int targetIndex)> options)
    {
        foreach (SnappingSet setA in a.GetComponentsInChildren<SnappingSet>())
        {
            foreach (MeshFilter filterA in setA.GetComponentsInChildren<MeshFilter>())
            {
                foreach (SnappingSet setB in b.GetComponentsInChildren<SnappingSet>())
                {
                    foreach (MeshFilter filterB in setB.GetComponentsInChildren<MeshFilter>())
                    {
                        var matrixA = filterA.transform.localToWorldMatrix;
                        var matrixB = filterB.transform.localToWorldMatrix;

                        var meshA = filterA.sharedMesh;
                        var meshB = filterB.sharedMesh;
                        var verticesA = meshA.vertices;
                        var verticesB = meshB.vertices;
                        var normalsA = meshA.normals;
                        var normalsB = meshB.normals;

                        for (int indexA = 0; indexA < verticesA.Length; indexA++)
                        {
                            for (int indexB = 0; indexB < verticesB.Length; indexB++)
                            {
                                Vector3 vertexAWorldSpace = matrixA.MultiplyPoint(verticesA[indexA]);
                                Vector3 vertexBWorldSpace = matrixB.MultiplyPoint(verticesB[indexB]);

                                Vector3 normalAWorldSpace = matrixA.MultiplyVector(normalsA[indexA]);
                                Vector3 normalBWorldSpace = matrixB.MultiplyVector(normalsB[indexB]);

                                float normalPenalty = 1f - Vector3.Dot(normalAWorldSpace, -normalBWorldSpace);
                                normalPenalty = normalPenalty / 2f + 1f;


                                float distance = Vector3.Distance(vertexAWorldSpace, vertexBWorldSpace);
                                if (distance <= range)
                                {
                                    options.Add((distance + normalPenalty, filterA, indexA, filterB, indexB));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private List<SnappingSetOwner> FindValidSetOwners(Bounds? myBounds)
    {
        List<SnappingSetOwner> validOwners = new List<SnappingSetOwner>();

        foreach (var other in Object.FindObjectsOfType<SnappingSetOwner>())
        {
            if (other == owner)
            {
                continue;
            }
            else
            {
                var otherBounds = CalculateBounds(other);
                if (otherBounds != null)
                {
                    Vector3 sourcePoint = myBounds.Value.ClosestPoint(otherBounds.Value.center);
                    Vector3 targetPoint = otherBounds.Value.ClosestPoint(sourcePoint);
                    float distance = Vector3.Distance(sourcePoint, targetPoint);
                    if (distance <= range)
                    {
                        validOwners.Add(other);
                    }
                }
            }
        }
        return validOwners;
    }

    private static Bounds? CalculateBounds(SnappingSetOwner owner)
    {
        Bounds? output = null;
        foreach (SnappingSet set in owner.GetComponentsInChildren<SnappingSet>())
        {
            foreach (MeshFilter filter in set.GetComponentsInChildren<MeshFilter>())
            {
                float diameter = Vector3.Distance(filter.mesh.bounds.max, filter.mesh.bounds.min);
                Vector3 center = filter.mesh.bounds.center;
                Bounds meshBounds = new Bounds(center, Vector3.one * diameter);
                if (output == null)
                {
                    output = meshBounds;
                }
                else
                {
                    var newBounds = output.Value;
                    newBounds.Encapsulate(meshBounds);
                    output = newBounds;
                }
            }
        }
        return output;
    }

    public void Itterate()
    {
        if (options.Length == 0)
        {
            currentIndex = -1;
        }
        else
        {
            currentIndex = (currentIndex + 1) % options.Length;
        }
    }

    public void Snap(GameObject gameObject, Snapper.SnapMode snapMode)
    {
        if (currentIndex >= 0 && currentIndex < options.Length)
        {
            var (filterA, indexA, filterB, indexB) = options[currentIndex];
            Snapper.Snap(gameObject, snapMode, filterA, indexA, filterB, indexB);
        }
    }
}
