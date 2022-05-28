using System.Numerics;
using LibNoise;
using LibNoise.Primitive;

namespace VoxelWorld.Generation;

public class Simplex3dWorld : WorldGenerator
{
    private SimplexPerlin _noiseFunction;
    private float _scale;
    
    public Simplex3dWorld(int seed, float scale)
    {
        _noiseFunction = new SimplexPerlin(seed, NoiseQuality.Best);
        _scale = scale;
    }

    public override Chunk GenerateChunk(Vector3 position, Vector3 dimensions)
    {
        var chunk = new Chunk(position, dimensions);
        var worldPosition = position * dimensions;
        
        for (var x = 0; x < dimensions.X; x++)
        for (var y = 0; y < dimensions.Y; y++)
        for (var z = 0; z < dimensions.Z; z++)
        {
            var blockPosition = new Vector3(x, y, z);
            var genPos = (worldPosition + blockPosition) * _scale;
            var value = _noiseFunction.GetValue(genPos.X, genPos.Y, genPos.Z);
            var blockType = value > 0 ? BlockType.Stone : BlockType.Air;
            var blockColor = new Vector4(blockPosition * 256 / dimensions, 255);
            chunk.SetBlock(x, y, z, new Block(blockPosition, blockType, blockColor));
        }

        return chunk;
    }
}