using System.Numerics;
using LibNoise;
using LibNoise.Primitive;
using Raylib_cs;

namespace Generator;

public class Chunk
{
    public BoundingBox BoundingBox { get; }
    private readonly Vector3 _position;
    private readonly Vector3 _dimensions;
    private Block[,,] _blocks;
    private Model _model;
    private int _seed;

    public Chunk(Vector3 position, Vector3 dimensions, int seed)
    {
        _position = position;
        _dimensions = dimensions;
        _seed = seed;
        _blocks = new Block[(int) dimensions.X, (int) dimensions.Y, (int) dimensions.Z];

        var min = _position * _dimensions;
        var max = min + _dimensions;
        BoundingBox = new BoundingBox(min, max);
    }

    public void GenerateBlocks()
    {
        var generator = new SimplexPerlin(_seed, NoiseQuality.Best);

        var pos = _position * _dimensions;
        var scale = 1f / 32;
        for (int x = 0; x < _dimensions.X; x++)
        for (int y = 0; y < _dimensions.Y; y++)
        for (int z = 0; z < _dimensions.Z; z++)
        {
            var result = generator.GetValue((pos.X + x) * scale, (pos.Y + y) * scale, (pos.Z + z) * scale);
            var type = result > 0 ? BlockType.Stone : BlockType.Air;
            var color = new Vector4(result, result, result, 1) * 255;
            _blocks[x, y, z] = new Block(new Vector3(x, y, z), type, color);
        }
    }

    public void GenerateMesh()
    {
        var verticesVec3 = new List<Vector3>();
        var indices = new List<ushort>();
        var colours = new List<byte>();

        foreach (var block in _blocks)
        {
            if (block.BlockType == BlockType.Air) continue;

            var pos = block.Position;
            var startIdx = verticesVec3.Count;

            // Get all the neighbouring blocks
            var neighbours = new[]
            {
                GetBlockAtPos(pos with {Y = pos.Y + 1}),
                GetBlockAtPos(pos with {Y = pos.Y - 1}),
                GetBlockAtPos(pos with {Z = pos.Z + 1}),
                GetBlockAtPos(pos with {Z = pos.Z - 1}),
                GetBlockAtPos(pos with {X = pos.X + 1}),
                GetBlockAtPos(pos with {X = pos.X - 1}),
            };

            // Block vertices
            var blockVertices = new[]
            {
                pos,
                pos with {X = pos.X + 1},
                pos with {Y = pos.Y + 1},
                pos with {Z = pos.Z + 1},
                pos with {X = pos.X + 1, Y = pos.Y + 1},
                pos with {X = pos.X + 1, Z = pos.Z + 1},
                pos with {Y = pos.Y + 1, Z = pos.Z + 1},
                pos with {X = pos.X + 1, Y = pos.Y + 1, Z = pos.Z + 1},
            };

            var blockIndices = new[]
            {
                6, 7, 2, 2, 7, 4, // Top
                0, 1, 3, 3, 1, 5, // Bottom
                7, 6, 5, 5, 6, 3, // North
                2, 4, 0, 0, 4, 1, // South
                4, 7, 1, 1, 7, 5, // East
                6, 2, 3, 3, 2, 0, // West
            };

            var vertIdxMap = new int[8];
            for (int i = 0; i < 8; i++)
                vertIdxMap[i] = -1;

            int addedVerticesCount = 0;
            for (int i = 0; i < 6; i++)
            {
                // Ignore neighbours that aren't air
                // If the neighbour is in another chunk (null) then we add the face
                var neighbour = neighbours[i];
                if (neighbour != null && neighbour.BlockType != BlockType.Air) continue;

                for (int j = 0; j < 6; j++)
                {
                    var baseIdx = blockIndices[i * 6 + j];

                    // Add the vertex if it's not already added
                    if (vertIdxMap[baseIdx] == -1)
                    {
                        vertIdxMap[baseIdx] = addedVerticesCount;
                        addedVerticesCount++;
                        verticesVec3.Add(blockVertices[baseIdx]);

                        // Add the colour too!
                        var c = block.Color;
                        colours.Add((byte) c.X);
                        colours.Add((byte) c.Y);
                        colours.Add((byte) c.Z);
                        colours.Add(255);
                    }

                    // Add the mapped index
                    indices.Add((ushort) (startIdx + vertIdxMap[baseIdx]));
                }
            }
        }

        // Convert vec3 list to float list
        var vertices = new List<float>();
        foreach (var vertex in verticesVec3)
        {
            vertices.Add(vertex.X);
            vertices.Add(vertex.Y);
            vertices.Add(vertex.Z);
        }

        // Unload the old model from the gpu
        Raylib.UnloadModel(_model);

        // Build new mesh
        var mesh = new Mesh {vertexCount = verticesVec3.Count, triangleCount = indices.Count / 3};
        unsafe
        {
            fixed (float* verticesPtr = vertices.ToArray()) mesh.vertices = verticesPtr;
            fixed (ushort* indicesPtr = indices.ToArray()) mesh.indices = indicesPtr;
            fixed (byte* coloursPtr = colours.ToArray()) mesh.colors = coloursPtr;
        }

        Raylib.UploadMesh(ref mesh, false);
        _model = Raylib.LoadModelFromMesh(mesh);
    }

    private bool PosInChunk(Vector3 pos)
    {
        return pos.X >= 0 && pos.X < _dimensions.X &&
               pos.Y >= 0 && pos.Y < _dimensions.Y &&
               pos.Z >= 0 && pos.Z < _dimensions.Z;
    }

    private Block? GetBlockAtPos(Vector3 pos)
    {
        return !PosInChunk(pos) ? null : _blocks[(int) pos.X, (int) pos.Y, (int) pos.Z];
    }

    public void Render()
    {
        Raylib.DrawModel(_model, _position * _dimensions, 1, Color.WHITE);
    }
}