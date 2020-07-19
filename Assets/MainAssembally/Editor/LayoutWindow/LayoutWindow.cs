using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LayoutWindow : EditorWindow
{
    [MenuItem("Tools/Layout Window")]
    public static void GetWindow()
    {
        var window = EditorWindow.GetWindow(typeof(LayoutWindow));
    }

    private void OnGUI()
    {
        minSize = new Vector2(1024, 25);
        maxSize = new Vector2(2000, 50);
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Scripting Mode", GUILayout.Width(200)) 
                || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Alpha1))
            {

            }
        }
        GUILayout.EndHorizontal();
    }
}