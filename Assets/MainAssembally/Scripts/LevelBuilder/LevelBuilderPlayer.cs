using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilderPlayer : UIElement
{
    Vector2 cameraRotation;
    Vector3 position;
    public LevelBuilder levelBuilder;

    public override bool OnEvent(object sender, CEvent e)
    {
        if(e is GamepadInputEvent inputEvent)
        {
            position = transform.position;
            Vector3 cameraEuler = transform.rotation.eulerAngles;
            cameraRotation.x = cameraEuler.y;
            cameraRotation.y = -cameraEuler.x;

            var gamepad = inputEvent.gamepad;

            Vector2 cameraInput = gamepad.RightStick;
            cameraRotation += cameraInput * Time.deltaTime * 90f;

            Quaternion rot = Quaternion.Euler(-cameraRotation.y, cameraRotation.x, 0f);

            Vector3 positionInput = Vector3.zero;
            positionInput.x += gamepad.LeftStick.x;
            positionInput.z += gamepad.LeftStick.y;
            if (gamepad.GetButton(Gamepad.Button.LeftStickButton))
            {
                positionInput.y += -1f;
            }
            if (gamepad.GetButton(Gamepad.Button.RightStickButton))
            {
                positionInput.y += 1f;
            }

            position += rot * positionInput * Time.deltaTime * 10f;

            transform.position = position;
            transform.rotation = rot;

            if (gamepad.GetButtonDown(Gamepad.Button.A))
            {
                Grab();
            }
        }

        return false;
    }

    public void Grab()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hitInfo))
        {
            var levelBuilderObject = hitInfo.collider.GetComponentInParent<LevelBuilderObject>();
            if(levelBuilderObject != null)
            {
                levelBuilder.Grab(levelBuilderObject.gameObject);
            }
        }
    }

    public override void OnEventInactive(object sender, CEvent e)
    {

    }
}
