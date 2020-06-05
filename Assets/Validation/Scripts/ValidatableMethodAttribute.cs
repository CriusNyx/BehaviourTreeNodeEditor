using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Validation
{
    public abstract class ValidatableMethodAttribute : ValidateableAttribute
    {
        public abstract bool IsValid(MethodInfo ownerMethod, out Exception e);
    }
}