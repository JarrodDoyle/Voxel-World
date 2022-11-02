using System.Numerics;
using Raylib_cs;
using Utils;

namespace VoxelWorld;

public class Chunk
{
    private readonly Vector3 _position;
    private readonly Vector3 _dimensions;
    private readonly Block[,,] _blocks;
    private bool _dirty;
    private Task<VoxelMesh?>? _modelGenTask;
    private VoxelMesh? _model;
    private readonly World _world;

    public Chunk(Vector3 position, Vector3 dimensions, World world)
    {
        _position = position;
        _dimensions = dimensions;
        _blocks = new Block[(int) dimensions.X, (int) dimensions.Y, (int) dimensions.Z];
        _dirty = false;
        _world = world;
    }

    public Block GetBlock(Vector3 pos)
    {
        if (PosInChunk(pos))
            return _blocks[(int) pos.X, (int) pos.Y, (int) pos.Z];
        var block = _world.GetBlock((_position * _dimensions) + pos);
        return block;
    }

    public Block GetBlock(int x, int y, int z)
    {
        return GetBlock(new Vector3(x, y, z));
    }

    public bool SetBlock(Vector3 pos, Block value)
    {
        if (!PosInChunk(pos)) return false;
        _blocks[(int) pos.X, (int) pos.Y, (int) pos.Z] = value;
        _dirty = true;
        return true;
    }

    public bool SetBlock(int x, int y, int z, Block value)
    {
        return SetBlock(new Vector3(x, y, z), value);
    }

    public bool PosInChunk(Vector3 pos)
    {
        return pos.X >= 0 && pos.X < _dimensions.X &&
               pos.Y >= 0 && pos.Y < _dimensions.Y &&
               pos.Z >= 0 && pos.Z < _dimensions.Z;
    }

    public void UnloadModel()
    {
        if (_model == null) return;

        _model?.Unload();
        _model = null;
        _dirty = true;
    }

    public void Render(Shader shader)
    {
        // If there was a mesh gen task running and it's complete we need to build the new model
        if (_modelGenTask is {IsCompleted: true})
        {
            UnloadModel();
            var tmpMesh = _modelGenTask.Result;
            if (tmpMesh != null)
            {
                var mesh = (VoxelMesh) tmpMesh;
                mesh.Upload();
                _model = mesh;
            }

            _modelGenTask = null;
        }

        // Only starts a new task if one isn't already running
        if (_dirty && _modelGenTask == null)
        {
            _modelGenTask = Task.Run(GenerateMesh);
            _dirty = false;
        }

        _model?.Render(_position * _dimensions, 1, Color.WHITE, shader);
    }

    private static readonly Vector3[] CubeVertices =
    {
        new(0, 0, 0),
        new(1, 0, 0),
        new(0, 1, 0),
        new(0, 0, 1),
        new(1, 1, 0),
        new(1, 0, 1),
        new(0, 1, 1),
        new(1, 1, 1),
    };

    private static readonly int[] CubeTriangles =
    {
        6, 7, 2, 2, 7, 4, // Top
        0, 1, 3, 3, 1, 5, // Bottom
        7, 6, 5, 5, 6, 3, // North
        2, 4, 0, 0, 4, 1, // South
        4, 7, 1, 1, 7, 5, // East
        6, 2, 3, 3, 2, 0, // West
    };

    private const float AmbientStrength = 0.25f;
    private static readonly Vector3 AmbientDir = new(1, 3, 2);

    // TODO: This probably shouldn't be in this file
    private VoxelMesh? GenerateMesh()
    {
        var verticesVec3 = new List<Vector3>();
        var indices = new List<ushort>();
        var colours = new List<byte>();

        var neighbours = new Block[6];
        var vertIdxMap = new int[8];

        // Calculate ambient colours
        var light =
            Vector3.Abs(Vector3.Normalize(AmbientDir) * (1.0f - AmbientStrength) + new Vector3(AmbientStrength));
        var ambientMultipliers = new float[6];
        ambientMultipliers[0] = light.Y;
        ambientMultipliers[1] = 1 - light.Y;
        ambientMultipliers[2] = light.Z;
        ambientMultipliers[3] = 1 - light.Z;
        ambientMultipliers[4] = light.X;
        ambientMultipliers[5] = 1 - light.X;

        var indicesIdx = 0;

        // foreach (var block in _blocks)
        for (var x = 0; x < _dimensions.X; x++)
        for (var y = 0; y < _dimensions.Y; y++)
        for (var z = 0; z < _dimensions.Z; z++)
        {
            var block = _blocks[x, y, z];
            if (block.BlockType == BlockType.Air) continue;

            var pos = new Vector3(x, y, z);

            // Get all the neighbouring blocks
            neighbours[0] = GetBlock(pos with {Y = pos.Y + 1});
            neighbours[1] = GetBlock(pos with {Y = pos.Y - 1});
            neighbours[2] = GetBlock(pos with {Z = pos.Z + 1});
            neighbours[3] = GetBlock(pos with {Z = pos.Z - 1});
            neighbours[4] = GetBlock(pos with {X = pos.X + 1});
            neighbours[5] = GetBlock(pos with {X = pos.X - 1});

            // Iterate over each side neighbour
            for (int i = 0; i < 6; i++)
            {
                // Ignore neighbours that aren't air
                var neighbour = neighbours[i];
                if (neighbour.BlockType != BlockType.Air) continue;

                var ambient = ambientMultipliers[i];

                // Clear the Vertex -> Index map
                for (var j = 0; j < 8; j++)
                    vertIdxMap[j] = -1;

                // We're adding a face so we need to add the four vertices and the 6 indices (2 tris to make the quad)
                for (var j = 0; j < 6; j++)
                {
                    var ctIdx = CubeTriangles[i * 6 + j];
                    if (vertIdxMap[ctIdx] == -1)
                    {
                        // If we've not got a mapped index for this CubeTriangle vertex we create the mapping and add
                        // the vertex to the mesh (+ it's other info like vertex colours and UVs (eventually))
                        vertIdxMap[ctIdx] = indicesIdx;
                        indicesIdx++;

                        verticesVec3.Add(pos + CubeVertices[ctIdx]);
                        var c = block.Color;
                        colours.Add((byte) (c.X * ambient));
                        colours.Add((byte) (c.Y * ambient));
                        colours.Add((byte) (c.Z * ambient));
                        colours.Add(255);
                    }

                    // Add the mapped index
                    indices.Add((ushort) vertIdxMap[ctIdx]);
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

        // Build the model
        if (verticesVec3.Count == 0) return null;
        return new VoxelMesh(vertices.ToArray(), indices.ToArray(), colours.ToArray());
    }
}