using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Wraps AI memory and tree call arguments into a single datastructure, allowing tree's to call child trees.
/// </summary>
public class TreeArguments : IReadOnlyDictionary<object, object>
{
    private Dictionary<string, object> arguments;
    public readonly AIMemory aiMemory;

    public TreeArguments(Dictionary<string, object> arguments, AIMemory aiMemory)
    {
        this.arguments = arguments;
        this.aiMemory = aiMemory;
    }

    public object this[object key]
    {
        get
        {
            if (key is Enum e)
            {
                return aiMemory[e];
            }
            else if (key is string s)
            {
                return arguments[s];
            }
            else
            {
                return null;
            }
        }
    }

    public IEnumerable<object> Keys => throw new NotImplementedException();

    public IEnumerable<object> Values => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public bool ContainsKey(object key)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(object key, out object value)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}