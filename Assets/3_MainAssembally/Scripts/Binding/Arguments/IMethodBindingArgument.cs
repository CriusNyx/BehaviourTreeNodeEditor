using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMethodBindingArgument
{
    string ArgName { get; }
    object GetArgValue(IReadOnlyDictionary<object, object> memoryMap);
    void Validate(MethodBindingValidationContext validation);
}