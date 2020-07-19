using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

[Serializable]
[XmlInclude(typeof(RootNode))]
[XmlInclude(typeof(ChildNode))]
public abstract class TreeNode
{
    [SerializeReference]
    public List<TreeNode> subNodes = new List<TreeNode>();

    public TreeNode()
    {

    }

    public TreeNode(params TreeNode[] children)
    {
        this.subNodes = children.ToList();
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        ToString(builder, 0);
        return builder.ToString();
    }

    private void ToString(StringBuilder stringBuilder, int indent)
    {
        for (int i = 0; i < indent; i++)
        {
            stringBuilder.Append(" ");
        }
        stringBuilder.Append(this.GetType().ToString());
        stringBuilder.Append("\n");
        foreach (var child in subNodes)
        {
            child.ToString(stringBuilder, indent + 1);
        }
    }

    public static byte[] Serialize(TreeNode treeNode)
    {
        return treeNode.ToXmlCompressed<TreeNode>();
    }

    public static TreeNode Deserialize(byte[] arr)
    {
        return arr.TFromXmlCompressed<TreeNode>();
    }
}