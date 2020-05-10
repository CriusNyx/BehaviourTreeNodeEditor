using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(validOn: AttributeTargets.Method)]
public class AIMethodAttribute : BindableMethodAttribute
{
    public readonly string tooltip;

    public AIMethodAttribute(string tooltip = "") : base("context")
    {
        this.tooltip = tooltip;
    }
}