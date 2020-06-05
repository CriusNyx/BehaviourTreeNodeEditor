using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigableObject : MonoBehaviour
{
    private List<NavMeshCell> containingCells = new List<NavMeshCell>();
    public void Invalidate()
    {
        foreach(var cell in containingCells)
        {
            cell.Invalidate();
        }
        containingCells = new List<NavMeshCell>();
        var collider = gameObject.GetComponent<Collider>();
        foreach(var cell in NavMeshCell.GetCellsInBounds(NavigationMesh.mainMesh, collider.bounds))
        {
            containingCells.Add(cell);
            cell.Invalidate();
        }
    }
}