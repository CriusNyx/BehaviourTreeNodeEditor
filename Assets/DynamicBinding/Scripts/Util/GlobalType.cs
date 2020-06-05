using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GlobalType
{
    private static Dictionary<string, Type> cache 
        = new Dictionary<string, Type>();

    public static Type GetType(string typeName)
    {
        if (!cache.ContainsKey(typeName))
        {
            cache[typeName] = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetType(typeName)).FirstOrDefault(x => x != null);
        }
        return cache[typeName];
    }
}
