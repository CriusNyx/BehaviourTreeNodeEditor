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
        output.transform.parent = parent;
        if (parent == null)
        {
            output.transform.position = position;
            output.transform.rotation = rotation;
        }
        else
        {
            output.transform.localPosition = position;
            output.transform.localRotation = rotation;
        }

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

    public static GameObject Instantiate(
        GameObject original,
        string name = null,
        Vector3 position = default,
        Quaternion rotation = default,
        Transform parent = default)
    {
        GameObject output = GameObject.Instantiate(original);
        if (name != null)
        {
            output.name = name;
        }
        output.transform.parent = parent;
        if (parent == null)
        {
            output.transform.position = position;
            output.transform.rotation = rotation;
        }
        else
        {
            output.transform.localPosition = position;
            output.transform.localRotation = rotation;
        }

        return output;
    }
}