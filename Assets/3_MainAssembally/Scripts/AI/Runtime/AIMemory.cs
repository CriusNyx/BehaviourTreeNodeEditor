using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIMemory : MonoBehaviour, IReadOnlyDictionary<Enum, object>
{
    Dictionary<Enum, (float startTime, float timeout, object value)> nodeData = new Dictionary<Enum, (float startTime, float timeout, object value)>();

    public IEnumerable<Enum> Keys => throw new NotImplementedException();

    public IEnumerable<object> Values => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public object this[Enum key] => Get(key).value;

    public (float startTime, float timeout, object value) Get(Enum key)
    {
        EvaluateKey(key);
        if (nodeData.ContainsKey(key))
        {
            var myData = nodeData[key];
            var parent = GetParent();
            if(parent == null)
            {
                return myData;
            }
            else
            {
                var parentData = parent.Get(key);
                return (parentData.startTime > myData.startTime) ? parentData : myData;
            }
        }
        else
        {
            var parent = GetParent();
            if(parent != null)
            {
                return parent.Get(key);
            }
            else
            {
                return (-1, -1, null);
            }
        }
    }

    public void Set(Enum key, float timeout, object myValue)
    {
        nodeData[key] = (Time.time, timeout, myValue);
    }

    public void ResetTimeout(Enum key)
    {
        if (nodeData.ContainsKey(key))
        {
            var myData = nodeData[key];
            nodeData[key] = (Time.time, myData.timeout, myData.value);
        }
    }

    public void SetTimeout(Enum key, float timeout)
    {
        if (nodeData.ContainsKey(key))
        {
            var myData = nodeData[key];
            nodeData[key] = (Time.time, timeout, myData.value);
        }
    }

    public void Remove(Enum key)
    {
        nodeData.Remove(key);
    }

    public void RemoveRecursive(Enum key)
    {
        Remove(key);
        GetParent()?.RemoveRecursive(key);
    }

    private void EvaluateKey(Enum key)
    {
        if (nodeData.ContainsKey(key))
        {
            (float startTime, float timeout, object value) = nodeData[key];
            if (Time.time > startTime + timeout)
            {
                nodeData.Remove(key);
            }
        }
    }

    private AIMemory GetParent()
    {
        return transform?.parent?.GetComponentInParent<AIMemory>();
    }

    public bool ContainsKey(Enum key) => throw new NotImplementedException();

    public bool TryGetValue(Enum key, out object value) => throw new NotImplementedException();

    public IEnumerator<KeyValuePair<Enum, object>> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}