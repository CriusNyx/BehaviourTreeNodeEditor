using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameEngine.Validation
{
    public class ValidateAttributes
    {
        [UnityEditor.Callbacks.DidReloadScripts]
        static void Validate()
        {
            foreach (var type in TypeCache.GetTypesWithAttribute<ValidatableTypeAttribute>())
            {
                var attr = type.GetCustomAttribute<ValidatableTypeAttribute>();
                if (!attr.IsValid(type, out Exception e))
                {
                    Debug.LogError(e);
                }
            }

            foreach (var methodInfo in TypeCache.GetMethodsWithAttribute<ValidatableMethodAttribute>())
            {
                var attr = methodInfo.GetCustomAttribute<ValidatableMethodAttribute>();
                if (!attr.IsValid(methodInfo, out Exception e))
                {
                    Debug.LogError(e);
                }
            }
        }
    }
}
