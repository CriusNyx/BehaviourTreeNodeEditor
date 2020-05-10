﻿using UnityEngine;

public class UnityEngineObjectWrapper : PrimitiveWrapper
{
    public Object value;

    public UnityEngineObjectWrapper(Object value)
    {
        this.value = value;
    }

    public override object Value => value;
}
