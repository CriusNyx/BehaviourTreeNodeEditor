using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RootNode : TreeNode
{
    public RootNode()
    {

    }

    public RootNode(params TreeNode[] children) : base(children)
    {
    }
}