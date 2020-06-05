using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct VoxelBounds3 : IVoxelBounds3
{
    public readonly (int x, int y, int z)
        min,
        max;

    public VoxelBounds3(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        : this((minX, minY, minZ), (maxX, maxY, maxZ))
    {

    }

    public VoxelBounds3((int x, int y, int z) min, (int x, int y, int z) max)
    {
        this.min = min;
        this.max = max;
    }

    public ((int x, int y, int z) min, (int x, int y, int z) max) GetMinAndMax(IVoxelOrientation orientation)
    {
        return (min, max);
    }
}

public struct VoxelBounds2 : IVoxelBounds2
{
    public readonly (int x, int y)
        min,
        max;

    public VoxelBounds2(int minX, int minY, int maxX, int maxY)
        : this((minX, minY), (maxX, maxY))
    {

    }

    public VoxelBounds2((int x, int y) min, (int x, int y) max)
    {
        this.min = min;
        this.max = max;
    }

    public ((int x, int y) min, (int x, int y) max) GetMinAndMax(IVoxelOrientation orientation)
    {
        return (min, max);
    }
}
