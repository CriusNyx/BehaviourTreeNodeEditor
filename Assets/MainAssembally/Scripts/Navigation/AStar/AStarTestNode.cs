using System.Collections.Generic;
using UnityEngine;

public class AStarTestNode : MonoBehaviour
{
    public List<AStarTestNode> adjacent = new List<AStarTestNode>();

    public void DrawLinks()
    {
        foreach(var adj in adjacent)
        {
            Vector3 targetPoint = Vector3.Lerp(transform.position, adj.transform.position, 0.4f);
            Debug.DrawLine(transform.position, targetPoint);
        }
    }
}