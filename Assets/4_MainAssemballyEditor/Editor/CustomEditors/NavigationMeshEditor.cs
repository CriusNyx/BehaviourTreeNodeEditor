using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavigationMesh))]
public class NavigationMeshEditor : Editor
{
    private void OnSceneGUI()
    {
        NavCellEditor.DrawNavCellEditorSettings(target as Component);
    }
}