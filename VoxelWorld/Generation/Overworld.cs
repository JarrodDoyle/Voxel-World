using System.Numerics;

namespace VoxelWorld.Generation;

public class Overworld : WorldGenerator
{
    private readonly FastNoise _noiseGenerator;
    private float _scale;
    private readonly int _seed;

    public Overworld(int seed, float scale)
    {
        _seed = seed;
        _scale = scale;
        
        _noiseGenerator = FastNoise.FromEncodedNodeTree(
            "HgATAJqZmT4aAAERAAIAAAAAAOBAEAAAAIhBHwAWAAEAAAALAAMAAAACAAAAAwAAAAQAAAAAAAAAPwEUAP//AAAAAAAAPwAAAAA/AAAAAD8AAAAAPwEXAAAAgL8AAIA/PQoXQFK4HkATAAAAoEAGAACPwnU8AJqZmT4AAAAAAADhehQ/AREAAgAAAAAAIEAQAAAAAEAZABMAw/UoPw0ABAAAAAAAIEAJAABmZiY/AAAAAD8BBAAAAAAAAABAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADNzEw+ADMzMz8AAAAAPw=="
        );
    }

    public override Chunk GenerateChunk(Vector3 position, Vector3 dimensions)
    {
        var chunk = new Chunk(position, dimensions);

        var xSize = (int) dimensions.X;
        var ySize = (int) dimensions.Y;
        var zSize = (int) dimensions.Z;
        var xStart = xSize * (int) position.X;
        var yStart = ySize * (int) position.Y;
        var zStart = zSize * (int) position.Z;

        var noiseData = new float[xSize * ySize * zSize];
        _noiseGenerator.GenUniformGrid3D(noiseData, xStart, yStart, zStart, xSize, ySize, zSize, _scale, _seed);
        for (var x = 0; x < xSize; x++)
        for (var y = 0; y < zSize; y++)
        for (var z = 0; z < zSize; z++)
        {
            var idx = (z * ySize * xSize) + (y * xSize) + x;
            var value = noiseData[idx];

            var blockPosition = new Vector3(x, y, z);
            var blockType = value > 0 ? BlockType.Air : BlockType.Stone;
            var blockColor = new Vector4(blockPosition * 256 / dimensions, 255);
            chunk.SetBlock(x, y, z, new Block(blockPosition, blockType, blockColor));
        }

        return chunk;
    }
}