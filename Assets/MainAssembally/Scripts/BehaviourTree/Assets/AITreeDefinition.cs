using System;
using UnityEngine;

namespace GameEngine.AI
{
    [Serializable]
    public class AITreeDefinition
    {
        public string name;
        [System.NonSerialized]
        public AITreeNode root;

        public AITreeDefinitionArgument[] arguments = new AITreeDefinitionArgument[] { };
    }
}