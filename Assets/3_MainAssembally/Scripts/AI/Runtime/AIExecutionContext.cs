using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.AI
{
    public class AIExecutionContext
    {
        public readonly GameObject gameObject;
        public readonly AIExecutionLog log;
        public readonly AITreeNode[] children;
        public readonly TreeArguments memoryMap;

        public AIExecutionContext(GameObject gameObject, AIExecutionLog log, AITreeNode[] children, TreeArguments memoryMap)
        {
            this.gameObject = gameObject;
            this.log = log;
            this.children = children;
            this.memoryMap = memoryMap;
        }
    }
}