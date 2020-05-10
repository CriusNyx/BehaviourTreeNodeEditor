using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[AttributeUsage(AttributeTargets.Method)]
public class ParamsDataSourceAttribute : Attribute
{
    public readonly Type dataSourceType;
    public readonly string dataSource;

    public ParamsDataSourceAttribute(Type dataSourceType, string dataSource)
    {
        this.dataSourceType = dataSourceType;
        this.dataSource = dataSource;
    }

    public IEnumerable<(string name, Type type)> GetParamsFromDataSource(MethodBinding binding)
    {
        MethodInfo targetMethod = dataSourceType.GetMethod(dataSource, (BindingFlags)(-1));
        var parameterInfos = targetMethod.GetParameters().ToArray();
        Dictionary<string, object> targetMethodArguments = parameterInfos.ToDictionary(x => x.Name, x => default(object));

        foreach (var argument in binding.arguments)
        {
            string name = argument.ArgName;
            targetMethodArguments[name] = argument.GetArgValue(new Dictionary<object, object>());
        }

        object[] arguments = new object[parameterInfos.Length];
        for (int i = 0; i < arguments.Length; i++)
        {
            arguments[i] = targetMethodArguments[parameterInfos[i].Name];
        }

        return (IEnumerable<(string name, Type type)>)targetMethod.Invoke(null, arguments);
    }
}
