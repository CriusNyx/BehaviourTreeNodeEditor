using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;

public class BlenderPostprocessor : AssetPostprocessor
{
    private void OnPostprocessGameObjectWithUserProperties(GameObject go,
        string[] propNames,
        object[] values)
    {
        foreach(var (name, value) in propNames.Zip(values))
        {
            foreach(var type in TypeCache.GetTypesWithAttribute<BlenderComponentAttribute>())
            {
                var attr = type.GetCustomAttribute<BlenderComponentAttribute>();
                if(name == attr.propertyName)
                {
                    attr.ProcessGameObject(type, go, name, value);
                }
            }
        }
    }
}