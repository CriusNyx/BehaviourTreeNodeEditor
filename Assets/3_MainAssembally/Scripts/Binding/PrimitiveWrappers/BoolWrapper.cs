using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoolWrapper : PrimitiveWrapper
{
    public bool value;

    public BoolWrapper(bool value)
    {
        this.value = value;
    }

    public override object Value => value;
}