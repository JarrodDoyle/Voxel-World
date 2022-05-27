using System.Numerics;
using LibNoise.Primitive;
using Raylib_cs;

namespace Generator;

public class Chunk : IDisposable
{
    public BoundingBox BoundingBox { get; }
    private readonly Vector3 _position;
    private readonly Vector3 _dimensions;
    private Block[,,] _blocks;
    private Model _model;
    private bool _dirty;
    
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

    public Chunk(Vector3 position, Vector3 dimensions)
    {
        _position = position;
        _dimensions = dimensions;
        _blocks = new Block[(int) dimensions.X, (int) dimensions.Y, (int) dimensions.Z];

        var min = _position * _dimensions;
        var max = min + _dimensions;
        BoundingBox = new BoundingBox(min, max);
    }

    public Block GetBlock(int x, int y, int z)
    {
        return _blocks[x, y, z];
    }

    public void SetBlock(int x, int y, int z, Block value)
    {
        _blocks[x, y, z] = value;
        _dirty = true;
    }

    public void GenerateBlocks(SimplexPerlin generator)
    {
        var pos = _position * _dimensions;
        var scale = 1f / 32;
        for (int x = 0; x < _dimensions.X; x++)
        for (int y = 0; y < _dimensions.Y; y++)
        for (int z = 0; z < _dimensions.Z; z++)
        {
            var result = generator.GetValue((pos.X + x) * scale, (pos.Y + y) * scale, (pos.Z + z) * scale);
            var type = result > 0 ? BlockType.Stone : BlockType.Air;
            // var color = new Vector4(result, result, result, 1) * 255;
            
            // var result = generator.GetValue((pos.X + x) * scale, (pos.Z + z) * scale);
            // var type = pos.Y + y < 32 + 20 * result ? BlockType.Stone : BlockType.Air;
            var color = new Vector4(x * 16, y * 16, z * 16, 255);
            _blocks[x, y, z] = new Block(new Vector3(x, y, z), type, color);
        }
    }

    public void GenerateMesh()
    {
        var verticesVec3 = new List<Vector3>();
        var indices = new List<ushort>();
        var colours = new List<byte>();
    
        var neighbours = new Block?[6];
        var vertIdxMap = new int[8];

        foreach (var block in _blocks)
        {
            if (block.BlockType == BlockType.Air) continue;
    
            var pos = block.Position;
            var startIdx = verticesVec3.Count;
    
            // Get all the neighbouring blocks
            neighbours[0] = GetBlockAtPos(pos with {Y = pos.Y + 1});
            neighbours[1] = GetBlockAtPos(pos with {Y = pos.Y - 1});
            neighbours[2] = GetBlockAtPos(pos with {Z = pos.Z + 1});
            neighbours[3] = GetBlockAtPos(pos with {Z = pos.Z - 1});
            neighbours[4] = GetBlockAtPos(pos with {X = pos.X + 1});
            neighbours[5] = GetBlockAtPos(pos with {X = pos.X - 1});
            
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
    
        // Unload the old model from the gpu
        Raylib.UnloadModel(_model);
    
        // Build new mesh
        var mesh = new Mesh {vertexCount = verticesVec3.Count, triangleCount = indices.Count / 3};
        unsafe
        {
            // Need to allocate memory manually here! Don't want that pesky GC messing things up
            mesh.vertices = (float*) Raylib.MemAlloc(sizeof(float) * vertices.Count);
            mesh.indices = (ushort*) Raylib.MemAlloc(sizeof(ushort) * indices.Count);
            mesh.colors = (byte*) Raylib.MemAlloc(sizeof(byte) * colours.Count);
    
            // Convert to arrays to iterate nicer
            var verticesArr = vertices.ToArray();
            var indicesArr = indices.ToArray();
            var coloursArr = colours.ToArray();
    
            // Copy data across
            for (int i = 0; i < vertices.Count; i++) mesh.vertices[i] = verticesArr[i];
            for (int i = 0; i < indices.Count; i++) mesh.indices[i] = indicesArr[i];
            for (int i = 0; i < colours.Count; i++) mesh.colors[i] = coloursArr[i];
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
        if (_dirty)
        {
            GenerateMesh();
            _dirty = false;
        }
        
        Raylib.DrawModel(_model, _position * _dimensions, 1, Color.WHITE);
        // Raylib.DrawBoundingBox(BoundingBox, Color.GOLD);
    }

    public void Dispose()
    {
        Raylib.UnloadModel(_model);
    }
}