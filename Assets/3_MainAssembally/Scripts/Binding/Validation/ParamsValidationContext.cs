

using System;
using System.Collections.Generic;

public class ParamsValidationContext : MethodBindingValidationContext
{
    Dictionary<string, Type> argumentTypes = new Dictionary<string, Type>();

    public ParamsValidationContext(MethodBinding methodBinding, string[] paramOptions, Enum[] enumOptions, Dictionary<string, Type> argumentTypes)
        : base(methodBinding, paramOptions, enumOptions)
    {
        this.argumentTypes = argumentTypes;
    }

    public override Type GetTypeOfArgument(string argumentName)
    {
        if (argumentTypes.ContainsKey(argumentName))
        {
            return argumentTypes[argumentName];
        }
        else
        {
            return base.GetTypeOfArgument(argumentName);
        }
    }
}
