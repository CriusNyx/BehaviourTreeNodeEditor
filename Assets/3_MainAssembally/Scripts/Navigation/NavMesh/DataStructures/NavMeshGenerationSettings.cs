using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NavMeshGenerationSettings
{
    public readonly VoxelOrientation orientation;
    public readonly int maxClimbDistanceInVoxels;
    public readonly int maxFallDistanceInVoxels;
    public readonly int crouchHeightInVoxels;
    public readonly int standingHeightInVoxels;

    public NavMeshGenerationSettings(
        VoxelOrientation orientation,
        int maxClimbDistanceInVoxels,
        int maxFallDistanceInVoxels,
        int crouchHeightInVoxels,
        int standingHeightInVoxels)
    {
        this.orientation = orientation;
        this.maxClimbDistanceInVoxels = maxClimbDistanceInVoxels;
        this.maxFallDistanceInVoxels = maxFallDistanceInVoxels;
        this.crouchHeightInVoxels = crouchHeightInVoxels;
        this.standingHeightInVoxels = standingHeightInVoxels;
    }

    public bool CanClimbTo(float a, float b)
    {
        return b >= a && b - a <= maxClimbDistanceInVoxels * orientation.voxelSize * 1.000001f;
    }

    public bool CanDropTo(float a, float b)
    {
        return a >= b && a - b <= maxFallDistanceInVoxels * orientation.voxelSize * 1.000001f;
    }
}