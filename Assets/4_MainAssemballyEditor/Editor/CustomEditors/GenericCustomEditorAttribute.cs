using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(validOn: AttributeTargets.Method)]
public class GenericCustomEditorAttribute : Attribute
{
    public readonly Type type;

    public GenericCustomEditorAttribute(Type type)
    {
        this.type = type;
    }
}
