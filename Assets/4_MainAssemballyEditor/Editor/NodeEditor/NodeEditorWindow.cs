using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class NodeEditorWindow : EditorWindow
{
    [MenuItem("Tools/Node Editor")]
    public static void GetNodeEditorWindow()
    {
        GetWindow<NodeEditorWindow>();
    }

    Vector2 cameraPosition = Vector2.zero;

    public NodeEditorSave save;

    public readonly ActiveControlSet activeControlSet = new ActiveControlSet();

    private void LoadAutoSave()
    {
        save = Resources.Load<NodeEditorSave>("NodeEditorWindow/Autosave");
        if (save == null)
        {
            save = CreateInstance(typeof(NodeEditorSave)) as NodeEditorSave;
            string directory = "Assets/Resources/NodeEditorWindow/";
            string file = "AutoSave.asset";
            if (!AssetDatabase.IsValidFolder(directory))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "NodeEditorWindow");
            }
            AssetDatabase.CreateAsset(save, directory + file);
            AssetDatabase.SaveAssets();
        }
    }

    int foo = 0;

    private void OnGUI()
    {
        if (save == null)
        {
            LoadAutoSave();
        }

        var rect = GUILayoutUtility.GetRect(position.width, position.height);

        NodeEditorNode.DrawAllNodes(rect, save.nodes, this, cameraPosition);

        DrawAllLines();

        GetCameraInputs();

        Repaint();
    }

    private void GetCameraInputs()
    {
        //Hot control is zero if there is no active control
        if (GUIUtility.hotControl == 0)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDrag:
                    cameraPosition += Event.current.delta;
                    break;
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.Space)
                    {
                        ResetCameraPosition();
                    }
                    break;
                case EventType.MouseDown:
                    if (Event.current.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        Vector2 currentMousePosition = Event.current.mousePosition;
                        Vector2 currentCameraPosition = cameraPosition;
                        menu.AddItem(new GUIContent("Create Node"), false, () => { save.nodes.Add(ConstructNewNode(currentMousePosition - currentCameraPosition)); });
                        menu.ShowAsContext();
                    }
                    break;
            }
        }
    }

    private List<(Vector2 start, Vector2 end)> lines = new List<(Vector2 start, Vector2 end)>();

    public void DrawLine(Vector2 startPosition, Vector2 endPosition)
    {
        lines.Add((startPosition, endPosition));
    }

    private void DrawAllLines()
    {
        foreach ((var start, var end) in lines)
        {
            Drawing.DrawLine(start + cameraPosition, end + cameraPosition);
        }
        lines = new List<(Vector2 start, Vector2 end)>();
    }

    private NodeEditorNode ConstructNewNode(Vector2 spawnPosition)
    {
        var output = new NodeEditorNode(spawnPosition);

        output.nodeObject = new ChildNode();

        AutoSave();

        return output;
    }

    private void ResetCameraPosition()
    {
        cameraPosition = Vector3.zero;
    }

    public static void AutoSave()
    {
        var instance = GetWindow<NodeEditorWindow>();
        EditorUtility.SetDirty(instance.save);
        AssetDatabase.SaveAssets();
    }
}