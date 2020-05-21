using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavGraph
{
    private readonly Dictionary<NavQuad, List<NavQuad>> links = new Dictionary<NavQuad, List<NavQuad>>();

    public void AddLink(NavQuad a, NavQuad b)
    {
        GetList(b);
        var aList = GetList(a);
        if (!aList.Contains(b))
        {
            aList.Add(b);
        }
    }

    private List<NavQuad> GetList(NavQuad quad)
    {
        if (!links.ContainsKey(quad))
        {
            links.Add(quad, new List<NavQuad>());
        }
        return links[quad];
    }

    public IEnumerable<(NavQuad node, IEnumerable<NavQuad> adjacent)> GetNodes()
    {
        foreach(var element in links)
        {
            yield return (element.Key, element.Value);
        }
    }
}