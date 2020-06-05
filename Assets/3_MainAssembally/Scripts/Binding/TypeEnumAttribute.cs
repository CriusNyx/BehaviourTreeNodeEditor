using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicBinding
{
    public class TypeEnumAttribute : ValidatableTypeAttribute
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

        public override bool IsValid(Type ownerType, out Exception e)
        {
            MethodInfo info = resolutionClass.GetMethod(resolutionMethod);

            List<string> errorReasons = new List<string>();

            if (!ValidateMethodReturn(info, typeof(Type), out string e1))
                errorReasons.Add(e1);

            if (!ValidateMethodHasSignature(info, new[] { typeof(Enum) }, out string e2))
                errorReasons.Add(e2);

            return ConditionallyReturnError($"{info.DeclaringType.Name}.{info.Name} Did not validate", errorReasons, out e);
        }

        public class MethodSignatureIsNotValidException : Exception
        {
            public MethodSignatureIsNotValidException(string message) : base(message)
            {

            }
        }
    }
}