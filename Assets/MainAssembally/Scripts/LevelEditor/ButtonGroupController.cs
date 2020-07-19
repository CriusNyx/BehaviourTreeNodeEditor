using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonGroupController : UIElement
{
    public event Action<GameObject> OnSpawn;

    public override void OnEventInactive(object sender, CEvent e)
    {
        if (e is GamepadInputEvent gamepadInput)
        {
            var gamepad = gamepadInput.gamepad;

            var children = gameObject.GetComponentsInChildren<LevelEditorSpawnButtonGroup>();
            foreach (LevelEditorSpawnButtonGroup buttonGroup in children)
            {
                buttonGroup.SetModKeyActive(false);
            }

            foreach (LevelEditorSpawnButtonGroup buttonGroup in children)
            {
                if (buttonGroup.IsMyModkeyPressed(gamepad))
                {
                    buttonGroup.SetModKeyActive(true);
                    break;
                }
            }

            foreach(var buttonGroup in children)
            {
                buttonGroup.OnEvent(sender, e);
            }
        }
    }

    public override bool OnEvent(object sender, CEvent e)
    {
        if (e is GamepadInputEvent gamepadInput)
        {
            var children = gameObject.GetComponentsInChildren<LevelEditorSpawnButtonGroup>();
            foreach (LevelEditorSpawnButtonGroup buttonGroup in children)
            {
                buttonGroup.OnEvent(sender, gamepadInput);
            }
        }
        return false;
    }

    public bool ShouldSpawn(GamepadPoll gamepad, out LevelBuilderAsset targetToSpawn)
    {
        targetToSpawn = null;
        foreach(var child in gameObject.GetComponentsInChildren<LevelEditorSpawnButtonGroup>())
        {
            if(child.ShouldSpawn(gamepad, out targetToSpawn))
            {
                return true;
            }
        }
        return false;
    }

    public void SetSelection(LevelBuilderAsset asset, GamepadPoll gamepad)
    {
        foreach(var child in gameObject.GetComponentsInChildren<LevelEditorSpawnButtonGroup>())
        {
            if (child.IsMyModkeyPressed(gamepad))
            {
                child.SetResourceSelection(asset, gamepad);
            }
        }
    }
}