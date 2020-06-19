using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicBinding
{
    public interface IMethodBindingLog
    {
        void AppendMethodCall(string methodName);
        void AppendArgument(string name, object value);
    }
}
