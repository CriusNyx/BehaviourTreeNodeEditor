using System;
using UnityEngine;

public interface IVoxelBounds2
{
    ((int x, int y) min, (int x, int y) max) GetMinAndMax(IVoxelOrientation orientation);
}

public interface IVoxelBounds3
{
    ((int x, int y, int z) min, (int x, int y, int z) max) GetMinAndMax(IVoxelOrientation orientation);
}