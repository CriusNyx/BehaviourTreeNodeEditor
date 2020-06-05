using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UniqueList<T>
{
    private List<T> elements = new List<T>();
    private Func<T, T, bool> comparator;

    public UniqueList(Func<T, T, bool> comparator = null)
    {
        if(comparator == null)
        {
            comparator = (x, y) => x.Equals(y);
        }
        this.comparator = comparator;
    }

    public IEnumerable<T> Elements => elements;

    public bool TryAdd(T element)
    {
        if(elements.Any(x => comparator(element, x)))
        {
            return false;
        }
        else
        {
            elements.Add(element);
            return true;
        }
    }
}