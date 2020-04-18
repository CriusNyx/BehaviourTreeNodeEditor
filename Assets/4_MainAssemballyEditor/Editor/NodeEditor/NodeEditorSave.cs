using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeEditorSave : ScriptableObject
{
    [SerializeReference]
    public List<NodeEditorNode> nodes = new List<NodeEditorNode>();
}
