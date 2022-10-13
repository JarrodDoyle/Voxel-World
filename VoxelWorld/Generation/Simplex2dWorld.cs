using System.Numerics;

namespace VoxelWorld.Generation;

public class Simplex2dWorld : WorldGenerator
{
    private readonly FastNoise _noiseGenerator;
    private readonly float _scale;
    private readonly int _seed;

    public Simplex2dWorld(int seed, float scale)
    {
        _seed = seed;
        _scale = scale;

        _noiseGenerator = new FastNoise("FractalFBm");
        _noiseGenerator.Set("Source", new FastNoise("Simplex"));
        _noiseGenerator.Set("Gain", 0.5f);
        _noiseGenerator.Set("Octaves", 5);
        _noiseGenerator.Set("Lacunarity", 2f);
    }

    public override Chunk GenerateChunk(Vector3 position, Vector3 dimensions, World world)
    {
        var chunk = new Chunk(position, dimensions, world);

        var xSize = (int) dimensions.X;
        var ySize = (int) dimensions.Y;
        var zSize = (int) dimensions.Z;
        var xStart = xSize * (int) position.X;
        var yStart = ySize * (int) position.Y;
        var zStart = zSize * (int) position.Z;

        var noiseData = new float[xSize * zSize];
        _noiseGenerator.GenUniformGrid2D(noiseData, xStart, zStart, xSize, zSize, _scale, _seed);
        for (var x = 0; x < xSize; x++)
        for (var z = 0; z < zSize; z++)
        {
            var idx = (z * xSize) + x;
            var value = noiseData[idx];
            for (var y = 0; y < ySize; y++)
            {
                var blockPosition = new Vector3(x, y, z);
                var blockType = yStart + y < 32 + 20 * value ? BlockType.Stone : BlockType.Air;
                var blockColor = new Vector4(blockPosition * 256 / dimensions, 255);
                chunk.SetBlock(x, y, z, new Block(blockPosition, blockType, blockColor));
            }
        }

        return chunk;
    }

    public override Block GenerateBlock(Vector3 worldPosition)
    {
        var x = (int) worldPosition.X;
        var y = (int) worldPosition.Y;
        var z = (int) worldPosition.Z;
        
        var value = _noiseGenerator.GenSingle2D(x * _scale, z * _scale, _seed);
        var blockType = y < 32 + 20 * value ? BlockType.Stone : BlockType.Air;
        var blockPosition = new Vector3(x, y, z);
        var blockColor = new Vector4(255);
        return new Block(blockPosition, blockType, blockColor);
    }
}