using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.NodeEditor;
using System;

[Serializable]
public class NodeEditorGrabBox : NodeEditorGUIControl
{
    Rect myRect;

    public NodeEditorDropBox DropBox { get => dropBox; private set => dropBox = value; }
    [SerializeReference]
    private NodeEditorDropBox dropBox;

    public NodeEditorGrabBox(NodeEditorNode parent) : base(parent)
    {

    }

    public override void OnGUI(NodeEditor nodeEditor, int windowID)
    {
        myRect = GUILayoutUtility.GetRect(10, 10);
        GUI.Box(myRect, "");

        if (Event.current.type == EventType.MouseDown)
        {
            if (myRect.Contains(Event.current.mousePosition))
            {
                nodeEditor.activeControlSet.Set(this);
                Event.current.Use();
            }
        }

        if (IsSelected(nodeEditor))
        {
            Vector2 start = myRect.center + Parent.Position;
            Vector2 end = Event.current.mousePosition + Parent.Position;
            foreach ((var a, var b) in Bezier.ConstructBezierFromNormal2(start, end, Vector2.up, 10).ForeachElementAndNext())
            {
                nodeEditor.DrawLine(a, b);
            }
        }
        if(DropBox != null)
        {
            Vector2 start = myRect.center + Parent.Position;
            Vector3 end = DropBox.centerLastFrame;
            foreach ((var a, var b) in Bezier.ConstructBezierFromNormal2(start, end, Vector2.up, 10).ForeachElementAndNext())
            {
                nodeEditor.DrawLine(a, b);
            }
        }
    }

    public override void ProcessEvents(NodeEditor window)
    {
        if (window.activeControlSet.Get<NodeEditorGrabBox>() == this)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDrag:
                    Event.current.Use();
                    break;
                case EventType.MouseUp:
                    window.activeControlSet.Remove<NodeEditorGrabBox>();
                    break;
            }
        }
    }

    public bool IsSelected(NodeEditor window)
    {
        return window.activeControlSet.Get<NodeEditorGrabBox>() == this;
    }

    public void OnDrop(NodeEditorDropBox dropBox)
    {
        this.DropBox = dropBox;
        NodeEditor.AutoSave();
    }
}