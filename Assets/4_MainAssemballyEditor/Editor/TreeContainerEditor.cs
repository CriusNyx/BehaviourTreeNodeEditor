using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.IO.Compression;
using System.Xml;

[CustomEditor(typeof(TreeContainer))]
public class TreeContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TreeContainer container = target as TreeContainer;

        Undo.RecordObject(container, "Created Tree Container");

        EditorGUI.BeginChangeCheck();

        TreeNode root = null;

        if (GUILayout.Button("Create"))
        {
            root = new RootNode(new ChildNode(), new ChildNode());
        }

        GUILayout.Label(container.Tree?.ToString());

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(container, "Created Tree");

            if (root != null)
            {
                container.Tree = root;
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(container);

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
