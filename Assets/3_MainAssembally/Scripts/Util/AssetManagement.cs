using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class AssetManagement
{
    public static void CreateOrOverwriteAsset(string path, Object target, bool replaceIfNeeded = false)
    {
        var existingFile = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (existingFile != null)
        {
            var objectType = target.GetType();
            if (existingFile.GetType() != objectType)
            {
                Overwrite(path, target);
            }
            else
            {
                foreach (var field in objectType.GetFields((BindingFlags)(-1)))
                {
                    if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null || field.GetCustomAttribute<SerializeReference>() != null)
                    {
                        field.SetValue(existingFile, field.GetValue(target));
                    }
                }
            }
            EditorUtility.SetDirty(existingFile);
            AssetDatabase.SaveAssets();
        }
        else
        {
            Overwrite(path, target);
        }
    }

    private static void Overwrite(string path, Object target)
    {
        var fileToWrite = Object.Instantiate(target);
        AssetDatabase.CreateAsset(fileToWrite, path);
        AssetDatabase.SaveAssets();
    }
}