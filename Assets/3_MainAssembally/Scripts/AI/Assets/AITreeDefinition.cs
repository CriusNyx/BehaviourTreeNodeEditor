using System;
using UnityEngine;

[Serializable]
public class AITreeDefinition
{
    public string name;
    [System.NonSerialized]
    public AITreeNode root;

    public AITreeDefinitionArgument[] arguments = new AITreeDefinitionArgument[] { };
}