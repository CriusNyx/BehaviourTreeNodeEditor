using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using GameEngine.Validation;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlenderComponentAttribute : ValidatableTypeAttribute
{
    public readonly string propertyName;
    public readonly string processingFunction;
    public readonly string propertyValidationFunction;
    public readonly bool destroyMesh;
    public readonly bool destroyMeshRenderer;
    public readonly bool destroyCollider;

    public BlenderComponentAttribute(string propertyName, string processingFunction = null, string propertyValidationFunction = null, bool destroyMesh = false, bool destroyMeshRenderer = false, bool destroyCollider = false)
    {
        this.propertyName = propertyName;
        this.processingFunction = processingFunction;
        this.propertyValidationFunction = propertyValidationFunction;
        this.destroyMesh = destroyMesh;
        this.destroyMeshRenderer = destroyMeshRenderer;
        this.destroyCollider = destroyCollider;
    }

    public override bool IsValid(Type ownerType, out Exception e)
    {
        bool valid = true;
        e = null;

        List<string> errors = new List<string>();


        BlenderComponentAttribute attr = ownerType.GetCustomAttribute<BlenderComponentAttribute>();

        if (!TypeInherritsType(ownerType, typeof(Component), out string e1))
        {
            valid = false;
            errors.Add(e1);
        }
        if (attr.propertyValidationFunction != null)
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
        if (attr.processingFunction != null)
        {
            MethodInfo targetMethod = ownerType.GetMethod(attr.processingFunction, (BindingFlags)(-1));
            if (targetMethod == null)
            {
                valid = false;
                errors.Add($"{ownerType.Name} does not define processing function \"{attr.processingFunction}\"");
            }
            if (!ValidateMethodHasSignature(targetMethod, new Type[] { typeof(string), typeof(object) }, out string error))
            {
                valid = false;
                errors.Add(error);
            }
        }
        return ConditionallyReturnError($"{ownerType.Name} did not validate as a BlenderComponent.", errors, out e);
    }

    public Action ProcessGameObject(Type targetType, GameObject gameObject, string propertyName, object propertyValue)
    {
        if (propertyValidationFunction == null)
        {
            return ProcessGameObjectAfterValidate(targetType, gameObject, propertyName, propertyValue);
        }
        else
        {
            MethodInfo validationMethod = targetType.GetMethod(propertyValidationFunction);
            if ((bool)validationMethod.Invoke(null, new object[] { propertyValue }))
            {
                return ProcessGameObjectAfterValidate(targetType, gameObject, propertyName, propertyValue);
            }
            else
            {
                return null;
            }
        }
    }

    private Action ProcessGameObjectAfterValidate(Type targetType, GameObject gameObject, string propertyName, object propertyValue)
    {
        var component = gameObject.AddComponent(targetType);
        if (destroyMesh)
        {
            var mesh = gameObject.GetComponent<MeshFilter>();
            if (mesh)
            {
                Object.DestroyImmediate(mesh);
            }
        }
        if (destroyMeshRenderer)
        {
            var renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer)
            {
                Object.DestroyImmediate(renderer);
            }
        }
        if (destroyCollider)
        {
            var collider = gameObject.GetComponent<Collider>();
            if (collider)
            {
                Object.DestroyImmediate(collider);
            }
        }

        if (processingFunction != null)
        {
            return () =>
            {
                MethodInfo targetMethod = targetType.GetMethod(processingFunction, (BindingFlags)(-1));
                targetMethod?.Invoke(component, new object[] { propertyName, propertyValue });
            };
        }
        else
        {
            return null;
        }
    }
}
