using DynamicBinding.Wrappers;
using System;
using UnityEngine;

namespace GameEngine.AI
{
    [Serializable]
    public class AITreeDefinitionArgument
    {
        [SerializeField]
        private EnumWrapper typeWrapper = null;
        public Enum type
        {
            get => typeWrapper?.value;
            set => typeWrapper = new EnumWrapper(value);
        }
        public string value = "";

        public AITreeDefinitionArgument()
        {

        }

        public AITreeDefinitionArgument(Enum type, string value)
        {
            this.type = type;
            this.value = value;
        }
    }
}