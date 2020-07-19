using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RectExtensions
{
    public static Rect Translate(this Rect rect, Vector2 vector)
    {
        return new Rect(rect.position + vector, rect.size);
    }
}