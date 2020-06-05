using DynamicBinding.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DynamicBinding
{
    /// <summary>
    /// Maps a cell in the method bindings memory to an argument for the method binding.
    /// </summary>
    [Serializable]
    public class MemoryMethodBindingArgument : IMethodBindingArgument
    {
        public string argName;
        public string ArgName => argName;

        [SerializeField]
        EnumWrapper wrapper = new EnumWrapper(null);

        public Enum ArgumentKey
        {
            get => wrapper?.value;
            set => wrapper = new EnumWrapper(value);
        }

        public MemoryMethodBindingArgument()
        {

        }

        public MemoryMethodBindingArgument(ParameterInfo parameterInfo) : this(parameterInfo.Name)
        {
        }

        public MemoryMethodBindingArgument(string argumentName)
        {
            this.argName = argumentName;
        }

        public object GetArgValue(IReadOnlyDictionary<object, object> memoryMap)
        {
            try
            {
                return memoryMap[ArgumentKey];
            }
            catch
            {
                return null;
            }
        }

        public void Validate(MethodBindingValidationContext validation)
        {

        }
    }
}