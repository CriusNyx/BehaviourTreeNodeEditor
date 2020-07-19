using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilderAsset
{
    public readonly GameObject gameObject;
    public readonly Texture2D icon;
    public readonly string filename;

    public LevelBuilderAsset(GameObject gameObject, Texture2D icon, string filename)
    {
        this.gameObject = gameObject;
        this.icon = icon;
        this.filename = filename;
    }
}