using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawNormalsAndTangents : MonoBehaviour
{
    private void Update()
    {
        foreach(var filter in gameObject.GetComponentsInChildren<MeshFilter>())
        {
            var mesh = filter.sharedMesh;
            Matrix4x4 localToWorld = filter.transform.localToWorldMatrix;
            for(int i = 0; i < mesh.vertexCount; i++)
            {
                Vector3 vertex = localToWorld.MultiplyPoint(mesh.vertices[i]);
                Vector3 normal = localToWorld.MultiplyVector(mesh.normals[i]);
                Vector3 tangentDir = localToWorld.MultiplyPoint(mesh.tangents[i]);
                Debug.DrawRay(vertex, normal * 0.1f, Color.white);


                if(mesh.tangents[i].w > 0)
                {
                    Debug.DrawRay(vertex, tangentDir * 0.1f, Color.green);
                }
                else
                {
                    Debug.DrawRay(vertex, tangentDir * 0.1f, Color.red);
                }
            }
        }
    }
}