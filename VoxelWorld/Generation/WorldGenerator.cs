using System.Numerics;

namespace VoxelWorld.Generation;

public abstract class WorldGenerator
{
    public abstract Chunk GenerateChunk(Vector3 position, Vector3 dimensions, World world);
    public abstract Block GenerateBlock(Vector3 worldPosition);
}