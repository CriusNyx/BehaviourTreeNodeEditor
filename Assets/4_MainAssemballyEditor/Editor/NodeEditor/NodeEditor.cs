using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

public class NodeEditor : EditorWindow
{
    [MenuItem("Tools/Node Editor")]
    public static void GetNodeEditorWindow()
    {
        GetWindow<NodeEditor>();
    }

    protected Vector2 cameraPosition = Vector2.zero;

    public NodeEditorSave save;

    private bool isDragging = false;

    public readonly ActiveControlSet activeControlSet = new ActiveControlSet();

    protected virtual string AutoSaveFileName { get; } = "NodeEditorAutoSave";

    public Rect CenterRectLastFrame;

    private static float TimeToAutoSave = -1f;

    private bool regenerateContextMenu = false;

    /// <summary>
    /// Loads the auto save file, or generates one if none exists
    /// </summary>
    private void LoadAutoSave()
    {
        string resourcesLocalFolderPath = "NodeEditorWindow";
        string resourcesFolderPath = "Assets/Resources";
        string resourcesFilePath = $"{resourcesLocalFolderPath}/{AutoSaveFileName}";
        string assetsFolderPath = $"{resourcesFolderPath}/{resourcesLocalFolderPath}";
        string assetsFilePath = $"{assetsFolderPath}/{AutoSaveFileName}.asset";

        // Load the existing save file
        save = Resources.Load<NodeEditorSave>(resourcesFilePath);

        // Generate a new save file
        if (save == null)
        {
            CreateNewAutoSave();
        }
    }

    private void CreateNewAutoSave()
    {
        string resourcesLocalFolderPath = "NodeEditorWindow";
        string resourcesFolderPath = "Assets/Resources";
        string resourcesFilePath = $"{resourcesLocalFolderPath}/{AutoSaveFileName}";
        string assetsFolderPath = $"{resourcesFolderPath}/{resourcesLocalFolderPath}";
        string assetsFilePath = $"{assetsFolderPath}/{AutoSaveFileName}.asset";

        save = CreateInstance(typeof(NodeEditorSave)) as NodeEditorSave;
        if (!AssetDatabase.IsValidFolder(assetsFolderPath))
        {
            AssetDatabase.CreateFolder(resourcesFolderPath, resourcesLocalFolderPath);
        }
        AssetDatabase.CreateAsset(save, assetsFilePath);
        AssetDatabase.SaveAssets();
        save = AssetDatabase.LoadAssetAtPath<NodeEditorSave>(assetsFilePath);
    }

    protected virtual void OnGUIProtected()
    {

    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        

        // Attempt to load the auto save
        if (save == null)
        {
            LoadAutoSave();
        }

        // Draw top bar for editor
        // Useful for file buttons, etc
        DrawTopBar();

        // Draw horizontal line after top bar to visual navigation
        GUILayout.Box("", GUILayout.Height(2), GUILayout.ExpandWidth(true));

        // Draws side bar, then canvas
        GUILayout.BeginHorizontal();
        {
            // Draw the side bar in a vertical layout
            GUILayout.BeginVertical(GUILayout.Width(200));
            DrawSideBar();
            GUILayout.EndVertical();

            // Draw verticle line after side bar
            GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));

            OnGUIProtected();

            // Draw center rect for drawing windows
            var rect = GUILayoutUtility.GetRect(position.width, position.height);

            // Draw all nodes on center rect
            NodeEditorNode.DrawAllNodes(rect, save.Nodes, this, cameraPosition);

            // Draw Bezier curves on nodes
            DrawAllLines(rect.position);

            // Cache center rect for other layout calculations
            if (Event.current.type == EventType.Repaint)
            {
                CenterRectLastFrame = new Rect(rect.position, rect.size - rect.position);
            }
        }
        GUILayout.EndHorizontal();

        // Get all canvas inputs, and process them
        GetInputs();

        // Trigger a repaint every frame to accomidate missed layout changes
        Repaint();

        // End change check, and auto save
        if (EditorGUI.EndChangeCheck())
        {
            AutoSave();
        }

        CheckAutoSave();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    public void JumpCameraToNode(NodeEditorNode node)
    {
        cameraPosition = -node.windowRect.position + CenterRectLastFrame.size / 2f - node.windowRect.size / 2f;
    }

    protected virtual void DrawTopBar()
    {
        GUILayout.BeginHorizontal();
        DrawFileBar();
        GUILayout.EndHorizontal();
    }

    protected virtual void DrawSideBar()
    {

    }

    private void GetInputs()
    {
        if (regenerateContextMenu)
        {
            GenerateContextMenu();
        }

        //Hot control is zero if there is no active control
        if (GUIUtility.hotControl == 0)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDrag:
                    if (isDragging)
                    {
                        cameraPosition += Event.current.delta;
                    }
                    break;
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.Space)
                    {
                        ResetCameraPosition();
                    }
                    break;
                case EventType.MouseDown:
                    if (Event.current.button == 0)
                    {
                        if (CenterRectLastFrame.Contains(Event.current.mousePosition))
                        {
                            activeControlSet.Set<NodeEditorNode>(null);
                            isDragging = true;
                        }
                    }
                    else if (Event.current.button == 1)
                    {
                        GenerateContextMenu();
                    }
                    break;
                case EventType.MouseUp:
                    if (Event.current.button == 0)
                    {
                        isDragging = false;
                    }
                    break;
            }
        }
        switch (Event.current.type)
        {
            case EventType.KeyDown:
                switch (Event.current.keyCode)
                {
                    case KeyCode.Delete:
                        NodeEditorNode active = activeControlSet.Get<NodeEditorNode>();
                        if (active != null)
                        {
                            save.Nodes.Remove(active);
                            AutoSave();
                        }
                        break;
                }
                break;
        }
    }



    protected void RegenerateContextMenu()
    {
        regenerateContextMenu = true;
        Repaint();
    }

    private void GenerateContextMenu()
    {
        GenericMenu menu = new GenericMenu();
        if (regenerateContextMenu)
        {
            regenerateContextMenu = false;
        }

        Vector2 currentMousePosition = Event.current.mousePosition;
        Vector2 currentCameraPosition = cameraPosition;
        Vector2 nodeSpawnPosition = currentMousePosition - currentCameraPosition;

        var items = GetMenuItems(nodeSpawnPosition);

        if (items != null)
        {
            foreach (var item in items)
            {
                menu.AddItem(new GUIContent(item.itemName, item.itemTooltip), false, () => item.onClick());
            }
            menu.ShowAsContext();
        }
        else
        {
            Debug.Log("Generating Context Menu");
        }
    }

    protected virtual IEnumerable<(string itemName, string itemTooltip, Action onClick)> GetMenuItems(Vector2 nodeSpawnPosition)
    {
        yield return
            (
                "Create Node",
                "Create a new Node",
                () => ConstructNewNode(nodeSpawnPosition)
            );
    }

    private List<(Vector2 start, Vector2 end)> lines = new List<(Vector2 start, Vector2 end)>();

    public void DrawLine(Vector2 startPosition, Vector2 endPosition)
    {
        lines.Add((startPosition, endPosition));
    }

    protected virtual void DrawAllLines(Vector2 offset)
    {
        foreach ((var start, var end) in lines)
        {
            Vector2 a = start + cameraPosition + offset;
            Vector2 b = end + cameraPosition + offset;

            if (CenterRectLastFrame.Contains(a) && CenterRectLastFrame.Contains(b))
            {
                Drawing.DrawLine(a, b);
            }
        }
        lines = new List<(Vector2 start, Vector2 end)>();
    }

    protected NodeEditorNode ConstructNewNode(Vector2 spawnPosition, Func<Vector2, long, NodeEditorNode> constructor = null)
    {
        if (constructor == null)
        {
            constructor = (x, y) => new NodeEditorNode(x, y);
        }

        var output = constructor(spawnPosition, -1);

        save.Nodes.Add(output);

        AutoSave();

        return output;
    }

    private void ResetCameraPosition()
    {
        cameraPosition = Vector3.zero;
    }

    public static void AutoSave()
    {
        TimeToAutoSave = Time.realtimeSinceStartup + 1f;
    }

    private static void CheckAutoSave()
    {
        if (TimeToAutoSave > 0f && Time.realtimeSinceStartup > TimeToAutoSave)
        {
            var instance = GetWindow<NodeEditor>();

            instance.DoAutoSave();

            TimeToAutoSave = -1f;
        }
    }

    private void DoAutoSave()
    {
        string resourcesLocalFolderPath = "NodeEditorWindow";
        string resourcesFolderPath = "Assets/Resources";
        string resourcesFilePath = $"{resourcesLocalFolderPath}/{AutoSaveFileName}";
        string assetsFolderPath = $"{resourcesFolderPath}/{resourcesLocalFolderPath}";
        string assetsFilePath = $"{assetsFolderPath}/{AutoSaveFileName}.asset";

        save.Save(assetsFilePath, this.GetType(), true);
    }

    protected void DrawFileBar()
    {
        if (GUILayout.Button("New"))
        {
            New();
        }
        if (GUILayout.Button("Save"))
        {
            Save();
        }
        if (GUILayout.Button("Save As"))
        {
            SaveAs();
        }
        if (GUILayout.Button("Open"))
        {
            Open();
        }
    }

    private void New()
    {
        CreateNewAutoSave();
        AutoSave();
    }

    private void Save()
    {
        if (save.Filename == null || save.Filename == "")
        {
            SaveAs();
        }
        else
        {
            Save(save.Filename);
        }
    }

    private void Save(string filename)
    {
        ValidateGUIDs();

        if (filename != "")
        {
            save.Save(filename, this.GetType(), false);
            AutoSave();
            OnSave(filename);
        }
    }

    protected virtual void OnSave(string filename)
    {

    }

    private void SaveAs()
    {
        var filename = EditorUtility.SaveFilePanel("Save Node Canvas", Application.dataPath, "Save", "asset");
        if (filename != "" && filename != null)
        {
            Save(EditorPathUtility.SystemPathToProjectPath(filename));
        }
    }

    private void Open()
    {
        string filepath = EditorUtility.OpenFilePanel("Open Node Canvas", Application.dataPath, "asset");
        if (filepath != "")
        {
            filepath = EditorPathUtility.SystemPathToProjectPath(filepath);
            OpenFile(filepath);
        }
    }

    private void Open(NodeEditorSave save)
    {
        this.save = save;
        AutoSave();
    }

    public static NodeEditor OpenFile(string filepath)
    {
        NodeEditorSave save = AssetDatabase.LoadAssetAtPath<NodeEditorSave>(filepath);
        Type saveType = Type.GetType(save?.NodeEditorType);
        if (saveType == null)
        {
            saveType = typeof(NodeEditor);
        }
        if (saveType != null)
        {
            NodeEditor window = GetWindow(saveType) as NodeEditor;
            if (window != null)
            {
                window.Open(save);
            }
            return window;
        }
        return null;
    }

    private void ValidateGUIDs()
    {
        GUIDPool pool = new GUIDPool();

        foreach(var node in save.Nodes)
        {
            if(node.GUID >= 0)
            {
                if (pool.Contains((ulong)node.GUID))
                {
                    node.ClearGUID();
                }
                else
                {
                    pool.Add((ulong)node.GUID);
                }
            }
        }
        
        foreach(var node in save.Nodes)
        {
            if(node.GUID < 1)
            {
                node.SetGUID((long)pool.GetGUID());
            }
        }
    }
}