using Scripts.Extensions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.NodeEditor;
using UnityEngine;

[Serializable]
public class NodeEditorNode
{
    [SerializeField]
    public Rect windowRect = new Rect(0, 0, 400, 200);

    [SerializeReference]
    private NodeEditorDropBox dropBox;

    [SerializeReference]
    private List<NodeEditorGrabBox> grabBoxes = new List<NodeEditorGrabBox>();
    [SerializeReference]
    private NodeEditorGrabBox openBox;

    [SerializeReference]
    public object data;

    [SerializeField]
    private long guid = -1;

    public long GUID => guid;

    public Vector2 Position { get; private set; }

    private bool resizeMode = false;

    public NodeEditorNode(long guid)
    {
        this.guid = guid;
        dropBox = new NodeEditorDropBox(this);
    }

    public NodeEditorNode(Vector2 spawnPosition, long guid) : this(guid)
    {
        this.windowRect = this.windowRect.Translate(spawnPosition);
    }

    public void SetGUID(long guid)
    {
        if(this.guid == -1)
        {
            this.guid = guid;
        }
    }

    public void ClearGUID()
    {
        this.guid = -1;
    }

    public NodeEditorNode[] GetChildren()
    {
        List<NodeEditorNode> output = new List<NodeEditorNode>();
        foreach(var grabBox in grabBoxes)
        {
            NodeEditorNode other = grabBox?.DropBox?.Parent;
            if(other != null)
            {
                output.Add(other);
            }
        }
        return output.ToArray();
    }

    private void DrawGUI(int windowNumber, Vector3 cameraPosition, NodeEditor nodeEditor)
    {
        windowRect =
            GUI.Window(
                windowNumber,
                windowRect.Translate(cameraPosition),
                (x) => { BaseDrawContents(x, nodeEditor); },
                data?.ToString())
            .Translate(-cameraPosition);
    }

    private void BaseDrawContents(int id, NodeEditor nodeEditor)
    {
        DrawDropBoxes(id, nodeEditor);
        DrawInteriorGUI();
        DrawGrabBoxes(id, nodeEditor);

        var resizeBox = DrawResizeBox();

        ProcessMyEvents(nodeEditor, resizeBox);

        if (!resizeMode)
            GUI.DragWindow();
    }

    private void DrawDropBoxes(int id, NodeEditor nodeEditor)
    {
        RemoveUnusedDropBoxes();
        DrawOpenBox();
        DrawDropBox(id, nodeEditor);

        //This line is needed to correctly draw bezier curves
        Position = new Vector2(windowRect.x, windowRect.y);
    }

    private Rect DrawResizeBox()
    {
        Vector2 resizeSize = Vector2.one * 20;
        var output = new Rect(windowRect.size - resizeSize * 1.2f, resizeSize);
        GUI.Box(output, "");
        return output;
    }

    protected virtual void DrawInteriorGUI()
    {
        GUILayout.Space(5);

        if (data != null)
        {
            var output = GenericCustomEditors.DrawCustomEditor(data, out bool success);
            if (success)
            {
                data = output;
            }
        }

        GUILayout.Space(5);
    }

    private void DrawGrabBoxes(int id, NodeEditor nodeEditor)
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

    private void DrawDropBox(int id, NodeEditor nodeEditor)
    {
        dropBox.OnGUI(nodeEditor, id);
    }

    private void ProcessMyEvents(NodeEditor nodeEditor, Rect sizeBox)
    {
        switch (Event.current.type)
        {
            case EventType.MouseDown:
                {
                    nodeEditor.activeControlSet.Set<NodeEditorNode>(this);
                    if (sizeBox.Contains(Event.current.mousePosition))
                    {
                        resizeMode = true;
                    }
                }
                break;
            case EventType.MouseDrag:
                {
                    if (resizeMode)
                    {
                        windowRect = new Rect(windowRect.position, windowRect.size + Event.current.delta);
                        Event.current.Use();
                    }
                }
                break;
            case EventType.MouseUp:
                {
                    resizeMode = false;
                }
                break;
        }
    }

    private void ProcessEditorEvents(NodeEditor nodeEditor)
    {
        dropBox.ProcessEvents(nodeEditor);

        foreach (var box in grabBoxes)
        {
            box.ProcessEvents(nodeEditor);
        }
        openBox.ProcessEvents(nodeEditor);
    }

    public static void DrawAllNodes(Rect groupPosition, IList<NodeEditorNode> nodes, NodeEditor nodeEditorWindow, Vector3 cameraPosition)
    {
        float oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 75;

        GUI.Box(groupPosition, "");

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

        EditorGUIUtility.labelWidth = oldLabelWidth;
    }
}