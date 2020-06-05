using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBinding.Wrappers
{
    [System.Serializable]
    public class FloatWrapper : PrimitiveWrapper
    {
        public float value;

        public FloatWrapper(float value)
        {
            this.value = value;
        }

        public override object Value => value;
    }
}
