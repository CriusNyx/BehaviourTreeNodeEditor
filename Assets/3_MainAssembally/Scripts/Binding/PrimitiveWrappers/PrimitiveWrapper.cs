using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBinding.Wrappers
{
    [System.Serializable]
    public abstract class PrimitiveWrapper
    {
        public abstract object Value { get; }

        public static object GetWrapper(object source)
        {
            if (source is int i)
            {
                return new IntWrapper(i);
            }
            else if (source is bool b)
            {
                return new BoolWrapper(b);
            }
            else if (source is float f)
            {
                return new FloatWrapper(f);
            }
            else if (source is string s)
            {
                return new StringWrapper(s);
            }
            else if (source is Object o)
            {
                return new UnityEngineObjectWrapper(o);
            }
            else
            {
                return source;
            }
        }
    }
}
