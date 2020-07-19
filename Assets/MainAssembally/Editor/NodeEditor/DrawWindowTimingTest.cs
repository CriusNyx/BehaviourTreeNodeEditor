using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DrawWindowTimingTest : EditorWindow
{
    private Rect windowRect = new Rect(20, 20, 150, 50);

    [MenuItem("Tools/Test/Timing")]
    private static void GetTimingTestWindow() => EditorWindow.GetWindow<DrawWindowTimingTest>();
    bool log = true;

    private void OnGUI()
    {
        var rect = GUILayoutUtility.GetRect(position.width, position.height);


        GUI.BeginGroup(rect);
        {
            if (log)
                Debug.Log("Before Begin Windows");
            BeginWindows();
            {
                if (log)
                    Debug.Log("Before Window");
                windowRect = GUI.Window(0, windowRect, (x) => DrawWindow(x), "Foo");
                if (log)
                    Debug.Log("After Window");
            }
            EndWindows();
            if (log)
                Debug.Log("After End Windows");
        }
        GUI.EndGroup();
    }
    //
    private void DrawWindow(int id)
    {
        if (GUILayout.Button("Test"))
        {
            Debug.Log($"log = {log}");
        }
        if (log)
            Debug.Log("Draw Window");
        log = false;

        GUI.DragWindow();
    }
}