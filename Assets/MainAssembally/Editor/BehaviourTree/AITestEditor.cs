using GameEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameEngine.AIEditor
{
    [CustomEditor(typeof(AITest))]
    public class AITestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var aiTest = target as AITest;

            DrawDefaultInspector();

            GUILayout.Box("", GUILayout.Height(2), GUILayout.ExpandWidth(true));

            if (aiTest?.aiTreeAsset != null)
            {
                var treeEditor = CreateEditor(aiTest.aiTreeAsset);
                treeEditor.OnInspectorGUI();
            }
        }
    }
}