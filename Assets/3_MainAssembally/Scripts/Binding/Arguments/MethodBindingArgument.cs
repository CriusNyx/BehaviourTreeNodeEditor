using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Contains additional methods for interfacing with method bindings.
/// </summary>
public static class MethodBindingArgument
{
    /// <summary>
    /// Gets the type of the binding argument for the object.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Enum GetBindingArgumentTypeFromObjectType(Type type)
    {
        if (type == typeof(StaticMethodBindingArgument))
        {
            return ChangeableMethodBindingArgumentType.Static;
        }
        else if (type == typeof(MemoryMethodBindingArgument))
        {
            return ChangeableMethodBindingArgumentType.Memory;
        }
        else if (type == typeof(ArgumentMethodBindingArgument))
        {
            return ChangeableMethodBindingArgumentType.Argument;
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

    /// <summary>
    /// Attempts to get the type of the binding argument for the object.
    /// Returns true if it is successful.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="outputType"></param>
    /// <returns></returns>
    public static bool TryGetBindingArgumentTypeFromObjectType(Type type, out Enum outputType)
    {
        try
        {
            outputType = GetBindingArgumentTypeFromObjectType(type);
            return true;
        }
        catch (ArgumentException)
        {
            outputType = default;
            return false;
        }
    }

    /// <summary>
    /// Builds a method binding out of the specified method info.
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="argumentName"></param>
    /// <param name="bindingType"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Builds a method binding argument for the specified argument of the specified type
    /// </summary>
    /// <param name="argumentName"></param>
    /// <param name="type"></param>
    /// <param name="bindingType"></param>
    /// <returns></returns>
    public static IMethodBindingArgument BuildArgumentOfType(string argumentName, Type type, Enum bindingType)
    {
        if (bindingType is ChangeableMethodBindingArgumentType changable)
        {
            switch (bindingType)
            {
                case ChangeableMethodBindingArgumentType.Static:
                    return new StaticMethodBindingArgument(argumentName, GetDefault.GetDefaultValueFromType(type));
                case ChangeableMethodBindingArgumentType.Memory:
                    return new MemoryMethodBindingArgument(argumentName);
                case ChangeableMethodBindingArgumentType.Argument:
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
}