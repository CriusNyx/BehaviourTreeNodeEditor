using DynamicBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.AI
{
    public class AIExecutionLog : IMethodBindingLog
    {
        private readonly List<AILogEntry> entries = new List<AILogEntry>();
        public IEnumerable<AILogEntry> Entires => entries;
        AILogEntry current = null;

        public void AppenTreeNode(long guid)
        {
            current = new AILogEntry(guid);
            entries.Add(current);
        }

        public void AppendMethodCall(string methodName)
        {
            current.methodName = methodName;
        }

        public void AppendArgument(string name, object value)
        {
            if (current != null)
            {
                current.LogArgument(name, value);
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach (var entry in entries)
            {
                output.AppendLine($"{entry.methodName}");
                foreach (var arg in entry.Arguments)
                {
                    if (arg.argValue is AIExecutionLog)
                    {
                        output.AppendLine($"   {arg.argName}: Log");
                    }
                    else
                    {
                        output.AppendLine($"   {arg.argName}: {arg.argValue?.ToString()}");
                    }
                }
            }

            return output.ToString();
        }
    }
}