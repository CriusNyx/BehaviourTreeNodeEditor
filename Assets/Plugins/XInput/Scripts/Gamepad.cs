using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XInputDotNetPure;

public class Gamepad
{
    PlayerIndex player;

    public enum Button
    {
        A, B, X, Y,
        Start, Back,
        LeftStickButton, RightStickButton,
        LeftShoulder, RightShoulder,
        LeftTriggerButton, RightTriggerButton,
        DPadLeft, DPadRight, DPadDown, DPadUp,
        None
    }

    public enum PressMode
    {
        Down,
        Up,
        Hold
    }

    public float leftStickDZ = 0.05f;
    public float leftStickPow = 2f;
    public float rightStickDZ = 0.05f;
    public float rightStickPow = 2f;

    public GamepadPoll lastPoll { get; private set; }

    public Vector2 LeftStickNDZ => lastPoll.LeftStickNDZ;
    public Vector2 RightStickNDZ => lastPoll.RightStickNDZ;

    public Vector2 LeftStick => lastPoll.LeftStick;

    public Vector2 RightStick => lastPoll.RightStick;

    public float LeftTrigger => lastPoll.LeftTrigger;
    public float RightTrigger => lastPoll.RightTrigger;

    public Gamepad(PlayerIndex player)
    {
        this.player = player;
    }

    public GamepadPoll Poll()
    {
        lastPoll = new GamepadPoll(GamePad.GetState(player), lastPoll, leftStickDZ, leftStickPow, rightStickDZ, rightStickPow);
        return lastPoll;
    }

    public bool GetButton(Button button, PressMode mode)
    {
        switch (mode)
        {
            case PressMode.Down:
                return GetButtonDown(button);
            case PressMode.Hold:
                return GetButton(button);
            case PressMode.Up:
                return GetButtonUp(button);
            default:
                throw new ArgumentException("Unknown Press Mode");
        }
    }

    public bool GetButton(Button button)
    {
        return lastPoll.GetButton(button);
    }

    public bool GetAnyPressed(params Button[] buttons)
    {
        return buttons.Any(x => GetButton(x));
    }

    public bool GetButtonDown(Button button)
    {
        return lastPoll.GetButtonDown(button);
    }

    public bool GetButtonUp(Button button)
    {
        return lastPoll.GetButtonUp(button);
    }

    public interface IGamepadButton
    {
        bool GetButton(GamePadState current, bool pressedPreviously);
        bool GetButtonDown(GamePadState current, GamePadState previous, bool pressedPreviously);
        bool GetButtonUp(GamePadState current, GamePadState previous, bool pressedPresiously);
    }

    public class GamepadButton : IGamepadButton
    {
        private readonly Func<GamePadState, bool> GetState;

        public GamepadButton(Func<GamePadState, bool> getState)
        {
            GetState = getState;
        }

        public bool GetButton(GamePadState current, bool pressedPreviously)
        {
            return GetState(current);
        }

        public bool GetButtonDown(GamePadState current, GamePadState previous, bool pressedPreviously)
        {
            return GetState(current) && !GetState(previous);
        }

        public bool GetButtonUp(GamePadState current, GamePadState previous, bool pressedPreviously)
        {
            return !GetState(current) && GetState(previous);
        }
    }

    public class FloatButton : IGamepadButton
    {
        // Remember that 0 is unpressed, and 1 is fully pressed
        float downActuationPoint = 0.002f;
        float upActuationPoint = 0.001f;

        private readonly Func<GamePadState, float> GetValue;

        public FloatButton(Func<GamePadState, float> getValue)
        {
            GetValue = getValue;
        }

        public bool GetButton(GamePadState current, bool pressedPreviously)
        {
            if (pressedPreviously)
            {
                return GetValue(current) >= upActuationPoint;
            }
            else
            {
                return GetValue(current) >= downActuationPoint;
            }
        }

        public bool GetButtonDown(GamePadState current, GamePadState previous, bool pressedPreviously)
        {
            return GetButton(current, pressedPreviously) && !pressedPreviously;
        }

        public bool GetButtonUp(GamePadState current, GamePadState previous, bool pressedPreviously)
        {
            return !GetButton(current, pressedPreviously) && pressedPreviously;
        }
    }
}
