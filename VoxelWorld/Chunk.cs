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
    private Task<Mesh?>? _modelGenTask;
    private Model? _model;
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

        Raylib.UnloadModel((Model) _model);
        _model = null;
        _dirty = true;
    }

    public void Render()
    {
        // If there was a mesh gen task running and it's complete we need to build the new model
        if (_modelGenTask is {IsCompleted: true})
        {
            UnloadModel();
            var tmpMesh = _modelGenTask.Result;
            if (tmpMesh != null)
            {
                var mesh = (Mesh) tmpMesh;
                Raylib.UploadMesh(ref mesh, false);
                _model = Raylib.LoadModelFromMesh(mesh);
            }

            _modelGenTask = null;
        }

        // Only starts a new task if one isn't already running
        if (_dirty && _modelGenTask == null)
        {
            _modelGenTask = Task.Run(GenerateMesh);
            _dirty = false;
        }

        if (_model == null) return;
        Raylib.DrawModel((Model) _model, _position * _dimensions, 1, Color.WHITE);
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

    // TODO: This probably shouldn't be in this file
    private Mesh? GenerateMesh()
    {
        var verticesVec3 = new List<Vector3>();
        var indices = new List<ushort>();
        var colours = new List<byte>();

        var neighbours = new Block[6];
        var vertIdxMap = new int[8];

        foreach (var block in _blocks)
        {
            if (block.BlockType == BlockType.Air) continue;

            var pos = block.Position;
            var startIdx = verticesVec3.Count;

            // Get all the neighbouring blocks
            neighbours[0] = GetBlock(pos with {Y = pos.Y + 1});
            neighbours[1] = GetBlock(pos with {Y = pos.Y - 1});
            neighbours[2] = GetBlock(pos with {Z = pos.Z + 1});
            neighbours[3] = GetBlock(pos with {Z = pos.Z - 1});
            neighbours[4] = GetBlock(pos with {X = pos.X + 1});
            neighbours[5] = GetBlock(pos with {X = pos.X - 1});

            for (int i = 0; i < 8; i++)
                vertIdxMap[i] = -1;

            int addedVerticesCount = 0;
            for (int i = 0; i < 6; i++)
            {
                // Ignore neighbours that aren't air
                var neighbour = neighbours[i];
                if (neighbour.BlockType != BlockType.Air) continue;

                for (int j = 0; j < 6; j++)
                {
                    var baseIdx = CubeTriangles[i * 6 + j];

                    // Add the vertex if it's not already added
                    if (vertIdxMap[baseIdx] == -1)
                    {
                        vertIdxMap[baseIdx] = addedVerticesCount;
                        addedVerticesCount++;
                        verticesVec3.Add(pos + CubeVertices[baseIdx]);

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

        // Build the model
        if (verticesVec3.Count == 0) return null;
        return MeshBuilder.BuildMesh(vertices.ToArray(), indices.ToArray(), colours.ToArray());
    }
}