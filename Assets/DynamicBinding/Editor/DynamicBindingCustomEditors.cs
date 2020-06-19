using DynamicBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class DynamicBindingCustomEditors
{
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

        bool result = MethodBindingArgument.TryGetBindingArgumentTypeFromObjectType(argument.GetType(), out Enum bindingType);

        if (result)
        {
            if (bindingType is ChangeableMethodBindingArgumentType changeable)
            {
                var newBindingType =
                    (ChangeableMethodBindingArgumentType)EditorGUILayout.EnumPopup(
                        changeable,
                        GUILayout.Width(70));

                if (newBindingType != changeable)
                {
                    argument = MethodBindingArgument.BuildArgumentOfType(argument.ArgName, type, newBindingType);
                    change = true;
                }
            }
        }
        argument = GenericCustomEditors.DrawCustomEditor(argument, out _) as IMethodBindingArgument;

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
            newArgValue = GenericCustomEditors.DrawCustomEditor(staticArgument.ArgValue, out success, type, staticArgument.ArgName);
        }
        else
        {
            newArgValue = GenericCustomEditors.DrawCustomEditor(staticArgument.ArgValue, out success, staticArgument.ArgName);
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
        var output = GenericCustomEditors.AllEnumField<BindableEnumAttribute>(dynamicArgument.ArgumentKey, dynamicArgument.argName);
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
}