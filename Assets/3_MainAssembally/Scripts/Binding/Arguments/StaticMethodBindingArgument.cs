using DynamicBinding.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DynamicBinding
{
    [Serializable]
    public class StaticMethodBindingArgument : IMethodBindingArgument
    {
        [SerializeField]
        private string argName;
        [SerializeReference]
        private object argValue;

        [SerializeField]
        private Type type;

        public Type ArgumentType => type;

        public object ArgValue
        {
            get
            {
                return GetArgValue(null);
            }
            set
            {
                this.argValue = PrimitiveWrapper.GetWrapper(value);
            }
        }

        public string ArgName => argName;

        public StaticMethodBindingArgument(ParameterInfo info)
        {
            this.argName = info.Name;
            if (info.ParameterType == typeof(string))
            {
                this.ArgValue = "";
            }
            else
            {
                this.ArgValue = GetDefault.GetDefaultValueFromType(info.ParameterType);
            }
        }

        public StaticMethodBindingArgument(string argName, object argValue)
        {
            this.argName = argName;
            this.ArgValue = argValue;
        }

        public object GetArgValue(IReadOnlyDictionary<object, object> memoryMap)
        {
            if (argValue is PrimitiveWrapper wrapper)
            {
                return wrapper.Value;
            }
            else
            {
                return argValue;
            }
        }

        public void Validate(MethodBindingValidationContext validation)
        {
            this.type = validation.GetTypeOfArgument(ArgName);

            if (ArgValue == null)
            {
                if (type != null)
                {
                    if (type == typeof(string))
                    {
                        ArgValue = "";
                    }
                    else if (type.IsPrimitive || type.IsValueType)
                    {
                        ArgValue = GetDefault.GetDefaultValueFromType(type);
                    }
                }
            }
        }
    }
}
