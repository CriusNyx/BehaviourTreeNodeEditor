using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GamepadPoller : MonoBehaviour
{
    private Gamepad player1, player2, player3, player4;

    public enum EventChannel
    {
        input
    }

    public enum EventSubchannel
    {
        player1,
        player2,
        player3,
        player4,
    }

    private void Awake()
    {
        player1 = new Gamepad(XInputDotNetPure.PlayerIndex.One);
        player2 = new Gamepad(XInputDotNetPure.PlayerIndex.Two);
        player3 = new Gamepad(XInputDotNetPure.PlayerIndex.Three);
        player4 = new Gamepad(XInputDotNetPure.PlayerIndex.Four);
    }

    private void Update()
    {
        Poll(player1, EventSubchannel.player1);
        Poll(player2, EventSubchannel.player2);
        Poll(player3, EventSubchannel.player3);
        Poll(player4, EventSubchannel.player4);
    }

    private void Poll(Gamepad gamepad, EventSubchannel channel)
    {
        CustomEventSystem.Broadcast(EventChannel.input, channel, this, new GamepadInputEvent(gamepad.Poll()));
    }

    
}
