using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A method binding argument is used to get or create an argument for calling a specific method.
/// Method binding arguments can either be static, or get an argument from the execution context, such as getting an argument from the global memory, or local memory.
/// Method binding arguments may also map to a set of params for a method call.
/// </summary>
public interface IMethodBindingArgument
{
    string ArgName { get; }
    object GetArgValue(IReadOnlyDictionary<object, object> memoryMap);
    void Validate(MethodBindingValidationContext validation);
}