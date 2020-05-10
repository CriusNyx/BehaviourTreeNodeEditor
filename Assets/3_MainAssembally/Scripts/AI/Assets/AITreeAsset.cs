using UnityEngine;

public class AITreeAsset : ScriptableObject
{
    [SerializeReference]
    public AITreeDefinition definition;
    [SerializeReference]
    public AITreeNode root;

    public string sourceFileName;
}
