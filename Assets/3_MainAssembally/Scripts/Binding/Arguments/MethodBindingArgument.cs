using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public static class MethodBindingArgument
{
    public static Enum GetBindingTypeFromObjectType(Type type)
    {
        if (type == typeof(StaticMethodBindingArgument))
        {
            return ChangeableMethodBindingType.Static;
        }
        else if (type == typeof(MemoryMethodBindingArgument))
        {
            return ChangeableMethodBindingType.Memory;
        }
        else if (type == typeof(ArgumentMethodBindingArgument))
        {
            return ChangeableMethodBindingType.Argument;
        }
        else if (type == typeof(ParamsMethodBindingArgument))
        {
            return NonChangeableMethodBindingType.Params;
        }
        else
        {
            throw new ArgumentException($"Unknown Type {type.ToString()}");
        }
    }

    public static bool TryGetBindingTypeFromObjectType(Type type, out Enum outputType)
    {
        try
        {
            outputType = GetBindingTypeFromObjectType(type);
            return true;
        }
        catch (ArgumentException)
        {
            outputType = default;
            return false;
        }
    }

    public static IMethodBindingArgument BuildArgumentOfType(MethodInfo methodInfo, string argumentName, Enum bindingType)
    {
        ParameterInfo argInfo = methodInfo.GetParameters().FirstOrDefault(x => x.Name == argumentName);
        if (argInfo == null)
        {
            throw new ArgumentException($"The Argument Name {argumentName} does not belong to method {methodInfo.DeclaringType}.{methodInfo.Name}");
        }

        Type type = argInfo.ParameterType;

        return BuildArgumentOfType(argumentName, type, bindingType);
    }

    public static IMethodBindingArgument BuildArgumentOfType(string argumentName, Type type, Enum bindingType)
    {
        if (bindingType is ChangeableMethodBindingType changable)
        {
            switch (bindingType)
            {
                case ChangeableMethodBindingType.Static:
                    return new StaticMethodBindingArgument(argumentName, GetDefault.GetDefaultValueFromType(type));
                case ChangeableMethodBindingType.Memory:
                    return new MemoryMethodBindingArgument(argumentName);
                case ChangeableMethodBindingType.Argument:
                    return new ArgumentMethodBindingArgument(argumentName);
            }
        }
        else if (bindingType is NonChangeableMethodBindingType nonChangeable)
        {
            switch (bindingType)
            {
                case NonChangeableMethodBindingType.Params:
                    return new ArgumentMethodBindingArgument(argumentName);
            }
        }
        throw new ArgumentException($"Unknown binding type {bindingType}");
    }

    public static IMethodBindingArgument BuildParamsType(string argumentName, Type type)
    {
        return new ParamsMethodBindingArgument(argumentName);
    }
}