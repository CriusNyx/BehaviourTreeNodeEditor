public struct GamepadInputEvent : CEvent
{
    public readonly GamepadPoll gamepad;

    public GamepadInputEvent(GamepadPoll gamepad)
    {
        this.gamepad = gamepad;
    }
}