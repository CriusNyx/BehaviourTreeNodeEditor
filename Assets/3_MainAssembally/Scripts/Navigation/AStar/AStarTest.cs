using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarTest : MonoBehaviour
{
    List<AStarTestNode> nodeList = new List<AStarTestNode>();
    IEnumerable<AStarTestNode> solution;

    public int size = 50;
    public int removeCount = 10;

    private void Start()
    {
        //create nodes
        AStarTestNode[,] nodes = new AStarTestNode[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var gameObject = new GameObject($"node({i}, {j})");
                gameObject.transform.position = new Vector3(i, 0f, j);

                var node = gameObject.AddComponent<AStarTestNode>();
                nodes[i, j] = node;
                nodeList.Add(node);
            }
        }

        //Remove random nodes
        for(int i = 0; i < removeCount; i++)
        {
            int x = Random.Range(0, size);
            int y = Random.Range(0, size);

            var node = nodes[x, y];
            if(node != null)
            {
                nodeList.Remove(node);
                Destroy(node.gameObject);
                nodes[x, y] = null;
            }
        }

        //link nodes
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                for (int iOffset = -1; iOffset <= 1; iOffset++)
                    for (int jOffset = -1; jOffset <= 1; jOffset++)
                    {
                        var node = nodes[i, j];

                        if(node == null)
                        {
                            continue;
                        }

                        int iPlus = i + iOffset;
                        int jPlus = j + jOffset;
                        if (iPlus >= 0 && jPlus >= 0 && iPlus < size && jPlus < size)
                        {
                            var adjacent = nodes[iPlus, jPlus];
                            if (adjacent != null)
                                node.adjacent.Add(adjacent);
                        }
                    }
    }

    private void Update()
    {
        foreach (var node in nodeList)
        {
            node.DrawLinks();
        }


        var start = nodeList[Random.Range(0, nodeList.Count)];
        var end = nodeList[Random.Range(0, nodeList.Count)];

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        solution = AStar.FindRoute(
                start,
                end,
                (x, y) => Vector3.Distance(x.transform.position, y.transform.position),
                (x, y) => Vector3.Distance(x.transform.position, y.transform.position),
                x => x.adjacent);

        stopwatch.Stop();

        Debug.Log(stopwatch.ElapsedMilliseconds);

        Debug.DrawRay(start.transform.position, Vector3.up, Color.green);
        Debug.DrawRay(end.transform.position, Vector3.up, Color.green);

        foreach ((var a, var b) in solution.ForeachElementAndNext())
        {
            Debug.DrawLine(a.transform.position, b.transform.position, Color.red);
        }
    }
}