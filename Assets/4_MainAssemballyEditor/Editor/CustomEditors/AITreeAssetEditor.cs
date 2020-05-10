using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

[CustomEditor(typeof(AITreeAsset))]
public class AITreeAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AITreeAsset asset = target as AITreeAsset;
        GUILayout.Label(asset.ToString());
        GUILayout.Label(asset.root?.ToString());
        if (GUILayout.Button("Open"))
        {
            NodeEditor.OpenFile(asset.sourceFileName);
        }
    }
}