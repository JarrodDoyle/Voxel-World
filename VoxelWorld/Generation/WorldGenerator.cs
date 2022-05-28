using System.Numerics;

namespace VoxelWorld.Generation;

public abstract class WorldGenerator
{
    public abstract Chunk GenerateChunk(Vector3 position, Vector3 dimensions);
}