using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using DynamicBinding;

namespace GameEngine.AI
{
    [AttributeUsage(validOn: AttributeTargets.Method)]
    public class AIMethodAttribute : BindableMethodAttribute
    {
        public readonly string tooltip;

        public AIMethodAttribute(string tooltip = "") : base("context")
        {
            this.tooltip = tooltip;
        }

        public override bool IsValid(MethodInfo ownerMethod, out Exception e)
        {
            List<string> errors = new List<string>();

            if (!ValidateMethodHasArg(ownerMethod, typeof(AIExecutionContext), "context", out string e1))
            {
                errors.Add(e1);
            }

            if (!ValidateMethodReturn(ownerMethod, typeof(IEnumerable<AIResult>), out string e2))
            {
                errors.Add(e2);
            }

            return ConditionallyReturnError($"{ownerMethod.DeclaringType.Name}.{ownerMethod.Name} Did not validate", errors, out e);
        }
    }
}