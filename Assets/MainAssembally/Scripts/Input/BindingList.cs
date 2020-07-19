using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindingList : MonoBehaviour, CEventListener
{
    private static BindingList instance;
    private static EventChannelGroup channel = new EventChannelGroup((GamepadPoller.EventChannel.input, GamepadPoller.EventSubchannel.player1));
    private Dictionary<(Gamepad.Button button, Gamepad.PressMode pressmode), Dictionary<string, Action>> buttonMap
        = new Dictionary<(Gamepad.Button button, Gamepad.PressMode pressmode), Dictionary<string, Action>>();

    [RuntimeInitializeOnLoadMethod]
    private static void CreateBindingList()
    {
        instance = new GameObject("BindingList").AddComponent<BindingList>();
    }

    public bool OnEvent(object sender, CEvent e)
    {
        if (e is GamepadInputEvent gamepadInput)
        {
            var gamepad = gamepadInput.gamepad;
            foreach (var element in buttonMap)
            {
                var key = element.Key;
                var value = element.Value;
                var (button, pressMode) = key;

                if (gamepad.GetButton(button, pressMode))
                {
                    foreach (var action in value.Values)
                    {
                        action();
                    }
                }
            }
        }
        return false;
    }

    private void Start()
    {
        channel.Add(this);
    }

    private void OnDestroy()
    {
        channel.Remove(this);
    }

    public static void AddBinding(Gamepad.Button button, Gamepad.PressMode pressMode, string name, Action action)
    {
        instance?._AddBinding(button, pressMode, name, action);
    }

    private void _AddBinding(Gamepad.Button button, Gamepad.PressMode pressMode, string name, Action action)
    {
        var buttonDictionary = GetButtonDictionary(button, pressMode);
        if (buttonDictionary.ContainsKey(name))
        {
            throw new ArgumentException($"{name} is already bound to an action");
        }
        buttonDictionary.Add(name, action);
    }

    public static void RemoveBinding(Gamepad.Button button, Gamepad.PressMode pressMode, string bindingName)
    {
        instance?._RemoveBinding(button, pressMode, bindingName);
    }

    private void _RemoveBinding(Gamepad.Button button, Gamepad.PressMode pressMode, string bindingName)
    {
        var dictionary = GetButtonDictionary(button, pressMode);
        if (dictionary.ContainsKey(bindingName))
        {
            dictionary.Remove(bindingName);
        }
    }

    private Dictionary<string, Action> GetButtonDictionary(Gamepad.Button button, Gamepad.PressMode pressMode)
    {
        Dictionary<string, Action> output;
        if (buttonMap.TryGetValue((button, pressMode), out output))
        {
            return output;
        }
        else
        {
            output = new Dictionary<string, Action>();
            buttonMap.Add((button, pressMode), output);
            return output;
        }
    }
}
