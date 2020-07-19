using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Binding : MonoBehaviour
{
    public Gamepad.Button button;
    public Gamepad.PressMode pressMode;
    public string bindingName;
    public UnityEvent action;

    private void Start()
    {
        BindingList.AddBinding(button, pressMode, bindingName, () => action?.Invoke());
    }

    private void OnDestroy()
    {
        BindingList.RemoveBinding(button, pressMode, bindingName);
    }
}
