using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventChannelGroup
{
    private (Enum channel, Enum subchannel)[] channels;

    public EventChannelGroup(params (Enum channel, Enum subchannel)[] channels)
    {
        this.channels = channels;
    }

    public void Add(CEventListener listener)
    {
        foreach(var channel in channels)
        {
            CustomEventSystem.AddEventListener(channel.channel, channel.subchannel, listener);
        }
    }

    public void Remove(CEventListener listener)
    {
        foreach (var channel in channels)
        {
            CustomEventSystem.RemoveEventListener(channel.channel, channel.subchannel, listener);
        }
    }
}