using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


public static class EditorGUICustomUtility
{
    /// <summary>
    /// Draw a custom editor for an array that can add and remove elements dynamically
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="elementEditor"></param>
    /// <param name="addElementButtonName"></param>
    /// <returns></returns>
    public static T[] DrawArrayEditor<T>(T[] arr, Func<T, T> elementEditor, string addElementButtonName, bool labelElement) => DrawArrayEditor<T>(arr, elementEditor, addElementButtonName, () => default(T), labelElement);

    /// <summary>
    /// Draw a custom editor for an array that can add and remove elements dynamically
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="elementEditor"></param>
    /// <param name="addElementButtonName"></param>
    /// <param name="constructor"></param>
    /// <returns></returns>
    public static T[] DrawArrayEditor<T>(T[] arr, Func<T, T> elementEditor, string addElementButtonName, Func<T> constructor, bool labelElement)
    {
        if (arr == null)
        {
            arr = new T[] { };
        }
        List<T> list = new List<T>(arr);
        int elementToRemove = -1;

        GuiLine(2, 5);

        for (int i = 0; i < list.Count; i++)
        {
            var element = list[i];
            if (labelElement)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(element?.ToString());
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        elementToRemove = i;
                    }
                }
                GUILayout.EndHorizontal();

                element = elementEditor(element);
                list[i] = element;

                GuiLine(2, 5);
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    element = elementEditor(element);
                    list[i] = element;
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        elementToRemove = i;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        if (elementToRemove != -1)
        {
            list.RemoveAt(elementToRemove);
        }
        if (GUILayout.Button(addElementButtonName))
        {
            list.Add(constructor());
        }

        GuiLine(2, 5);
        GuiLine(2, 5);

        return list.ToArray();
    }

    /// <summary>
    /// Draw a horizontal line in the gui
    /// </summary>
    /// <param name="lineHeight"></param>
    /// <param name="padding"></param>
    public static void GuiLine(int lineHeight, int padding = 0) => GuiLine(lineHeight, padding, padding);

    /// <summary>
    /// Draw a horizontal line in the gui
    /// </summary>
    /// <param name="lineHeight"></param>
    /// <param name="paddingAbove"></param>
    /// <param name="paddingBelow"></param>
    public static void GuiLine(int lineHeight, int paddingAbove, int paddingBelow)
    {

        Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);

        rect.height = lineHeight;

        GUILayout.Space(paddingAbove);

        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

        GUILayout.Space(paddingBelow);
    }

    public static void DrawSelectableNameField(float padding, GameObject target, string undoName)
    {
        Undo.RecordObject(target, undoName);

        GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
        GUIStyle testStyle = GUI.skin.GetStyle("textField");

        Color oldColor = GUI.color;

        if (Selection.activeGameObject == target)
        {
            GUI.color *= Color.red;
        }

        GUILayout.BeginHorizontal();
        {
            GUILayout.Space(padding);
            if (GUILayout.Button("+"))
            {
                Selection.activeObject = target;
            }
            target.name = EditorGUILayout.TextField(target.name);
        }
        GUILayout.EndHorizontal();

        GUI.color = oldColor;
    }

    private const float DEFAULT_INDENT_AMOUNT = 20f;

    public static void DrawTree<T>(T
        root,
        Func<T, IEnumerable<T>> GetChildren,
        float indentAmount = DEFAULT_INDENT_AMOUNT,
        params (Type type, Action<T> typeEditor)[] editors)
        => DrawTreeFull<T>(root, GetChildren, null, indentAmount, 0, editors);

    public static void DrawTree<T>(T
        root,
        Func<T, IEnumerable<T>> GetChildren,
        Action<T> defaultEditor,
        params (Type type, Action<T> typeEditor)[] editors)
        => DrawTreeFull<T>(root, GetChildren, defaultEditor, DEFAULT_INDENT_AMOUNT, 0, editors);

    public static void DrawTree<T>(T
        root,
        Func<T, IEnumerable<T>> GetChildren,
        Action<T> defaultEditor,
        float indentAmount,
        params (Type type, Action<T> typeEditor)[] editors)
        => DrawTreeFull<T>(root, GetChildren, defaultEditor, indentAmount, 0, editors);

    private static void DrawTreeFull<T>(
        T root,
        Func<T, IEnumerable<T>> GetChildren,
        Action<T> defaultTypeEditor,
        float indentAmount,
        int currentIndent,
        (Type type, Action<T> typeEditor)[] editors)
    {
        foreach (var (type, editor) in editors)
        {
            if (root.GetType().IsAssignableFrom(type))
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(indentAmount * currentIndent);
                    GUILayout.BeginVertical();
                    {
                        if (editor == null)
                        {
                            defaultTypeEditor?.Invoke(root);
                        }
                        else
                        {
                            editor(root);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }
        foreach (var child in GetChildren(root))
        {
            DrawTreeFull<T>(child, GetChildren, defaultTypeEditor, indentAmount, currentIndent + 1, editors);
        }
    }
}
