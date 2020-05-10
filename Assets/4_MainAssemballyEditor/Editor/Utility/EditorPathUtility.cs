using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class EditorPathUtility
{
    public static string SystemPathToProjectPath(string path)
    {
        return "Assets" + path.Replace(Application.dataPath, "");
    }
}