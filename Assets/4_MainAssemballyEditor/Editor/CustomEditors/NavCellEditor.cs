using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavMeshCell))]
public class NavCellEditor : Editor
{
    void OnSceneGUI()
    {
        DrawNavCellEditorSettings(target as Component);
    }

    public static void DrawNavCellEditorSettings(Component component)
    {
        if(!(component is NavMeshCell) && component.GetComponent<NavMeshCell>() != null)
        {
            return;
        }

        Func<bool, string, bool> ToggleButton = (x, y) => GUILayout.Toggle(x, y, "Button", GUILayout.Width(100));

        Handles.BeginGUI();
        GUILayout.Label("Draw Settings");
        GUILayout.BeginHorizontal();
        {
            if(component is NavMeshCell cell)
            {
                cell.alwaysDrawThisGizmos = ToggleButton(cell.alwaysDrawThisGizmos, "This");
            }
            NavMeshCell.alwaysDrawAllGizmos = ToggleButton(NavMeshCell.alwaysDrawAllGizmos, "All");
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            NavMeshCell.drawQuads = ToggleButton(NavMeshCell.drawQuads, "Quads");
            NavMeshCell.drawIslands = ToggleButton(NavMeshCell.drawIslands, "Islands");
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            NavMeshCell.drawQuadLinks = ToggleButton(NavMeshCell.drawQuadLinks, "Links");
            NavMeshCell.drawIslandLinks = ToggleButton(NavMeshCell.drawIslandLinks, "Links");
        }
        GUILayout.EndHorizontal();

        Handles.EndGUI();
    }
}
