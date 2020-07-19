using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalSpaceShipInput
{
    public readonly Vector3
        inputDirection,
        targetAngularMomentum;

    public readonly bool airbrakes;

    public LocalSpaceShipInput(Vector3 inputDirection, Vector3 targetAngularMomentum, bool airbrakes)
    {
        this.inputDirection = inputDirection;
        this.targetAngularMomentum = targetAngularMomentum;
        this.airbrakes = airbrakes;
    }

    public WorldSpaceShipInput GetInWorldSpace(Transform transform)
    {
        return new WorldSpaceShipInput(
            transform.localToWorldMatrix.MultiplyVector(inputDirection),
            transform.localToWorldMatrix.MultiplyVector(targetAngularMomentum),
            airbrakes);
    }
}