using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBinding.Wrappers
{
    public class IntWrapper : PrimitiveWrapper
    {
        public int value;

        public IntWrapper(int value)
        {
            this.value = value;
        }

        public override object Value => value;
    }
}
