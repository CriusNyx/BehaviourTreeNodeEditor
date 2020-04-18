using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class XMLContainer
{
    public XMLContainer(object o)
    {
        Set(o);
    }

    [SerializeField]
    private byte[] data;

    private object cache = null;

    public void Set<T>(T t)
    {
        cache = null;
        data = t?.ToXmlCompressed<T>();
    }

    public T Get<T>()
    {
        if(cache == null)
        {
            if(data != null)
            {
                cache = data.TFromXmlCompressed<T>();
            }
        }
        return (T)cache;
    }
}