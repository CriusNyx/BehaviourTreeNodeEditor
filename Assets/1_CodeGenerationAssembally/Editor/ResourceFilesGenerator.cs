using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class ResourceFilesGenerator : AssetPostprocessor
{
    private const string classStub = @"
namespace GeneratedCode{{
    public class ResourceFiles{{        
{0}
    }}
}}
";

    private const string fileLine = "            public const string {0} = \"{1}\";";

    //private void OnPostprocessAllAsset()
    //{

    //}

    //[MenuItem("Debug/TestResourceFilesGeneator")]
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        HashSet<string> resources = new HashSet<string>();
        Crawl(
            resourcesPath,
            resourcesPath,
            (x) => Path.GetExtension(x) != ".meta",
            (x) => resources.Add(x));

        string output =
            string.Format(
                classStub,
                string.Join(
                    "\n",
                    resources.Select(
                        x => string.Format(
                            fileLine,
                            EscapeResources(x),
                            x)).ToArray()));

        string outputPath = Path.Combine(Application.dataPath, "2_GeneratedCodeAssembally/Scripts/ResourceFiles.cs");

        if (!File.Exists(outputPath) || File.ReadAllText(outputPath) != output)
        {
            File.WriteAllText(outputPath, output);
            AssetDatabase.Refresh();
        }
    }

    private static string EscapeResources(string path)
    {
        return path.Replace("\\", "/").Replace("/", "__").Replace(".", "_").Replace(" ", "");
    }

    private static void Crawl(string resourcesPath, string path, Func<string, bool> validateFile, Action<string> processFile)
    {
        resourcesPath = EscapePath(resourcesPath);
        path = EscapePath(path);

        foreach (var file in Directory.GetFiles(path))
        {
            var fileEscaped = EscapePath(file);
            fileEscaped = fileEscaped.Replace(resourcesPath, "");
            if (validateFile(fileEscaped))
            {
                processFile(
                    fileEscaped
                    .Replace(Path.GetExtension(fileEscaped), "")
                    .Trim('/'));
            }
        }
        foreach (var subdir in Directory.GetDirectories(path))
        {
            Crawl(resourcesPath, subdir, validateFile, processFile);
        }
    }

    private static string EscapePath(string path)
    {
        return path.Replace("\\", "/");
    }
}