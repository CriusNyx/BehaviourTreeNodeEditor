using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("Snapping Tool")]
public class SnappingTool : EditorTool
{
    GameObject selectedGameObject;
    public SnappingToolSettings settings { get; private set; }
    SnappingToolState state;

    public override void OnToolGUI(EditorWindow window)
    {
        Initialize();
        CheckSelection();

        state?.DrawSceneGUI();

        DrawSceneGUI();
        GetInputs();
    }

    private void Initialize()
    {
        if (settings == null)
        {
            settings = AssetManagement.OpenOrCreateAsset<SnappingToolSettings>("Assets/Resources/Tools/SnappingTool/SnappingToolSettings.asset");
        }
    }

    #region Draw Scene GUI
    private void DrawSceneGUI()
    {
        Handles.BeginGUI();
        {
            DrawSnappingModeButtons();
        }
        Handles.EndGUI();
    }

    private void DrawSnappingModeButtons()
    {
        var (snapPosition, snapRotation) = settings.GetSnappingMode();
        GUILayout.BeginHorizontal();
        {
            snapPosition = GUILayout.Toggle(snapPosition, "Snap Position", "Button", GUILayout.Width(200));
            snapRotation = GUILayout.Toggle(snapRotation, "Snap Rotation", "Button", GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();
        settings.SetSnappingMode(snapPosition, snapRotation);
    }
    #endregion

    #region Inputs
    private void GetInputs()
    {
        var current = Event.current;
        if (current.type == EventType.KeyDown && current.keyCode == KeyCode.A)
        {
            current.Use();
            switch (settings.snapMode)
            {
                case Snapper.SnapMode.none:
                    settings.snapMode = Snapper.SnapMode.position;
                    break;
                case Snapper.SnapMode.position:
                    settings.snapMode = Snapper.SnapMode.rotation;
                    break;
                case Snapper.SnapMode.rotation:
                    settings.snapMode = Snapper.SnapMode.both;
                    break;
                case Snapper.SnapMode.both:
                    settings.snapMode = Snapper.SnapMode.none;
                    break;
            }
        }
    }
    #endregion

    private void CheckSelection()
    {
        if (Selection.activeGameObject != selectedGameObject)
        {
            ResetState();
        }
    }

    private void ResetState()
    {
        selectedGameObject = Selection.activeGameObject;
        state = new SnappingToolState(selectedGameObject, this);
    }

    [MenuItem("Tools/Snapping Tool _u")]
    public static void SelectSnappingTool()
    {
        Tools.current = Tool.Custom;
    }
}
