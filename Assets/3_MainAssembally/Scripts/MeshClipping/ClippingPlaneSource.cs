﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BlenderComponent("ClippingPlaneSource", destroyMeshRenderer: true)]
public class ClippingPlaneSource : MonoBehaviour
{
    public object ToDictionary { get; internal set; }
}