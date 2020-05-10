using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StringWrapper : PrimitiveWrapper
{
    public string value;

    public StringWrapper(string value)
    {
        this.value = value;
    }

    public override object Value => value;
}
