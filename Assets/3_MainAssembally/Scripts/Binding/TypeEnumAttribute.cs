using System;
using System.Reflection;

public class TypeEnumAttribute : Attribute
{
    public Type resolutionClass;
    public string resolutionMethod;

    public TypeEnumAttribute(Type resolutionClass, string resolutionMethod)
    {
        this.resolutionClass = resolutionClass;
        this.resolutionMethod = resolutionMethod;
    }

    private Type _ResolveType(Enum typeEnum)
    {
        return resolutionClass
            ?.GetMethod(resolutionMethod)
            ?.Invoke(
                null, 
                new object[] { 
                    typeEnum 
                })
            as Type;
    }

    public static Type ResolveType(Enum typeEnum)
    {
        return typeEnum
            ?.GetType()
            ?.GetCustomAttribute<TypeEnumAttribute>()
            ?._ResolveType(typeEnum);
    }
}