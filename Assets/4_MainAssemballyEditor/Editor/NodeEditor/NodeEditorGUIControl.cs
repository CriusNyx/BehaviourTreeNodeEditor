using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class NodeEditorGUIControl
{
    [SerializeReference]
    public NodeEditorNode Parent;

    public NodeEditorGUIControl(NodeEditorNode parent)
    {
        Parent = parent;
    }

    public abstract void OnGUI(NodeEditor nodeEditor, int id);
    public abstract void ProcessEvents(NodeEditor nodeEditor);
}