using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Validation
{
    public abstract class ValidatableTypeAttribute : ValidateableAttribute
    {
        public abstract bool IsValid(Type ownerType, out Exception e);
    }
}