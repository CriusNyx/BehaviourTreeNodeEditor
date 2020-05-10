using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
public class ParamsMethodBindingArgument : IMethodBindingArgument
{
    public string ArgName => argName;
    [SerializeField]
    private string argName;

    private Type[] types;
    [SerializeField]
    private string[] names;
    [SerializeReference]
    private IMethodBindingArgument[] arguments;

    public (Type type, string name, IMethodBindingArgument arguments)[] Parameters
    {
        get
        {
            if (names == null)
            {
                names = new string[] { };
            }
            if (arguments == null)
            {
                arguments = new IMethodBindingArgument[] { };
            }
            if(types == null)
            {
                types = new Type[] { };
            }
            if(names.Length == arguments.Length)
            {
                if(types.Length == names.Length)
                {
                    return types.Zip(names, (x, y) => (x, y)).Zip(arguments, (x, y) => (x.x, x.y, y)).ToArray();
                }
                else
                {
                    return names.Zip(arguments, (x, y) => ((Type)null, x, y)).ToArray();
                }
            }
            else
            {
                return new (Type, string, IMethodBindingArgument)[] { };
            }
        }
        set
        {
            types = value.Select(x => x.type).ToArray();
            names = value.Select(x => x.name).ToArray();
            arguments = value.Select(x => x.arguments).ToArray();
        }
    }

    public ParamsMethodBindingArgument()
    {

    }

    public ParamsMethodBindingArgument(string argumentName)
    {
        this.argName = argumentName;
    }

    public ParamsMethodBindingArgument(ParameterInfo argInfo) : this(argInfo.Name)
    {
    }

    public object GetArgValue(IReadOnlyDictionary<object, object> memoryMap)
    {
        return Parameters
            .Select(x => (x.name, x.arguments.GetArgValue(memoryMap)))
            .ToArray();
    }

    private void ValidateParams(MethodBindingValidationContext validation, IEnumerable<(string name, Type type)> paramaterDefinitions)
    {
        Dictionary<string, IMethodBindingArgument> argumentMap = new Dictionary<string, IMethodBindingArgument>();

        foreach (var parameter in Parameters)
        {
            argumentMap[parameter.name] = parameter.arguments;
        }
        List<(Type type, string name, IMethodBindingArgument value)> output = new List<(Type type, string name, IMethodBindingArgument value)>();

        foreach (var def in paramaterDefinitions)
        {
            string parameterName = def.name;
            Type parameterType = def.type;

            if (argumentMap.ContainsKey(parameterName))
            {
                var arg = argumentMap[parameterName];
                output.Add((def.type, def.name, arg));
            }
            else
            {
                output.Add((
                    def.type, 
                    def.name, 
                    new StaticMethodBindingArgument(
                        def.name, 
                        GetDefault.GetDefaultValueFromType(parameterType))));
            }
        }

        Parameters = output.ToArray();

        foreach(var def in Parameters)
        {
            def.arguments.Validate(
                new ParamsValidationContext(
                    validation.methodBinding,
                    validation.paramOptions,
                    validation.enumOptions,
                    Parameters.ToDictionary(x => x.name, x => x.type)
                    ));
        }
    }

    public void Validate(MethodBindingValidationContext validation)
    {
        var binding = validation.methodBinding;

        if (binding.HasParams)
        {
            var paramsAttr = binding.ParamsDataSource;
            ValidateParams(validation, paramsAttr.GetParamsFromDataSource(binding));
        }
    }
}