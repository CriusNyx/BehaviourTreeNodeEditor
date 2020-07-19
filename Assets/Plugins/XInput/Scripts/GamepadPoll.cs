using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XInputDotNetPure;
using static Gamepad;

public class GamepadPoll
{
    private readonly GamePadState currentState;
    private readonly GamePadState previousState;
    private bool leftTriggerPressedPreviously;
    private bool rightTriggerPressedPreviously;
    private float leftStickDZ;
    private float leftStickPow;
    private float rightStickDZ;
    private float rightStickPow;

    public Vector2 LeftStickNDZ
    {
        get
        {
            return new Vector2(currentState.ThumbSticks.Left.X, currentState.ThumbSticks.Left.Y);
        }
    }

    public Vector2 RightStickNDZ
    {
        get
        {
            return new Vector2(currentState.ThumbSticks.Right.X, currentState.ThumbSticks.Right.Y);
        }
    }

    public Vector2 LeftStick
    {
        get
        {
            return ApplyDeadZone(LeftStickNDZ, leftStickDZ, leftStickPow);
        }
    }

    public Vector2 RightStick
    {
        get
        {
            return ApplyDeadZone(RightStickNDZ, rightStickDZ, rightStickPow);
        }
    }

    public float LeftTrigger => currentState.Triggers.Left;
    public float RightTrigger => currentState.Triggers.Right;

    private static readonly Dictionary<Button, IGamepadButton> Buttons = new Dictionary<Button, IGamepadButton>()
        {
            { Button.A, new GamepadButton(x => x.Buttons.A == ButtonState.Pressed) },
            { Button.B, new GamepadButton(x => x.Buttons.B == ButtonState.Pressed) },
            { Button.X, new GamepadButton(x => x.Buttons.X == ButtonState.Pressed) },
            { Button.Y, new GamepadButton(x => x.Buttons.Y == ButtonState.Pressed) },

            { Button.Start, new GamepadButton(x => x.Buttons.Start == ButtonState.Pressed) },
            { Button.Back, new GamepadButton(x => x.Buttons.Back == ButtonState.Pressed) },

            { Button.LeftStickButton, new GamepadButton(x => x.Buttons.LeftStick == ButtonState.Pressed) },
            { Button.RightStickButton, new GamepadButton(x => x.Buttons.RightStick == ButtonState.Pressed) },

            { Button.LeftShoulder, new GamepadButton(x => x.Buttons.LeftShoulder == ButtonState.Pressed) },
            { Button.RightShoulder, new GamepadButton(x => x.Buttons.RightShoulder == ButtonState.Pressed) },

            { Button.LeftTriggerButton, new FloatButton(x => x.Triggers.Left) },
            { Button.RightTriggerButton, new FloatButton(x => x.Triggers.Right) },

            { Button.DPadLeft, new GamepadButton(x => x.DPad.Left == ButtonState.Pressed) },
            { Button.DPadRight, new GamepadButton(x => x.DPad.Right == ButtonState.Pressed) },
            { Button.DPadDown, new GamepadButton(x => x.DPad.Down == ButtonState.Pressed) },
            { Button.DPadUp, new GamepadButton(x => x.DPad.Up == ButtonState.Pressed) }
        };

    public GamepadPoll(GamePadState currentState, GamepadPoll poll, float leftStickDZ, float leftStickPow, float rightStickDZ, float rightStickPow)
    {
        this.currentState = currentState;
        this.previousState = poll?.currentState ?? new GamePadState();
        this.leftTriggerPressedPreviously = poll?.GetButton(Button.LeftTriggerButton) ?? false;
        this.rightTriggerPressedPreviously = poll?.GetButton(Button.RightTriggerButton) ?? false;

        this.leftStickDZ = leftStickDZ;
        this.leftStickPow = leftStickPow;
        this.rightStickDZ = rightStickDZ;
        this.rightStickPow = rightStickPow;
    }

    public bool GetAnyButton(params Button[] button)
    {
        return button.Any(x => GetButton(x));
    }

    public bool GetAnyButtonDown(params Button[] button)
    {
        return button.Any(x => GetButtonDown(x));
    }

    public bool GetButton(Button button, PressMode pressMode)
    {
        switch (pressMode)
        {
            case PressMode.Hold:
                return GetButton(button);
            case PressMode.Down:
                return GetButtonDown(button);
            case PressMode.Up:
                return GetButtonUp(button);
            default:
                throw new ArgumentException($"Unkown Press Mode {pressMode}");
        }
    }

    public bool GetButton(Button button)
    {
        if(button == Button.None)
        {
            return false;
        }

        var buttonObject = Buttons[button];
        if (buttonObject is FloatButton floatButton)
        {
            switch (button)
            {
                case Button.LeftTriggerButton:
                    return buttonObject.GetButton(currentState, leftTriggerPressedPreviously);
                case Button.RightTriggerButton:
                    return buttonObject.GetButton(currentState, rightTriggerPressedPreviously);
                default:
                    throw new ArgumentException($"{button} is not a known button type.");
            }
        }
        else
        {
            return buttonObject.GetButton(currentState, false);
        }
    }

    public bool GetButtonDown(Button button)
    {
        if (button == Button.None)
        {
            return false;
        }

        var buttonObject = Buttons[button];
        if (buttonObject is FloatButton floatButton)
        {
            switch (button)
            {
                case Button.LeftTriggerButton:
                    return buttonObject.GetButtonDown(currentState, previousState, leftTriggerPressedPreviously);
                case Button.RightTriggerButton:
                    return buttonObject.GetButtonDown(currentState, previousState, rightTriggerPressedPreviously);
                default:
                    throw new System.ArgumentException($"{button} is not a known button type.");
            }
        }
        else
        {
            return buttonObject.GetButtonDown(currentState, previousState, false);
        }
    }

    public bool GetButtonUp(Button button)
    {
        if (button == Button.None)
        {
            return false;
        }

        var buttonObject = Buttons[button];
        if (buttonObject is FloatButton floatButton)
        {
            switch (button)
            {
                case Button.LeftTriggerButton:
                    return buttonObject.GetButtonUp(currentState, previousState, leftTriggerPressedPreviously);
                case Button.RightTriggerButton:
                    return buttonObject.GetButtonUp(currentState, previousState, rightTriggerPressedPreviously);
                default:
                    throw new System.ArgumentException($"{button} is not a known button type.");
            }
        }
        else
        {
            return buttonObject.GetButtonUp(currentState, previousState, false);
        }
    }

    public static Vector2 ApplyDeadZone(Vector2 input, float deadZone, float power)
    {
        float mag = input.magnitude;
        mag = Mathf.Clamp01(mag);
        if (mag < deadZone)
        {
            mag = deadZone;
        }
        mag = (mag - deadZone) / (1f - deadZone);
        mag = Mathf.Pow(mag, power);
        return input.normalized * mag;
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append($"LeftStick: {LeftStickNDZ.ToString()} RightStick: {RightStickNDZ.ToString()} LeftTrigger: {LeftTrigger} RightTrigger: {RightTrigger} ");

        foreach(var button in Enum.GetValues(typeof(Button)))
        {
            if (GetButton((Button)button))
            {
                stringBuilder.Append(button + " ");
            }
        }

        return stringBuilder.ToString();
    }
}