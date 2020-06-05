using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public static class GetDefault
{
    public static T GetDefaultValue<T>()
    {
        return default;
    }

    public static object GetDefaultValueFromType(Type type)
    {
        if (type == null)
        {
            return null;
        }
        else
        {
            return typeof(GetDefault).GetMethod(nameof(GetDefaultValue)).MakeGenericMethod(new Type[] { type }).Invoke(null, new object[] { });
        }
    }
}
