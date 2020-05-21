using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IVoxelBounds2
{
    (int x, int y) Min { get; }
    (int x, int y) Max { get; }

    IEnumerable<(int x, int y)> GetVoxelsInBounds();

    IVoxelBounds2 Union(IVoxelBounds2 other);
    IVoxelBounds2 Intersect(IVoxelBounds2 other);
}

public interface IVoxelBounds3
{
    (int x, int y, int z) Min { get; }
    (int x, int y, int z) Max { get; }

    IEnumerable<(int x, int y, int z)> GetVoxelsInBounds();

    IVoxelBounds3 Union(IVoxelBounds3 other);
    IVoxelBounds3 Intersect(IVoxelBounds3 other);
}