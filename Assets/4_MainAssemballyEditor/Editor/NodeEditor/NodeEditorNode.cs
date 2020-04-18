using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Scripts.Extensions;
using UnityEditor.NodeEditor;

[Serializable]
public class NodeEditorNode
{
    private Rect windowRect = new Rect(20, 20, 200, 100);

    [SerializeReference]
    private NodeEditorDropBox dropBox;

    [SerializeReference]
    private List<NodeEditorGrabBox> grabBoxes = new List<NodeEditorGrabBox>();
    [SerializeReference]
    private NodeEditorGrabBox openBox;

    [SerializeReference]
    public TreeNode nodeObject;

    public Vector2 Position { get; private set; }

    public NodeEditorNode()
    {
        dropBox = new NodeEditorDropBox(this);
    }

    public NodeEditorNode(Vector2 spawnPosition) : this()
    {
        windowRect = new Rect(spawnPosition.x, spawnPosition.y, 120, 100);
    }

    private void DrawGUI(int windowNumber, Vector3 cameraPosition, NodeEditorWindow nodeEditor)
    {
        windowRect = 
            GUI.Window(
                windowNumber,
                windowRect.Translate(cameraPosition),
                (x) => { BaseDrawContents(x, nodeEditor); },
                "Foo")
            .Translate(-cameraPosition);
    }

    private void BaseDrawContents(int id, NodeEditorWindow nodeEditor)
    {
        DrawDropBoxes(id, nodeEditor);
        DrawInteriorGUI();
        DrawGrabBoxes(id, nodeEditor);

        GUI.DragWindow();
    }

    private void DrawDropBoxes(int id, NodeEditorWindow nodeEditor)
    {
        RemoveUnusedDropBoxes();
        DrawOpenBox();
        DrawDropBox(id, nodeEditor);

        //This line is needed to correctly draw bezier curves
        Position = new Vector2(windowRect.x, windowRect.y);
    }

    protected virtual void DrawInteriorGUI()
    {
        GUILayout.Space(5);

        if (nodeObject is ChildNode childNode)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(nameof(childNode.test));
                childNode.test = EditorGUILayout.TextField(childNode.test);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(5);
    }

    private void DrawGrabBoxes(int id, NodeEditorWindow nodeEditor)
    {
        GUILayout.BeginHorizontal();
        {
            foreach (var box in grabBoxes)
            {
                box.OnGUI(nodeEditor, id);
                GUILayout.Space(5);
            }
            openBox.OnGUI(nodeEditor, id);
        }
        GUILayout.EndHorizontal();
    }

    private void RemoveUnusedDropBoxes()
    {
        grabBoxes.RemoveAll(x => x.DropBox == null);
    }

    private void DrawOpenBox()
    {
        if (openBox == null)
        {
            openBox = new NodeEditorGrabBox(this);
        }
        if (openBox.DropBox != null)
        {
            grabBoxes.Add(openBox);
            openBox = new NodeEditorGrabBox(this);
        }
    }

    private void DrawDropBox(int id, NodeEditorWindow nodeEditor)
    {
        dropBox.OnGUI(nodeEditor, id);
    }

    private void ProcessEditorEvents(NodeEditorWindow nodeEditor)
    {
        dropBox.ProcessEvents(nodeEditor);

        foreach(var box in grabBoxes)
        {
            box.ProcessEvents(nodeEditor);
        }
        openBox.ProcessEvents(nodeEditor);
    }

    public static void DrawAllNodes(Rect groupPosition, IList<NodeEditorNode> nodes, NodeEditorWindow nodeEditorWindow, Vector3 cameraPosition)
    {
        GUI.BeginGroup(groupPosition);
        {
            nodeEditorWindow.BeginWindows();
            foreach ((var index, var node) in nodes.Foreach())
            {
                node.DrawGUI(index, cameraPosition, nodeEditorWindow);
            }
            nodeEditorWindow.EndWindows();
        }
        GUI.EndGroup();

        foreach ((var _, var node) in nodes.Foreach())
        {
            node.ProcessEditorEvents(nodeEditorWindow);
        }
    }
}