using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderBounds : MonoBehaviour
{
    private void Update()
    {
        var collider = gameObject.GetComponent<Collider>();
        var bounds = collider.bounds;
        DebugDraw.Box(bounds.center, Quaternion.identity, bounds.size);
    }
}