using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    public void Toggle(string target)
    {
        if (target == null || target == "")
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
        else
        {
            MonoBehaviour behaviour = gameObject.GetComponent(target) as MonoBehaviour;
            behaviour.enabled = !behaviour.enabled;
        }
    }
}