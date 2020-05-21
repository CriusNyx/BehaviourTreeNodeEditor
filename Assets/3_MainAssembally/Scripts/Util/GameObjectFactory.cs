using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectFactory
{
    public static GameObject Create(
        string name = "new GameObject",
        Vector3 position = default, 
        Quaternion rotation = default, 
        Transform parent = default)
    {
        GameObject output = new GameObject(name);
        output.transform.position = position;
        output.transform.rotation = rotation;
        output.transform.parent = parent;

        return output;
    }

    public static T Create<T>(
        string name = "new GameObject",
        Vector3 position = default,
        Quaternion rotation = default,
        Transform parent = default) where T : Component
    {
        var output = Create(name, position, rotation, parent);
        return output.AddComponent<T>();
    }
}