using UnityEngine;

public class WorldSpaceShipInput
{
    public readonly Vector3 posInput;
    public readonly Vector3 targetRotMomentum;

    public readonly bool airbrakes;

    public WorldSpaceShipInput(Vector3 posInput, Vector3 targetRotMomentum, bool airbrakes)
    {
        this.posInput = posInput;
        this.targetRotMomentum = targetRotMomentum;
        this.airbrakes = airbrakes;
    }

    public LocalSpaceShipInput GetLocalSpaceInput(Transform shipTransform)
    {
        return new LocalSpaceShipInput(
            shipTransform.worldToLocalMatrix.MultiplyVector(posInput),
            shipTransform.worldToLocalMatrix.MultiplyVector(targetRotMomentum),
            airbrakes);
    }
}