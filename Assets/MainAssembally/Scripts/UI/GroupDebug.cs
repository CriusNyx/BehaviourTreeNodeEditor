using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupDebug : UIElement
{
    UIStack uiStack = new UIStack();

    public GameObject prefab;

    Gamepad gamepad;

    private void Start()
    {
        uiStack.Push(this);
        this.gamepad = new Gamepad(XInputDotNetPure.PlayerIndex.One);
    }

    private void Update()
    {
        uiStack.SendEvent(this, new GamepadInputEvent(gamepad.Poll()));
    }

    public override bool OnEvent(object sender, CEvent e)
    {
        if(e is GamepadInputEvent inputEvent)
        {
            var gamepad = inputEvent.gamepad;
            if (gamepad.GetButtonDown(Gamepad.Button.A))
            {
                //var task = uiStack.Push<string>(prefab);
                //task.ThenOnMainThread((x) => Print(x));
                Debug.Log("A: Pushing");
            }
            return true;
        }
        return false;
    }

    public override void OnEventInactive(object sender, CEvent e)
    {

    }
}
