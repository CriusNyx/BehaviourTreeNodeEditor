using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlenderComponentAttribute : ValidatableTypeAttribute
{
    public readonly string propertyName;
    public readonly string propertyValidationFunction;
    public readonly bool destroyMesh;
    public readonly bool destroyMeshRenderer;

    public BlenderComponentAttribute(string propertyName, string propertyEvaluationFunction = null, bool destroyMesh = false, bool destroyMeshRenderer = false)
    {
        this.propertyName = propertyName;
        this.propertyValidationFunction = propertyEvaluationFunction;
        this.destroyMesh = destroyMesh;
        this.destroyMeshRenderer = destroyMeshRenderer;
    }

    public override bool IsValid(Type ownerType, out Exception e)
    {
        bool valid = true;
        e = null;

        List<string> errors = new List<string>();


        BlenderComponentAttribute attr = ownerType.GetCustomAttribute<BlenderComponentAttribute>();

        if(!TypeInherritsType(ownerType, typeof(Component), out string e1))
        {
            valid = false;
            errors.Add(e1);
        }
        if(attr.propertyValidationFunction != null)
        {
            MethodInfo targetMethod = ownerType.GetMethod(attr.propertyValidationFunction);
            if (targetMethod == null)
            {
                valid = false;
                errors.Add($"{ownerType.Name} does not define property validation function \"{attr.propertyValidationFunction}\"");
            }
            else
            {
                if (!ValidateMethodReturn(targetMethod, typeof(bool), out string e2))
                {
                    valid = false;
                    errors.Add(e2);
                }
                if (!ValidateMethodHasSignature(targetMethod, new Type[] { typeof(object) }, out string e3))
                {
                    valid = false;
                    errors.Add(e3);
                }
            }
        }
        if (!valid)
        {
            ConditionallyReturnError($"{ownerType.Name} did not validate as a BlenderComponent.", errors, out e);
        }
        return valid;
    }

    public void ProcessGameObject(Type targetType, GameObject gameObject, string propertyName, object propertyValue)
    {
        if(propertyValidationFunction == null)
        {
            ProcessGameObjectAfterValidate(targetType, gameObject, propertyName, propertyValue);
        }
        else
        {
            MethodInfo validationMethod = targetType.GetMethod(propertyValidationFunction);
            if ((bool)validationMethod.Invoke(null, new object[] { propertyValue }))
            {
                ProcessGameObjectAfterValidate(targetType, gameObject, propertyName, propertyValue);
            }
        }
    }

    private void ProcessGameObjectAfterValidate(Type targetType, GameObject gameObject, string propertyName, object propertyValue)
    {
        gameObject.AddComponent(targetType);
        if (destroyMesh)
        {
            UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<MeshFilter>());
        }
        if (destroyMeshRenderer)
        {
            UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<MeshRenderer>());
        }
    }
}