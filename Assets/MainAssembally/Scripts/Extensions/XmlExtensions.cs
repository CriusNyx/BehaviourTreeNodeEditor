using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public static class XmlExtensions
{
    public static byte[] ToXmlCompressed<T>(this T t)
    {
        var type = typeof(T);
        XmlSerializer serializer = new XmlSerializer(type);
        using (StringWriter sw = new StringWriter())
        {
            serializer.Serialize(sw, t);
            string output = sw.ToString();
            return output.Zip();
        }
    }

    public static T TFromXmlCompressed<T>(this byte[] bytes)
    {
        var type = typeof(T);
        try
        {
            string text = bytes.Unzip();
            XmlSerializer serializer = new XmlSerializer(type);
            using (StringReader sr = new StringReader(text))
            {
                return (T)serializer.Deserialize(sr);
            }
        }
        catch
        {
            return default(T);
        }
    }
}