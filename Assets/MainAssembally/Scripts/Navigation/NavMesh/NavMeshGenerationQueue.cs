using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshGenerationQueue : MonoBehaviour
{
    private readonly object queueLock = new object();
    HashSet<NavCellController> queuedCells = new HashSet<NavCellController>();
    PriorityQueue<float, NavCellController> queue
        = new PriorityQueue<float, NavCellController>();

    NavMeshGenerationSettings navMeshGenerationSettings 
        = new NavMeshGenerationSettings(new VoxelOrientation(0.25f, Vector3.zero), 8, 8, 2, 4);

    private bool QueueHasElements
    {
        get
        {
            lock (queueLock)
            {
                return queue.Count > 0;
            }
        }
    }

    private static NavMeshGenerationQueue instance;

    private bool isRunning = false;

    [RuntimeInitializeOnLoadMethod]
    private static void CreateInstance()
    {
        instance = new GameObject("Nav Mesh Generation Queue").AddComponent<NavMeshGenerationQueue>();
    }

    void Update()
    {
        if (!isRunning)
        {
            if (QueueHasElements)
            {
                StartCoroutine(Generate());
            }
        }
    }

    private IEnumerator Generate()
    {
        isRunning = true;
        while (QueueHasElements)
        {
            float key = -1f;
            NavCellController cell = null;
            lock (queueLock)
            {
                (key, cell) = queue.Dequeue();
                queuedCells.Remove(cell);
            }
            yield return cell.Generate(navMeshGenerationSettings);
        }
        isRunning = false;
    }

    public static bool QueueCell(float priority, NavCellController cellController)
    {
        return instance._QueueCell(priority, cellController);
    }

    private bool _QueueCell(float priority, NavCellController cellController)
    {
        lock (queueLock)
        {
            if (!queuedCells.Contains(cellController))
            {
                queuedCells.Add(cellController);
                queue.Enqueue(priority, cellController);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
