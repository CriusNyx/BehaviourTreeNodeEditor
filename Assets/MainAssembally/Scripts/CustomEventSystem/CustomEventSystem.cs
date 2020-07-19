using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEventSystem : MonoBehaviour
{
    private Dictionary<(Enum channel, Enum subchannel), List<CEventListener>> channels = new Dictionary<(Enum channel, Enum subchannel), List<CEventListener>>();

    private static CustomEventSystem instance;

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        instance = new GameObject("Custom Event System").AddComponent<CustomEventSystem>();
    }

    public static void AddEventListener(Enum channel, Enum subchannel, CEventListener eventListener)
    {
        instance.GetList(channel, subchannel).Add(eventListener);
    }

    public static void RemoveEventListener(Enum channel, Enum subchannel, CEventListener eventListener)
    {
        instance.GetList(channel, subchannel).Remove(eventListener);
    }

    public static void Broadcast(Enum channel, Enum subchannel, object sender, CEvent e)
    {
        foreach (var listener in instance.GetList(channel, subchannel))
        {
            if (listener is MonoBehaviour behaviour && !behaviour.isActiveAndEnabled)
            {
                continue;
            }
            else
            {
                listener.OnEvent(sender, e);
            }
        }
    }

    public static void Broadcast(GameObject parent, object sender, CEvent e)
        => Broadcast(parent.transform, sender, e);

    public static void Broadcast(Transform parent, object sender, CEvent e)
    {
        if (!parent.gameObject.activeSelf)
        {
            return;
        }

        bool recurse = true;
        foreach (var listener in parent.GetComponents<CEventListener>())
        {
            if (listener is MonoBehaviour behaviour && !behaviour.isActiveAndEnabled)
            {
                continue;
            }
            else if (listener.OnEvent(sender, e))
            {
                recurse = false;
            }
        }
        if (recurse)
        {
            foreach (Transform child in parent)
            {
                Broadcast(child, sender, e);
            }
        }
    }

    private List<CEventListener> GetList(Enum channel, Enum subchannel)
    {
        List<CEventListener> output;

        if (instance.channels.TryGetValue((channel, subchannel), out output))
        {
            return output;
        }
        else
        {
            output = new List<CEventListener>();
            instance.channels.Add((channel, subchannel), output);
            return output;
        }
    }
}

public interface CEventListener
{
    /// <summary>
    /// Returns true if the receiver consumes the event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    bool OnEvent(object sender, CEvent e);
}

public interface CEvent
{

}