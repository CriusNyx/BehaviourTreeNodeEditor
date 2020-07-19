using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Navigation
{
    public static NavigationIsland[] GetPath(Vector3 a, Vector3 b)
    {
        NavigationIsland start = NavMeshCell.GetIslandFromPosition(NavigationMesh.mainMesh, a);
        if(start == null)
        {
            return new NavigationIsland[] { };
        }

        NavigationIsland end = NavMeshCell.GetIslandFromPosition(NavigationMesh.mainMesh, b);
        if(end == null)
        {
            return new NavigationIsland[] { };
        }

        var output = AStar.FindRoute(
            start, 
            end, 
            (x, y) => Vector3.Distance(x.center, y.center), 
            (x, y) => Vector3.Distance(x.center, y.center), 
            x => x.GetConnectedIslands());

        return output.ToArray();
    }
}
