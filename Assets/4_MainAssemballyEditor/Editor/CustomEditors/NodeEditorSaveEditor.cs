using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodeEditorSave))]
public class NodeEditorSaveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open"))
        {
            NodeEditor.OpenFile(AssetDatabase.GetAssetPath(target));
        }
    }
}
