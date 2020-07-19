using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(ShipPartInfo))]
public class ShipPartEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        string assetPath = AssetDatabase.GetAssetPath(target);
        string modelPath = Path.GetDirectoryName(assetPath) + "\\" + Path.GetFileNameWithoutExtension(assetPath) + ".blend";
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

        if(model == null)
        {
            GUILayout.Label("Error: ShipPartInfo name must match the name of a ship part model");
        }
    }
}