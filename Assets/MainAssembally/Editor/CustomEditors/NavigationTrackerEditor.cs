using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavMeshTracker))]
public class NavigationTrackerEditor : Editor
{
    private void OnSceneGUI()
    {
        NavCellEditor.DrawNavCellEditorSettings(target as Component);
    }
}