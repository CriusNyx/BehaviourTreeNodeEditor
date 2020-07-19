using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BlenderComponent("ShipThruster", processingFunction: nameof(OnImportAsset))]
public class ShipThruster : MonoBehaviour
{
    public string vector;
    public Vector3[] thrusters;

    private void Start()
    {
        HashSet<string> validDirections = new HashSet<string>();
        char currentAxis = '\0';

        foreach (var c in vector.ToLower())
        {
            switch (c)
            {
                case 'x':
                case 'y':
                case 'z':
                    currentAxis = c;
                    break;
                case '+':
                case '-':
                    var option = new string(new char[] { currentAxis, c });
                    if (!validDirections.Contains(option))
                    {
                        validDirections.Add(option);
                    }
                    break;
            }
        }

        List<Vector3> thrusterList = new List<Vector3>();

        foreach (var option in validDirections)
        {
            Vector3 axis = Vector3.zero;
            bool invert = false;
            switch (option[0])
            {
                case 'x':
                    axis = Vector3.right;
                    break;
                case 'y':
                    axis = Vector3.up;
                    break;
                case 'z':
                    axis = Vector3.forward;
                    break;
            }
            switch (option[1])
            {
                case '+':
                    invert = false;
                    break;
                case '-':
                    invert = true;
                    break;
            }

            if (invert)
            {
                axis = -axis;
            }
            thrusterList.Add(axis);
        }

        this.thrusters = thrusterList.ToArray();
    }

    private void OnImportAsset(string propertyName, object propertyValue)
    {
        this.vector = propertyValue.ToString();
    }

    private void OnDrawGizmos()
    {
        if (thrusters != null)
        {
            foreach (var thruster in thrusters)
            {
                Gizmos.DrawRay(transform.position, transform.localToWorldMatrix.MultiplyVector(thruster));
            }
        }
    }
}