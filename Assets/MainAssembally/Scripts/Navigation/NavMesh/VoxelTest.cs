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
            AddTracker(collider.transform);
        }

        //colliders = gameObject.GetComponentsInChildren<Collider>();
    }

    private void AddTracker(Transform target)
    {
        if(target != transform)
        {
            if(target.GetComponent<NavMeshTracker>() == null)
            {
                NavMeshTracker.Create(target.gameObject, mesh);
                AddTracker(target.transform.parent);
            }
        }
    }
}
