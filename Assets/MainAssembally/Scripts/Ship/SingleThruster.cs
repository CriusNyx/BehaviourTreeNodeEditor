using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleThruster : MonoBehaviour
{
    public Vector3 localThrustDirection;
    public Vector3 globalThrustDirection 
    { 
        get
        {
            return transform.localToWorldMatrix.MultiplyVector(localThrustDirection).normalized;
        } 
    }


    public float efficency = 1f;
    public float maxThrust = 100f;

    public float maxFuelUse
    {
        get
        {
            return maxThrust / efficency;
        }
    }
    
    public float CalcDirAlignment(Vector3 globalTargetDirection)
    {
        float eff = Vector3.Dot(-globalTargetDirection.normalized, globalThrustDirection.normalized);
        if(eff >= 0f)
        {
            return eff;
        }
        else
        {
            return 0f;
        }
    }

    public float CalcDirAlignEfficency(Vector3 globalTargetDirection)
    {
        return CalcDirAlignEfficency(globalTargetDirection) * efficency;
    }

    public Vector3 CalcTorqueDirection(Vector3 globalCenterOfMass)
    {
        return Vector3.Cross(globalThrustDirection.normalized, transform.position - globalCenterOfMass);
    }

    public float CalcTorqueAlignment(Vector3 globalCenterOfMass, Vector3 targetTorqueDirection)
    {
        Vector3 globalTorqueDirection = CalcTorqueDirection(globalCenterOfMass);
        float output = Vector3.Dot(globalTorqueDirection, targetTorqueDirection);
        if(output >= 0f)
        {
            return output;
        }
        else
        {
            return 0f;
        }
    }

    public (Vector3 thrust, float remainingFuel) Fire(Vector3 globalTargetDirection, float fuel)
    {
        if(fuel > maxFuelUse)
        {
            float engineAlignment = CalcDirAlignment(globalTargetDirection);
            return (-globalTargetDirection * engineAlignment * maxThrust, fuel - maxFuelUse);
        }
        else
        {
            float engineAlignment = CalcDirAlignment(globalTargetDirection);
            return (-globalTargetDirection * engineAlignment * maxThrust * (fuel / maxFuelUse), 0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        var oldColor = Gizmos.color;

        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, globalThrustDirection);

        Gizmos.color = Color.red;
        var rigidbody = gameObject.GetComponentInParent<Rigidbody>();
        if(rigidbody != null)
        {
            Vector3 globalCenterOfMass = rigidbody.transform.localToWorldMatrix.MultiplyPoint(rigidbody.centerOfMass);
            Vector3 torqueDirection = CalcTorqueDirection(globalCenterOfMass);
            Gizmos.DrawRay(globalCenterOfMass, torqueDirection);
        }

        Gizmos.color = oldColor;
    }
}
