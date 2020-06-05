using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMeshVertex
{
    Vector3 position { get; }
    Vector2 uv { get; }
    Vector3? normal { get; }
    Vector4? tangent { get; }
    Color? color { get; }
}