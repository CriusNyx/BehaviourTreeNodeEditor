using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

/// <summary>
/// Logic for finding generic editors for objects
/// </summary>
public static class GenericCustomEditors
{
    private static Dictionary<Type, MethodInfo> cache;

    public static T DrawCustomEditorT<T>(T t, out bool success, string label = null, bool drawDefaultEditorInstead = true)
    {
        object output = DrawCustomEditor(t, out success, t?.GetType(), label, drawDefaultEditorInstead);
        if (output == null)
        {
            return default(T);
        }
        else
        {
            return (T)output;
        }
    }

    public static object DrawCustomEditor(object o, out bool success, Type type, string label = null, bool drawDefaultEditorInstead = true)
    {
        EnsureCache();

        if (type == null)
        {
            success = false;
            return o;
        }

        if (cache.ContainsKey(type))
        {
            success = true;
            return cache[type].Invoke(null, new object[] { o, label });
        }
        else
        {
            success = false;
            if (o is UnityEngine.Object unityObject)
            {
                Editor editor = Editor.CreateEditor(unityObject);
                if (editor != null)
                {
                    success = true;
                    editor.OnInspectorGUI();
                }
                else
                {
                    success = false;
                }
            }
            return o;
        }
    }

    public static object DrawCustomEditor(object o, out bool success, string label = null, bool drawDeafultEditorInstead = true)
    {
        if (o == null)
        {
            success = false;
            return null;
        }
        else
        {
            return DrawCustomEditor(o, out success, o?.GetType(), label, drawDeafultEditorInstead);
        }
    }

    private static void EnsureCache()
    {
        if (cache == null)
        {
            cache =
                TypeCache
                    .GetMethodsWithAttribute<GenericCustomEditorAttribute>()
                    .ToDictionary(
                        x => x.GetCustomAttribute<GenericCustomEditorAttribute>().type,
                        x => x);
        }
    }

   

    

    [GenericCustomEditor(typeof(Vector2))]
    public static Vector2 DrawVector2Editor(Vector2 vector, string label)
    {
        return EditorGUILayout.Vector2Field(label, vector);
    }

    [GenericCustomEditor(typeof(Vector3))]
    public static Vector3 DrawVector3Editor(Vector3 vector, string label)
    {
        return EditorGUILayout.Vector3Field(label, vector);
    }

    [GenericCustomEditor(typeof(Vector4))]
    public static Vector4 DrawVector4Editor(Vector4 vector, string label)
    {
        return EditorGUILayout.Vector4Field(label, vector);
    }

    [GenericCustomEditor(typeof(int))]
    public static int DrawIntEditor(int value, string label) => EditorGUILayout.IntField(label, value);

    [GenericCustomEditor(typeof(float))]
    public static float DrawFloatEditor(float value, string label) => EditorGUILayout.FloatField(label, value);

    [GenericCustomEditor(typeof(bool))]
    public static bool DrawBoolEditor(bool value, string label) => EditorGUILayout.Toggle(label, value, "Button");

    [GenericCustomEditor(typeof(string))]
    public static string DrawStringEditor(string value, string label) => EditorGUILayout.TextField(label, value);

    public static Enum AllEnumField<T>(Enum source, string label) where T : Attribute
    {
        var types = TypeCache.GetTypesWithAttribute<T>();

        List<GUIContent> guiList = new List<GUIContent>();
        Dictionary<string, Enum> outputMap = new Dictionary<string, Enum>();

        guiList.Add(new GUIContent("none"));
        outputMap["none"] = null;

        int selection = 0;
        int count = 1;

        foreach (var type in types)
        {
            foreach (var value in Enum.GetValues(type))
            {
                string entry = $"{type.Name}/{value.ToString()}";
                guiList.Add(new GUIContent(entry));
                outputMap[entry] = value as Enum;

                if (Equals(value, source))
                {
                    selection = count;
                }

                count++;
            }
        }

        int output = EditorGUILayout.Popup(
            new GUIContent(label),
            selection,
            guiList.ToArray());

        return outputMap[guiList[output].text];
    }
}