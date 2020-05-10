using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindableMethodAttribute : Attribute
{
    public string[] ignoreArgs;

    public BindableMethodAttribute(params string[] ignoreArgs)
    {
        this.ignoreArgs = ignoreArgs;
    }
}