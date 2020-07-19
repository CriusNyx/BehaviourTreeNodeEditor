using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicBinding
{
    /// <summary>
    /// Types for method binding arguments that can be swapped out for different types.
    /// </summary>
    public enum ChangeableMethodBindingArgumentType
    {
        Static,
        Memory,
        Argument,
    }

    /// <summary>
    /// Types for method binding arguments that cannot be swaped out for different types.
    /// </summary>
    public enum NonChangeableMethodBindingType
    {
        Params
    }
}