using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AutoInitDictionary<T, U> : IReadOnlyDictionary<T, U>
{
    Dictionary<T, U> dictionary = new Dictionary<T, U>();
    Func<U> CreateEntry;

    public AutoInitDictionary(Func<U> createEntry)
    {
        CreateEntry = createEntry;
    }

    public U this[T key]
    {
        get
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, CreateEntry());
            }
            return dictionary[key];
        }
    }

    public IEnumerable<T> Keys => ((IReadOnlyDictionary<T, U>)dictionary).Keys;

    public IEnumerable<U> Values => ((IReadOnlyDictionary<T, U>)dictionary).Values;

    public int Count => ((IReadOnlyDictionary<T, U>)dictionary).Count;

    public bool ContainsKey(T key)
    {
        return ((IReadOnlyDictionary<T, U>)dictionary).ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<T, U>> GetEnumerator()
    {
        return ((IReadOnlyDictionary<T, U>)dictionary).GetEnumerator();
    }

    public bool TryGetValue(T key, out U value)
    {
        return ((IReadOnlyDictionary<T, U>)dictionary).TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IReadOnlyDictionary<T, U>)dictionary).GetEnumerator();
    }
}
