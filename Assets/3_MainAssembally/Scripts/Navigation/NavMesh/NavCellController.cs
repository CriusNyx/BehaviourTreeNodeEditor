using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavCellController : MonoBehaviour
{
    private List<Collider> collidersInCell = new List<Collider>();
    private NavMeshCell cell;

    public float timeSinceLastInvalidation = 0f;
    public float currentPenalty = 0f;

    public const float PENALTY_PER_GENERATION = 1f;
    public const float PENALTY_DECAY_PER_SECOND = 1f;

    public NavMeshCell Cell
    {
        get
        {
            if (cell == null) 
                cell = gameObject.GetComponent<NavMeshCell>();
            return cell;
        }
    }

    public NavigationMesh mesh { get; private set; }

    public static NavCellController GetCell(int x, int y, NavigationMesh mesh)
    {
        return mesh.GetCell(x, y).controller;
    }

    public static NavCellController GetCell(Vector3 pos, NavigationMesh mesh)
    {
        return mesh.GetCell(pos).controller;
    }

    public static IEnumerable<NavCellController> GetCellsInBounds(Collider collider, NavigationMesh mesh)
    {
        foreach(var element in NavMeshCell.GetCellsInBounds(mesh, collider))
        {
            yield return element.controller;
        }
    }

    public static IEnumerable<NavCellController> GetCellsInBounds(Bounds bounds, NavigationMesh mesh)
    {
        foreach(var element in NavMeshCell.GetCellsInBounds(mesh, bounds))
        {
            yield return element.controller;
        }
    }

    public void AddCollider(Collider collider)
    {
        if (!collidersInCell.Contains(collider))
        {
            collidersInCell.Add(collider);
        }
    }

    public void RemoveCollider(Collider collider)
    {
        collidersInCell.Remove(collider);
    }

    private void Awake()
    {
        mesh = gameObject.GetComponentInParent<NavigationMesh>();
        if(mesh == null)
        {
            mesh = NavigationMesh.mainMesh;
        }
    }

    private void Update()
    {
        currentPenalty -= (Time.deltaTime * PENALTY_DECAY_PER_SECOND) * Mathf.Pow(2, timeSinceLastInvalidation + 1f);
        timeSinceLastInvalidation += Time.deltaTime;
        if(currentPenalty <= 0f)
        {
            currentPenalty = 0f;
            timeSinceLastInvalidation = 0f;
        }
    }

    public void Invalidate()
    {
        float weight = Mathf.Infinity;
        foreach(var player in Game.Players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < weight)
                weight = distance;
        }

        weight += currentPenalty;

        timeSinceLastInvalidation = 0f;

        if(NavMeshGenerationQueue.QueueCell(weight, this))
        {
            currentPenalty += PENALTY_PER_GENERATION;
        }
    }

    public Coroutine Generate(NavMeshGenerationSettings generationSettings)
    {
        return Cell.Generate(collidersInCell.ToArray(), generationSettings);
    }
}