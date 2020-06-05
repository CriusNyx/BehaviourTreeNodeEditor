using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindQuad : MonoBehaviour
{
    GameObject a;
    GameObject b;

    private void Start()
    {
        a = GameObjectFactory.Create("A", transform.position, transform.rotation, transform);
        b = GameObjectFactory.Create("B", transform.position, transform.rotation, transform);
    }

    private void Update()
    {
        var path = Navigation.GetPath(a.transform.position, b.transform.position);

        foreach(var element in path)
        {
            DebugDraw.Cross(element.center, Quaternion.identity, 1f);
        }

        foreach(var (a, b) in path.ForeachElementAndNext())
        {
            Debug.DrawLine(a.center, b.center);
        }

        var islandA = NavMeshCell.GetIslandFromPosition(NavigationMesh.mainMesh, a.transform.position);
        var islandB = NavMeshCell.GetIslandFromPosition(NavigationMesh.mainMesh, b.transform.position);

        if(islandA != null)
        {
            DebugDraw.Cross(islandA.center, Quaternion.identity, 1f, Color.red);
        }

        if (islandB != null)
        {
            DebugDraw.Cross(islandB.center, Quaternion.identity, 1f, Color.red);
        }
    }
}