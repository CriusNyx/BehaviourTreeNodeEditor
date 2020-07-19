using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an isolated navigation space within a nav cell
/// </summary>
public class NavigationIsland
{
    public Vector3 center;
    public List<NavigationIsland> links = new List<NavigationIsland>();
    public readonly NavMeshCell cell;

    public NavigationIsland(NavMeshCell cell)
    {
        this.cell = cell;
    }

    public IEnumerable<NavigationIsland> GetConnectedIslands() => cell.GetConnectedIslands(this);
}