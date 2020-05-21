using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class CreateMenuExtensions
{
    [MenuItem("GameObject/Create Other/At Origin")]
    public static void CreateAtOrigin()
    {
        new GameObject("GameObject");
    }
}
