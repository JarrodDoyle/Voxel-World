using System.Numerics;
using LibNoise;
using LibNoise.Primitive;

namespace VoxelWorld.Generation;

public class Simplex2dWorld : WorldGenerator
{
    private SimplexPerlin _noiseFunction;
    private float _scale;

    public Simplex2dWorld(int seed, float scale)
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
            var value = NoiseValue(genPos.X, genPos.Z);
            var blockType = worldPosition.Y + y < 32 + 20 * value ? BlockType.Stone : BlockType.Air;
            var blockColor = new Vector4(blockPosition * 256 / dimensions, 255);
            chunk.SetBlock(x, y, z, new Block(blockPosition, blockType, blockColor));
        }

        return chunk;
    }

    private float NoiseValue(float x, float z)
    {
        const int octaves = 3;
        var frequency = 1.0f;
        var amplitude = 1.0f;
        var totalAmplitude = 0.0f;
        var result = 0.0f;
        for (int i = 0; i < octaves; i++)
        {
            result += _noiseFunction.GetValue(x * frequency, z * frequency) * amplitude;
            totalAmplitude += amplitude;
            frequency *= 2.0f;
            amplitude *= 0.5f;
        }
        result /= totalAmplitude;
        return result;
    }
}