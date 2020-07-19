using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SnappingToolSettings : ScriptableObject
{
    public Snapper.SnapMode snapMode = Snapper.SnapMode.both;
    public (bool snapPosition, bool snapRotation) GetSnappingMode()
    {
        return
            ((snapMode & Snapper.SnapMode.position) != 0,
            (snapMode & Snapper.SnapMode.rotation) != 0);
    }

    public void SetSnappingMode(bool snapPosition, bool snapRotation)
    {
        Snapper.SnapMode snapMode = Snapper.SnapMode.none;

        if (snapPosition) 
            snapMode = snapMode | Snapper.SnapMode.position;
        if (snapRotation)
            snapMode = snapMode | Snapper.SnapMode.rotation;

        this.snapMode = snapMode;
    }

    //[MenuItem("Experimental/CreateFolders")]
    public static void FolderTest()
    {
        string path = "Assets/b/b/c/d";
        AssetManagement.BuildAllDependantDirectories(path);
    }
}
