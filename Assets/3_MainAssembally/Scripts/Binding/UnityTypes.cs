using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[TypeEnum(typeof(ResolveUnityType), nameof(ResolveUnityType.ResolveType))]
public enum UnityTypes
{
    Vector2,
    Vector3,
    Vector4,
}

public static class ResolveUnityType
{
    public static Type ResolveType(Enum type)
    {
        if(type is UnityTypes unityType)
        {
            switch (unityType)
            {
                case UnityTypes.Vector2:
                    return typeof(Vector2);
                case UnityTypes.Vector3:
                    return typeof(Vector3);
                case UnityTypes.Vector4:
                    return typeof(Vector4);
                default:
                    return null;
            }
        }
        return null;
    }
}