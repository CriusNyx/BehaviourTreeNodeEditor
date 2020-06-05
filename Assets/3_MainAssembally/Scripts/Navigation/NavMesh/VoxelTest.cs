using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class VoxelTest : MonoBehaviour
{
    Collider[] colliders;
    NavigationMesh mesh;

    public void Start()
    {
        mesh = gameObject.AddComponent<NavigationMesh>();

        foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
        {
            NavMeshTracker.Create(collider.gameObject, mesh);
        }

        //colliders = gameObject.GetComponentsInChildren<Collider>();
    }

    public void Update()
    {
        //foreach(var coll in colliders)
        //{
        //    foreach (var cell in mesh.GetCellsInBounds(coll.bounds, Quaternion.identity, true))
        //    {
        //        cell.Draw();
        //    }
        //}
    }
}
