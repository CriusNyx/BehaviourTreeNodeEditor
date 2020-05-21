using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class VoxelTest : MonoBehaviour
{
    public int voxelsPerUnit = 4;
    public float actorHeight = 2f;

    public bool done = false;

    public int actorHeightInVoxels
    {
        get
        {
            return Mathf.CeilToInt(actorHeight * voxelsPerUnit);
        }
    }

    private void Start()
    {
        StartCoroutine(ProcessVoxels());
    }

    private IEnumerator ProcessVoxels()
    {
        var set = new VoxelSet<int>(1f / voxelsPerUnit, Vector3.one * (1f / voxelsPerUnit) / 2f);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Dictionary<NavMeshCell, List<MeshRenderer>> renderersPerCell = new Dictionary<NavMeshCell, List<MeshRenderer>>();

        foreach (var renderer in gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            var bounds = renderer.bounds;

            foreach (var cell in NavMeshCell.GetCellsInBounds(renderer.bounds))
            {
                DebugDraw.Box(renderer.bounds.center, Quaternion.identity, renderer.bounds.size);
                cell.Draw();

                if (!renderersPerCell.ContainsKey(cell))
                {
                    renderersPerCell.Add(cell, new List<MeshRenderer>());
                }
                renderersPerCell[cell].Add(renderer);

                // Check stopwatch
                if (stopwatch.ElapsedMilliseconds >= 2)
                {
                    yield return null;
                    stopwatch.Restart();
                }
            }
        }

        foreach (var element in renderersPerCell)
        {
            var routine
                = element.Key.Generate(
                    element.Value.ToArray(),
                    new NavMeshGenerationSettings(
                        set.orientation,
                        8,
                        16,
                        4,
                        8));

            yield return routine;
        }

        done = true;
    }

    private void Update()
    {
    }
}
