using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NodeEditorSave : ScriptableObject
{
    [SerializeReference]
    private List<NodeEditorNode> nodes = new List<NodeEditorNode>();
    public IList<NodeEditorNode> Nodes => nodes;

    [SerializeField]
    private string filename;

    public string Filename => filename;

    [SerializeField]
    private string nodeEditorType;
    public string NodeEditorType => nodeEditorType;

    public void Save(string filename, Type nodeEditorType, bool autosave)
    {
        if (!autosave)
        {
            this.filename = filename;
        }
        this.nodeEditorType = nodeEditorType.ToString();

        AssetManagement.CreateOrOverwriteAsset(filename, this, autosave);
    }
}
