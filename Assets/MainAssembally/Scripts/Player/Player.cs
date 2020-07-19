using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Vector3 velocity;
    Quaternion rotation;
    Vector3 angularVelocity;

    Gamepad gamepad;

    private void Start()
    {
        gamepad = new Gamepad(XInputDotNetPure.PlayerIndex.One);
        rotation = Quaternion.identity;
    }

    void Update()
    {
        gamepad.Poll();

        Vector3 velocityInput = Vector3.zero;

        velocityInput.x += gamepad.LeftStick.x;
        velocityInput.z += gamepad.LeftStick.y;
        velocityInput.y += gamepad.GetButton(Gamepad.Button.RightShoulder) ? 1f : 0f;
        velocityInput.y += gamepad.GetButton(Gamepad.Button.LeftShoulder) ? -1f : 0f;
        velocityInput = rotation * velocityInput;

        var deltaV = velocityInput * 10f * Time.deltaTime;

        if (gamepad.GetButton(Gamepad.Button.A))
        {
            deltaV = -velocity.normalized * 10f * Time.deltaTime;
            if (deltaV.magnitude > velocity.magnitude)
            {
                deltaV = -velocity;
            }
        }

        velocity += deltaV;

        Vector3 rotationInput = Vector3.zero;
        rotationInput.y += gamepad.RightStick.x;
        rotationInput.x += -gamepad.RightStick.y;


        var targetRot = rotationInput * 90f;
        angularVelocity = Vector3.MoveTowards(angularVelocity, targetRot, 90f * Time.deltaTime);
        var deltaRotation = Quaternion.Euler(angularVelocity * Time.deltaTime);
        rotation *= deltaRotation;

        transform.position += velocity * Time.deltaTime;
        transform.rotation = rotation;
    }
}