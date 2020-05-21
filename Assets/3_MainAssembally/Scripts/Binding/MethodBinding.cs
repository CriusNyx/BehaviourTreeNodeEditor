using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;

/// <summary>
/// Describes a method that can be serialized, and bound at runtime
/// </summary>
[Serializable]
public class MethodBinding
{
    public string className;
    public string methodName;
    [SerializeReference]
    public IMethodBindingArgument[] arguments;

    /// <summary>
    /// Create a new method binding
    /// </summary>
    public MethodBinding()
    {

    }

    /// <summary>
    /// Create a new method binding from the givin method info
    /// </summary>
    /// <param name="info"></param>
    public MethodBinding(MethodInfo info)
    {
        this.className = info.DeclaringType.ToString();
        this.methodName = info.Name;
        arguments =
            info
                .GetParameters()
                .Select(
                    x =>
                    {
                        if (x.GetCustomAttribute<ParamArrayAttribute>() != null)
                        {
                            return (IMethodBindingArgument)new ParamsMethodBindingArgument(x);
                        }
                        else
                        {
                            return (IMethodBindingArgument)new StaticMethodBindingArgument(x.Name, GetDefault.GetDefaultValueFromType(x.ParameterType));
                        }
                    })
                .ToArray();
        PruneArgs();
    }

    /// <summary>
    /// Create a new method binding for the specified class, method, and with the given arguments
    /// </summary>
    /// <param name="className"></param>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    public MethodBinding(string className, string methodName, params (string argName, object argValue)[] args)
    {
        this.className = className;
        this.methodName = methodName;
        arguments = args.Select(x => new StaticMethodBindingArgument(x.argName, x.argValue) as IMethodBindingArgument).ToArray();
    }

    /// <summary>
    /// Bind the method to the component on the given gameObject
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="memoryMap"></param>
    /// <param name="additionalArgs"></param>
    /// <returns></returns>
    public IEnumerable Bind(GameObject gameObject, IReadOnlyDictionary<object, object> memoryMap, IMethodBindingLog log, params (string argName, object argValue)[] additionalArgs)
    {
        return PrepareBinding(gameObject, memoryMap, log, additionalArgs) as IEnumerable;
    }

    /// <summary>
    /// Bind the method to the component on the given object, casting the output to IEnumerable<typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gameObject"></param>
    /// <param name="memoryMap"></param>
    /// <param name="additionalArgs"></param>
    /// <returns></returns>
    public IEnumerable<T> Bind<T>(GameObject gameObject, IReadOnlyDictionary<object, object> memoryMap, IMethodBindingLog log, params (string argName, object argValue)[] additionalArgs)
    {
        return PrepareBinding(gameObject, memoryMap, log, additionalArgs) as IEnumerable<T>;
    }

    /// <summary>
    /// Validate the arguments of the method binding. Will generate missing arguments
    /// </summary>
    public void ValidateArgs(string[] argumentOptions, Enum[] enumOptions)
    {
        foreach (var arg in arguments)
        {
            arg.Validate(new MethodBindingValidationContext(this, argumentOptions, enumOptions));
        }

        //Get method info. Stop if there is no method to bind
        MethodInfo methodInfo = GetMethodInfo();
        if (methodInfo == null)
        {
            return;
        }

        //Get known arguments, and arguments to ignore
        Dictionary<string, IMethodBindingArgument> currentArgs = arguments.ToDictionary(x => x.ArgName, x => x);
        HashSet<string> ignoreArgs = new HashSet<string>(GetIgnoreArgs());

        bool valid = true;

        foreach (var parameter in methodInfo.GetParameters())
        {
            if (!currentArgs.ContainsKey(parameter.Name) && !ignoreArgs.Contains(parameter.Name))
            {
                valid = false;
                currentArgs[parameter.Name] = new StaticMethodBindingArgument(parameter);
            }
        }

        if (!valid)
        {
            this.arguments = currentArgs.Values.ToArray();
        }
    }

    /// <summary>
    /// Get the type of the specified argument
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public Type GetTypeOfArg(IMethodBindingArgument arg)
    {
        return GetMethodInfo()?.GetParameters()?.FirstOrDefault(x => x.Name == arg.ArgName)?.ParameterType;
    }

    /// <summary>
    /// Remove extra arguments
    /// </summary>
    public void PruneArgs()
    {
        var methodInfo = GetMethodInfo();
        HashSet<string> methodInfoArgs = new HashSet<string>(methodInfo.GetParameters().Select(x => x.Name));
        HashSet<string> ignoreArgs = new HashSet<string>(GetIgnoreArgs());

        arguments = arguments.Where(x => methodInfoArgs.Contains(x.ArgName) && !ignoreArgs.Contains(x.ArgName)).ToArray();
    }

    /// <summary>
    /// Get a set of arguments to ignore
    /// </summary>
    /// <returns></returns>
    public string[] GetIgnoreArgs()
    {
        MethodInfo methodInfo = GetMethodInfo();
        if (methodInfo != null)
        {
            var attr = methodInfo.GetCustomAttribute<BindableMethodAttribute>();
            if (attr != null)
            {
                return attr.ignoreArgs;
            }
        }
        return new string[] { };
    }

    /// <summary>
    /// Create the binding for the method, and return it's output
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="memoryMap"></param>
    /// <param name="additionalArgs"></param>
    /// <returns></returns>
    private object PrepareBinding(GameObject gameObject, IReadOnlyDictionary<object, object> memoryMap, IMethodBindingLog log, params (string argName, object argValue)[] additionalArgs)
    {
        MethodInfo targetMethod = GetMethodInfo();

        log.AppendMethodCall($"{targetMethod.DeclaringType.Name}.{targetMethod.Name}");

        var args = GetArguments(targetMethod, memoryMap, log, additionalArgs);

        object component = gameObject.GetComponent(className);

        return targetMethod.Invoke(component, args);
    }

    private MethodInfo methodInfoCache;

    /// <summary>
    /// Get the method info for the target method
    /// </summary>
    /// <returns></returns>
    public MethodInfo GetMethodInfo()
    {
        if (methodInfoCache == null)
        {
            Type targetType = Type.GetType(className);
            var targetMethod = targetType.GetMethod(methodName);
            methodInfoCache = targetMethod;
        }
        return methodInfoCache;
    }

    /// <summary>
    /// Get the arguments in the correct order for a method call
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="memoryMap"></param>
    /// <param name="additionalArgs"></param>
    /// <returns></returns>
    public object[] GetArguments(MethodInfo methodInfo, IReadOnlyDictionary<object, object> memoryMap, IMethodBindingLog log, (string argName, object argValue)[] additionalArgs)
    {
        Dictionary<string, object> cache = arguments.ToDictionary(x => x.ArgName, x => x.GetArgValue(memoryMap));
        foreach (var arg in additionalArgs)
        {
            cache.Add(arg.argName, arg.argValue);
        }
        ParameterInfo[] paramters = methodInfo.GetParameters();
        return paramters
            .Select(
            x =>
            {
                var output = GetArgFromParameterInfo(x, cache);
                if (log != null)
                {
                    log.AppendArgument(x.Name, output);
                }
                return output;
            })
            .ToArray();
    }

    /// <summary>
    /// Get an argument object from the method info
    /// </summary>
    /// <param name="info"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public object GetArgFromParameterInfo(ParameterInfo info, Dictionary<string, object> values)
    {
        if (values.ContainsKey(info.Name))
        {
            return values[info.Name];
        }
        return null;
    }

    /// <summary>
    /// Print the method binding
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return methodName;
    }

    public bool HasParams
    {
        get
        {
            var methodInfo = GetMethodInfo();
            return
                //Any argument is params
                methodInfo.GetParameters().Any(x => x.GetCustomAttribute(typeof(ParamArrayAttribute), false) != null)
                //Method has params data source attribute
                && methodInfo.GetCustomAttribute<ParamsDataSourceAttribute>() != null;
        }
    }

    public ParamsDataSourceAttribute ParamsDataSource
    {
        get
        {
            return GetMethodInfo().GetCustomAttribute<ParamsDataSourceAttribute>();
        }
    }
}
