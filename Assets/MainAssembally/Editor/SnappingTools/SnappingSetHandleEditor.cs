using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SnappingSetOwner))]
public class SnappingSetOwnerEditor : Editor
{
    private MeshFilter selectedFilter = null;
    private int selectedVertex = -1;

    Snapper options = null;

    private void OnSceneGUI()
    {
        return;
        SnappingSetOwner handle = target as SnappingSetOwner;
        DrawSelectionHandles(handle);
        ProcessInputs(handle);
        DrawButtons();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    private void DrawSelectionHandles(SnappingSetOwner handle)
    {
        foreach (var set in handle.GetComponentsInChildren<SnappingSet>())
        {
            foreach (var filter in set.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = filter.sharedMesh;
                for (int i = 0; i < mesh.vertexCount; i++)
                {
                    Vector3 vertex = mesh.vertices[i];
                    Vector3 normal = mesh.normals[i];

                    Vector3 worldSpaceVertex = filter.transform.localToWorldMatrix.MultiplyPoint(vertex);
                    Vector3 worldSpaceNormal = filter.transform.localToWorldMatrix.MultiplyVector(normal);

                    Color color = Handles.color;

                    if (selectedFilter == filter && selectedVertex == i)
                    {
                        Handles.color = Color.green;
                    }

                    if (Handles.Button(worldSpaceVertex, Quaternion.LookRotation(worldSpaceNormal, Vector3.up), 0.06f, 0.1f, Handles.CircleHandleCap))
                    {
                        selectedFilter = filter;
                        selectedVertex = i;
                        options = null;
                    }

                    Handles.color = color;
                }
            }
        }
    }

    private void ProcessInputs(SnappingSetOwner handle)
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
        {
            Snap();
        }
    }

    private void DrawButtons()
    {
        Handles.BeginGUI();
        if(selectedFilter != null && selectedVertex != -1)
        {
            if (GUILayout.Button("Snap"))
            {
                Snap();
            }
        }
        Handles.EndGUI();
    }

    private void Snap()
    {
        var handle = target as SnappingSetOwner;

        if (options != null)
        {
            options.Itterate();
        }
        else
        {
            options = new Snapper(handle, selectedFilter, selectedVertex);
        }
        Snapper.SnapMode snapMode;

        switch (Tools.current)
        {
            case Tool.Move:
                snapMode = Snapper.SnapMode.position;
                break;
            case Tool.Rotate:
                snapMode = Snapper.SnapMode.rotation;
                break;
            case Tool.Transform:
                snapMode = Snapper.SnapMode.both;
                break;
            default:
                snapMode = Snapper.SnapMode.none;
                break;
        }

        options.Snap(handle.gameObject, snapMode);
    }
}
