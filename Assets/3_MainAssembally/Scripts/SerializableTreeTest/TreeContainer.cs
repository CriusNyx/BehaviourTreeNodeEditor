using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "TreeContainer", menuName = "Behaviour/Tree")]
public class TreeContainer : ScriptableObject
{
    [SerializeReference]
    public TreeNode Tree;
}