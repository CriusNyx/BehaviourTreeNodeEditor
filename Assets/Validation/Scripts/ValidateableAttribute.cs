using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Validation
{
    public abstract class ValidateableAttribute : Attribute
    {
        public static bool ValidateMethodReturn(MethodInfo methodInfo, Type returnType, out string error)
        {
            if (methodInfo.ReturnType != returnType)
            {
                error = $"{methodInfo.ReturnType.ToString()} {methodInfo.DeclaringType.Name}.{methodInfo.Name} does not have the correct return type {returnType.ToString()}";
                return false;
            }
            else
            {
                error = null;
                return true;
            }
        }

        public static bool ValidateMethodHasArg(MethodInfo methodInfo, string argName, out string error)
        {
            if (methodInfo.GetParameters().Any(x => x.Name == argName))
            {
                error = null;
                return true;
            }
            else
            {
                error = $"{methodInfo.ReturnType.Name} {methodInfo.DeclaringType.Name}.{methodInfo.Name} does not have required argument {argName}";
                return false;
            }
        }

        public static bool ValidateMethodHasArg(MethodInfo methodInfo, Type type, string argName, out string error)
        {
            if (methodInfo.GetParameters().Any(x => x.Name == argName && x.ParameterType == type))
            {
                error = null;
                return true;
            }
            else
            {
                error = $"{methodInfo.ReturnType.Name} {methodInfo.DeclaringType.Name}.{methodInfo.Name} does not have required argument {type.Name} {argName}";
                return false;
            }
        }

        public static bool ValidateMethodHasSignature(MethodInfo methodInfo, Type[] signature, out string error)
        {
            if (methodInfo.GetParameters().Select(x => x.ParameterType).SequenceEqual(signature))
            {
                error = null;
                return true;
            }
            else
            {
                error = $"{methodInfo.ReturnType.Name} {methodInfo.DeclaringType.Name}.{methodInfo.Name} does not have the correct signature ({string.Join(", ", signature.Select(x => x.Name))})";
                return false;
            }
        }

        public static bool TypeInherritsType(Type childType, Type parentType, out string error)
        {
            if (parentType.IsAssignableFrom(childType))
            {
                error = "";
                return true;
            }
            else
            {
                error = $"{childType.Name} does not inherrit {parentType.Name}";
                return false;
            }
        }

        public static bool ConditionallyReturnError(string errorMessage, List<string> errors, out Exception e, Func<string, Exception> GenerateException = null)
        {
            if (GenerateException == null)
            {
                GenerateException = (x) => new Exception(x);
            }

            if (errors.Count == 0)
            {
                e = null;
                return true;
            }
            else
            {
                e = GenerateException($"{errorMessage}:\n\t{string.Join("\n\t", errors)}");
                return false;
            }
        }
    }
}