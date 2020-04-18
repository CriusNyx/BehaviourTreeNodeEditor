using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEditor
{
    public class ActiveControlSet
    {
        private Dictionary<Type, object> activeControls = new Dictionary<Type, object>();

        public void Set<T>(T t)
        {
            activeControls[typeof(T)] = t;
        }

        public void Remove<T>()
        {
            activeControls.Remove(typeof(T));
        }

        public T Get<T>()
        {
            if (activeControls.ContainsKey(typeof(T)))
            {
                return (T)activeControls[typeof(T)];
            }
            else
            {
                return default(T);
            }
        }
    }
}
