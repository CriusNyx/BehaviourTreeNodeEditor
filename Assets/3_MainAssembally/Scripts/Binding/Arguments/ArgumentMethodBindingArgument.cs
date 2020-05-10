﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ArgumentMethodBindingArgument : IMethodBindingArgument
{
    [SerializeField]
    private string argName;
    public string ArgName => argName;

    public string identifier;

    public string[] ArgumentOptions { get; private set; } = new string[] { };

    public ArgumentMethodBindingArgument()
    {
    }

    public ArgumentMethodBindingArgument(string argumentName)
    {
        this.argName = argumentName;
    }

    public ArgumentMethodBindingArgument(ParameterInfo parameterInfo) : this(parameterInfo.Name)
    {
    }

    public object GetArgValue(IReadOnlyDictionary<object, object> memoryMap)
    {
        return memoryMap[identifier];
    }

    private void SetArgumentOptions(string[] argumentOptions) 
    {
        this.ArgumentOptions = argumentOptions;
    }

    public void Validate(MethodBindingValidationContext validation)
    {
        SetArgumentOptions(validation.paramOptions);
    }
}
