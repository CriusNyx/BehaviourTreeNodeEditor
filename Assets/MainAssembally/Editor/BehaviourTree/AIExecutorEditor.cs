using GameEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEngine.AIEditor
{
    [CustomEditor(typeof(AIExecutor))]
    public class AIExecutorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AIExecutor executor = target as AIExecutor;

            GUILayout.Label(executor.Log?.ToString());

            if (executor.asset != null)
                if (GUILayout.Button("View Log"))
                    BehaviourTreeEditor.OpenFile(executor.asset.sourceFileName, executor.Log);
        }
    }
}