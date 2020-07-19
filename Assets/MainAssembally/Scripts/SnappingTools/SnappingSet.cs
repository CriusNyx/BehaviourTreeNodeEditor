using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[BlenderComponent("SnappingSet", nameof(OnImportAsset))]
public class SnappingSet : MonoBehaviour
{
    private void OnImportAsset(string propertyName, object propertyValue)
    {
        var root = gameObject.transform.root.gameObject;
        var owner = root.GetComponent<SnappingSetOwner>();
        if (!owner)
        {
            owner = root.AddComponent<SnappingSetOwner>();
        }

        Process(gameObject);
    }

    private void Process(GameObject gameObject)
    {
        var renderer = gameObject.GetComponent<MeshRenderer>();
        if (renderer)
        {
            DestroyImmediate(renderer);
        }
        var collider = gameObject.GetComponent<Collider>();
        if (collider)
        {
            DestroyImmediate(collider);
        }

        foreach(Transform child in gameObject.transform)
        {
            Process(child.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        return;
        foreach(var filter in gameObject.GetComponentsInChildren<MeshFilter>())
        {
            var mesh = filter.sharedMesh;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Vector3 vertex = mesh.vertices[i];
                Vector3 normal = mesh.normals[i];
                Vector3 worldSpaceVertex = filter.transform.localToWorldMatrix.MultiplyPoint(vertex);
                Vector3 worldSpaceNormal = filter.transform.localToWorldMatrix.MultiplyVector(normal);
#if UNITY_EDITOR
                Handles.DrawWireDisc(worldSpaceVertex, worldSpaceNormal, 0.05f);
#endif
                Gizmos.DrawLine(worldSpaceVertex, worldSpaceVertex + worldSpaceNormal * 0.1f);

            }
        }
    }
}
