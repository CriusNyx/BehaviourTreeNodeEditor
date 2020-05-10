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

    [GenericCustomEditor(typeof(MethodBinding))]
    public static MethodBinding DrawMethodBindingEditor(MethodBinding binding, string label)
    {
        GUILayout.Label($"{binding.className}.{binding.methodName}");

        List<IMethodBindingArgument> newArguments = new List<IMethodBindingArgument>();
        bool replaceArguments = false;

        foreach (var arg in binding.arguments)
        {
            GUILayout.BeginHorizontal();
            {
                newArguments.Add(DrawArgumentEditor(binding, arg, "", out bool change));

                if (change)
                {
                    replaceArguments = true;
                }
            }
            GUILayout.EndHorizontal();
        }

        if (replaceArguments)
        {
            binding.arguments = newArguments.ToArray();
        }

        if (GUILayout.Button("Prune Args"))
        {
            binding.PruneArgs();
        }

        return binding;
    }

    public static IMethodBindingArgument DrawArgumentEditor(MethodBinding binding, IMethodBindingArgument argument, string label, out bool change)
        => DrawArgumentEditor(
            binding
                ?.GetMethodInfo()
                ?.GetParameters()
                ?.FirstOrDefault(x => x.Name == argument.ArgName)
                ?.ParameterType,
            argument,
            label,
            out change);

    public static IMethodBindingArgument DrawArgumentEditor(Type type, IMethodBindingArgument argument, string label, out bool change)
    {
        change = false;

        bool result = MethodBindingArgument.TryGetBindingTypeFromObjectType(argument.GetType(), out Enum bindingType);

        if (result)
        {
            if (bindingType is ChangeableMethodBindingType changeable)
            {
                var newBindingType =
                    (ChangeableMethodBindingType)EditorGUILayout.EnumPopup(
                        changeable,
                        GUILayout.Width(70));

                if (newBindingType != changeable)
                {
                    argument = MethodBindingArgument.BuildArgumentOfType(argument.ArgName, type, newBindingType);
                    change = true;
                }
            }
        }
        argument = DrawCustomEditor(argument, out _) as IMethodBindingArgument;

        return argument;
    }

    [GenericCustomEditor(typeof(StaticMethodBindingArgument))]
    public static StaticMethodBindingArgument DrawStaticMethodBindingArgumentEditor(StaticMethodBindingArgument staticArgument, string label)
    {
        Type type = staticArgument.ArgumentType;
        bool success;
        object newArgValue;

        if (type != null)
        {
            newArgValue = DrawCustomEditor(staticArgument.ArgValue, out success, type, staticArgument.ArgName);
        }
        else
        {
            newArgValue = DrawCustomEditor(staticArgument.ArgValue, out success, staticArgument.ArgName);
        }

        if (success)
        {
            staticArgument.ArgValue = newArgValue;
        }
        return staticArgument;
    }

    [GenericCustomEditor(typeof(MemoryMethodBindingArgument))]
    public static MemoryMethodBindingArgument DrawDynamicethodBindingArgumentEditor(MemoryMethodBindingArgument dynamicArgument, string label)
    {
        var output = AllEnumField<BindableEnumAttribute>(dynamicArgument.ArgumentKey, dynamicArgument.argName);
        if (output != dynamicArgument.ArgumentKey)
        {
            dynamicArgument.ArgumentKey = output;
        }
        return dynamicArgument;
    }

    [GenericCustomEditor(typeof(ArgumentMethodBindingArgument))]
    public static ArgumentMethodBindingArgument DrawArgumentMethodBindingArgumentEditor(ArgumentMethodBindingArgument argumentArgument, string label)
    {
        int indexOfString = Array.IndexOf(argumentArgument.ArgumentOptions, argumentArgument.identifier);
        int newSelection = EditorGUILayout.Popup(indexOfString, argumentArgument.ArgumentOptions.Select(x => new GUIContent(x)).ToArray());
        if (newSelection != indexOfString)
        {
            argumentArgument.identifier = argumentArgument.ArgumentOptions[newSelection];
        }
        return argumentArgument;
    }

    [GenericCustomEditor(typeof(ParamsMethodBindingArgument))]
    public static ParamsMethodBindingArgument DrawParamsMethodBindingArgumentEditor(ParamsMethodBindingArgument paramsArgument, string label)
    {
        GUILayout.BeginVertical();

        List<(Type type, string name, IMethodBindingArgument arg)> output = new List<(Type type, string name, IMethodBindingArgument arg)>();

        foreach (var element in paramsArgument.Parameters)
        {
            var newValue = DrawArgumentEditor(element.type, element.arguments, "", out _);
            output.Add((element.type, element.name, newValue));
        }

        paramsArgument.Parameters = output.ToArray();

        GUILayout.EndVertical();

        return paramsArgument;
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

    [GenericCustomEditor(typeof(AITreeDefinition))]
    public static AITreeDefinition DrawDefinitionEditor(AITreeDefinition value, string label)
    {
        GUILayout.Label(label);
        GUILayout.Label("AI Tree Definition");
        value.name = EditorGUILayout.TextField("Name", value.name);

        value.arguments = EditorGUICustomUtility.DrawArrayEditor(
            value.arguments,
            (x) =>
            {
                var newType = AllEnumField<TypeEnumAttribute>(x.type, "");
                var newValue = EditorGUILayout.TextField("", x.value);
                if (newType != x.type || newValue != x.value)
                {
                    return new AITreeDefinitionArgument(newType, newValue);
                }
                else
                {
                    return x;
                }
            },
            "Add Argument",
            () => new AITreeDefinitionArgument(null, ""),
            false);

        return value;
    }

    [GenericCustomEditor(typeof(AITreeAsset))]
    public static AITreeAsset DrawAITreeAssetEditor(AITreeAsset value, string label)
    {
        return EditorGUILayout.ObjectField(label, value, typeof(AITreeAsset), false) as AITreeAsset;
    }

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