using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChildNode : TreeNode
{
    public string test = "FOO";

    public ChildNode()
    {

    }

    public ChildNode(params TreeNode[] children) : base(children)
    {
    }
}