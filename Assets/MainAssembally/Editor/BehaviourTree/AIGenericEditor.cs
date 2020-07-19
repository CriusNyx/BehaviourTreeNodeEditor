using DynamicBinding;
using GameEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class AIGenericEditor
{
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
                var newType = GenericCustomEditors.AllEnumField<TypeEnumAttribute>(x.type, "");
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
}
