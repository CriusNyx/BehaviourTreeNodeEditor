using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorSpawnButtonGroup : MonoBehaviour, CEventListener
{
    public Gamepad.Button button = Gamepad.Button.None;
    public bool IsModKeyActive { get; private set; }

    public LevelBuilderAsset xObject, yObject, bObject;

    public bool OnEvent(object sender, CEvent e)
    {
        if(e is GamepadInputEvent inputEvent)
        {
            UpdateGamepad(inputEvent.gamepad);
        }
        return false;
    }

    public void UpdateGamepad(GamepadPoll gamepad)
    {
        SetShading("Mod", IsModKeyActive);

        if (IsModKeyActive)
        {
            SetShading("X", gamepad.GetButton(Gamepad.Button.X));
            SetShading("Y", gamepad.GetButton(Gamepad.Button.Y));
            SetShading("B", gamepad.GetButton(Gamepad.Button.B));

            SetColor("X", Color.white);
            SetColor("Y", Color.white);
            SetColor("B", Color.white);
        }
        else
        {
            SetShading("X", false);
            SetShading("Y", false);
            SetShading("B", false);

            SetColor("X", Color.gray);
            SetColor("Y", Color.gray);
            SetColor("B", Color.gray);
        }
    }

    public bool ShouldSpawn(GamepadPoll gamepad, out LevelBuilderAsset objectToSpawn)
    {
        objectToSpawn = null;
        if (IsMyModkeyPressed(gamepad))
        {
            if (gamepad.GetButtonDown(Gamepad.Button.X))
            {
                objectToSpawn = xObject;
            }
            if (gamepad.GetButtonDown(Gamepad.Button.Y))
            {
                objectToSpawn = yObject;
            }
            if (gamepad.GetButtonDown(Gamepad.Button.B))
            {
                objectToSpawn = bObject;
            }
        }
        return objectToSpawn != null;
    }

    public bool IsMyModkeyPressed(GamepadPoll gamepad)
    {
        if (button == Gamepad.Button.None)
        {
            return !gamepad.GetAnyButton(
                Gamepad.Button.LeftShoulder, 
                Gamepad.Button.RightShoulder, 
                Gamepad.Button.LeftTriggerButton, 
                Gamepad.Button.RightTriggerButton);
        }
        else
        {
            return gamepad.GetButton(button);
        }
    }

    public void SetModKeyActive(bool active)
    {
        IsModKeyActive = active;
    }

    public void SetResourceSelection(LevelBuilderAsset asset, GamepadPoll gamepad)
    {
        if (IsMyModkeyPressed(gamepad))
        {
            if (gamepad.GetButtonDown(Gamepad.Button.X))
            {
                xObject = asset;
                SetThumbnail("X_Thumbnail", asset.icon);
            }
            else if (gamepad.GetButtonDown(Gamepad.Button.Y))
            {
                yObject = asset;
                SetThumbnail("Y_Thumbnail", asset.icon);
            }
            else if (gamepad.GetButtonDown(Gamepad.Button.B))
            {
                bObject = asset;
                SetThumbnail("B_Thumbnail", asset.icon);
            }
        }
    }

    private void SetShading(string targetName, bool shaded)
    {
        transform.Find(targetName).gameObject.SetActive(!shaded);
        transform.Find(targetName + "_Pressed").gameObject.SetActive(shaded);
    }

    private void SetColor(string targetName, Color color)
    {
        transform.Find(targetName).GetComponent<Image>().color = color;
        transform.Find(targetName + "_Pressed").GetComponent<Image>().color = color;
    }

    private void SetThumbnail(string childName, Texture texture)
    {
        var child = transform.Find(childName);
        child.gameObject.SetActive(true);
        child.GetComponent<RawImage>().texture = texture;
    }
}