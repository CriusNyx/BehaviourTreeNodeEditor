using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using Object = UnityEngine.Object;

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

    /// <summary>
    /// Open or Create an asset at the specified path.
    /// Will generate the target path if it doesn't exist.
    /// Paths should be reletive tot he project root, and include the Assets folder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    public static T OpenOrCreateAsset<T>(string path) where T : ScriptableObject
    {
        string dir = GetParentPath(path);
        BuildAllDependantDirectories(dir);
        var output = AssetDatabase.LoadAssetAtPath<T>(path);
        if(output == null)
        {
            T newT = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(newT, path);
            EditorUtility.SetDirty(newT);
            AssetDatabase.SaveAssets();
            output = AssetDatabase.LoadAssetAtPath<T>(path);
        }
        return output;
    }

    public static void BuildAllDependantDirectories(string path)
    {
        foreach(string subPath in GetAllParentFolderPaths(path))
        {
            if (!AssetDatabase.IsValidFolder(subPath))
            {
                string parent = GetParentPath(subPath);
                string end = GetEndOfPath(subPath);
                AssetDatabase.CreateFolder(parent, end);
            }
        }
    }

    public static string[] GetAllParentFolderPaths(string path)
    {
        Stack<string> stack = new Stack<string>();
        while(path != "" && path != null)
        {
            stack.Push(path);
            path = GetParentPath(path);
        }
        return stack.ToArray();
    }

    public static string GetParentPath(string path)
    {
        int currentMax = 0;
        currentMax = Math.Max(currentMax, path.LastIndexOf('/'));
        currentMax = Math.Max(currentMax, path.LastIndexOf('\\'));
        return path.Substring(0, currentMax);
    }

    public static string GetEndOfPath(string path)
    {
        int currentMax = 0;
        currentMax = Math.Max(currentMax, path.LastIndexOf('/'));
        currentMax = Math.Max(currentMax, path.LastIndexOf('\\'));
        return path.Substring(currentMax + 1);
    }

    private static void Overwrite(string path, Object target)
    {
        var fileToWrite = Object.Instantiate(target);
        AssetDatabase.CreateAsset(fileToWrite, path);
        AssetDatabase.SaveAssets();
    }
}