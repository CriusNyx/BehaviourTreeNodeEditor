using System;
using System.Linq;

namespace DynamicBinding
{
    public class MethodBindingValidationContext
    {
        public readonly MethodBinding methodBinding;
        public readonly string[] paramOptions;
        public readonly Enum[] enumOptions;

        public MethodBindingValidationContext(MethodBinding methodBinding, string[] paramOptions, Enum[] enumOptions)
        {
            this.methodBinding = methodBinding;
            this.paramOptions = paramOptions;
            this.enumOptions = enumOptions;
        }

        public virtual Type GetTypeOfArgument(string argumentName)
        {
            try
            {
                var info = methodBinding?.GetMethodInfo();
                return info?.GetParameters()?.FirstOrDefault(x => x.Name == argumentName)?.ParameterType;
            }
            catch
            {
                return null;
            }
        }
    }
}