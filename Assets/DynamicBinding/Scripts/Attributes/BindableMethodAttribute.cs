using GameEngine.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBinding
{
    /// <summary>
    /// A bindable method is one that can have a method bidning bound to it.
    /// Generally, you should not use this attribute directly. Instead, you should subclass it to suit your specific needs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class BindableMethodAttribute : ValidatableMethodAttribute
    {
        public string[] ignoreArgs;

        public BindableMethodAttribute(params string[] ignoreArgs)
        {
            this.ignoreArgs = ignoreArgs;
        }
    }
}