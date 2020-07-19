using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ship : MonoBehaviour
{
    new Rigidbody rigidbody;

    Gamepad gamepad;

    public float mass => rigidbody.mass;

    public Vector3 angularMomentum;
    public float maxAngularMomentum = 1 * Mathf.PI;
    public float momentOfInertia = 1f;
    public float maxTorque = Mathf.PI * 50f;

    public Vector3 currentVelocity;

    private void Start()
    {
        gamepad = new Gamepad(XInputDotNetPure.PlayerIndex.One);
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        currentVelocity = rigidbody.velocity;
        angularMomentum = rigidbody.angularVelocity * momentOfInertia;

        LocalSpaceShipInput localInput = GetInput();
        UpdateInput(localInput);

        rigidbody.velocity = currentVelocity;
        rigidbody.angularVelocity = angularMomentum / momentOfInertia;
    }

    private LocalSpaceShipInput GetInput()
    {
        GamepadPoll poll = gamepad.Poll();

        Vector3 positionInput = Vector3.zero;
        Vector3 rotationInput = Vector3.zero;

        positionInput += poll.LeftStick.ToVector3("x0y");
        positionInput.y += poll.RightTrigger;
        positionInput.y -= poll.LeftTrigger;

        rotationInput += poll.RightStick.ToVector3("-yx0");
        if (poll.GetButton(Gamepad.Button.RightShoulder))
        {
            rotationInput.z -= 1f;
        }
        if (poll.GetButton(Gamepad.Button.LeftShoulder))
        {
            rotationInput.z += 1f;
        }

        if (positionInput.sqrMagnitude > 1f)
        {
            positionInput = positionInput.normalized;
        }
        if (rotationInput.sqrMagnitude > 1f)
        {
            rotationInput = rotationInput.normalized;
        }

        bool airbrakes = poll.GetButton(Gamepad.Button.A);

        LocalSpaceShipInput localInput = new LocalSpaceShipInput(positionInput, rotationInput, airbrakes);
        return localInput;
    }

    private void UpdateInput(LocalSpaceShipInput input)
    {
        Vector3 localPosInput = GetPosInputVector(input);

        Vector3 engineInput = GetEngineInput(localPosInput);

        FireEngines(engineInput);

        Vector3 targetMomentum = input.targetAngularMomentum * maxAngularMomentum;

        angularMomentum = UpdateAngularMomentum(transform.localToWorldMatrix.MultiplyVector(targetMomentum));
    }

    private Vector3 GetPosInputVector(LocalSpaceShipInput input)
    {
        if (input.airbrakes)
        {
            return transform.worldToLocalMatrix.MultiplyVector(-currentVelocity).normalized;
        }
        else
        {
            return input.inputDirection;
        }
    }

    private Vector3 GetEngineInput(Vector3 localSpaceInput)
    {
        Vector3 engineInput = Vector3.zero;

        for (int i = 0; i < 3; i++)
        {
            ThrusterAxis axis = (ThrusterAxis)(0b1 << i);
            if (localSpaceInput[i] >= 0)
            {
                axis = axis | ThrusterAxis.plus;
            }
            else
            {
                axis = axis | ThrusterAxis.minus;
            }

            Vector3 axisVector = GetGlobalSpaceVectorFromAxis(axis);

            // calculate max thrust
            float maxThrust = 0f;

            foreach (var thruster in gameObject.GetComponentsInChildren<SingleThruster>())
            {
                if (Vector3.Dot(thruster.globalThrustDirection, axisVector) > 0.95f)
                {
                    maxThrust += thruster.maxThrust;
                }
            }

            engineInput[i] = maxThrust * localSpaceInput[i];
        }

        return engineInput;
    }

    private Vector3 UpdateAngularMomentum(Vector3 targetMomentumWS)
    {
        return Vector3.MoveTowards(angularMomentum, targetMomentumWS, maxTorque * Time.deltaTime);
    }

    public void FireEngines(Vector3 engineInput)
    {
        float compSum = Math.Abs(engineInput.x) + Math.Abs(engineInput.y) + Math.Abs(engineInput.z);
        float maxFuel = gameObject.GetComponentsInChildren<FuelPump>().Sum(x => x.maxForce);
        if (compSum > maxFuel)
        {
            engineInput = engineInput * (maxFuel / compSum);
        }

        Vector3 forceVector = transform.localToWorldMatrix.MultiplyVector(engineInput);
        currentVelocity += forceVector * Time.deltaTime / mass;
    }

    public Vector3 GetGlobalSpaceVectorFromAxis(ThrusterAxis axis)
    {
        return transform.localToWorldMatrix.MultiplyVector(GetLocalSpaceVectorFromAxis(axis));
    }

    public Vector3 GetLocalSpaceVectorFromAxis(ThrusterAxis axis)
    {
        float invert = axis.HasFlag(ThrusterAxis.minus) ? 1f : -1f;

        if (axis.HasFlag(ThrusterAxis.x))
        {
            return invert * Vector3.right;
        }
        else if (axis.HasFlag(ThrusterAxis.y))
        {
            return invert * Vector3.up;
        }
        else if (axis.HasFlag(ThrusterAxis.z))
        {
            return invert * Vector3.forward;
        }
        else
        {
            throw new Exception($"Unknown axis type {axis}");
        }
    }
}
