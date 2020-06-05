using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    public static T EnsureComponent<T>(this GameObject gameObject) where T : Component
    {
        T output = gameObject.GetComponent<T>();
        if(output == null)
        {
            output = gameObject.AddComponent<T>();
        }
        return output;
    }

    public static T EnsureComponent<T>(this Component component) where T : Component 
        => EnsureComponent<T>(component.gameObject);
}