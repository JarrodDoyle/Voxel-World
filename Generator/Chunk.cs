using System.Numerics;
using Raylib_cs;

namespace Generator;

public class Chunk
{
    private Vector3 _position;
    private Vector3 _dimensions;
    private Block[,,] _blocks;
    private Model _model;

    public Chunk(Vector3 position, Vector3 dimensions)
    {
        _position = position;
        _dimensions = dimensions;
        _blocks = new Block[(int) dimensions.X, (int) dimensions.Y, (int) dimensions.Z];
    }

    public void GenerateBlocks()
    {
        var rnd = new Random();
        for (int x = 0; x < _dimensions.X; x++)
        for (int y = 0; y < _dimensions.Y; y++)
        for (int z = 0; z < _dimensions.Z; z++)
            // _blocks[x, y, z] = new Block(new Vector3(x, y, z), rnd.Next(0, 2) == 1 ? BlockType.Stone : BlockType.Air);
            _blocks[x, y, z] = new Block(new Vector3(x, y, z), BlockType.Stone);
    }

    public void GenerateMesh()
    {
        var start = DateTime.Now;
        
        var meshes = new List<Mesh>();
        var verticesVec3 = new List<Vector3>();
        var indices = new List<ushort>();
        var colours = new List<byte>();
        

        var rnd = new Random();
        foreach (var block in _blocks)
        {
            if (block.BlockType == BlockType.Air) continue;

            var pos = block.Position;
            var startIdx = verticesVec3.Count;

            // Add all the vertices
            verticesVec3.Add(pos);
            verticesVec3.Add(pos with {X = pos.X + 1});
            verticesVec3.Add(pos with {Y = pos.Y + 1});
            verticesVec3.Add(pos with {Z = pos.Z + 1});
            verticesVec3.Add(pos with {X = pos.X + 1, Y = pos.Y + 1});
            verticesVec3.Add(pos with {X = pos.X + 1, Z = pos.Z + 1});
            verticesVec3.Add(pos with {Y = pos.Y + 1, Z = pos.Z + 1});
            verticesVec3.Add(pos with {X = pos.X + 1, Y = pos.Y + 1, Z = pos.Z + 1});

            // Add vertex colours
            for (int i = 0; i < 8; i++)
            {
                colours.Add((byte) rnd.Next(100, 256));
                colours.Add((byte) rnd.Next(100, 256));
                colours.Add((byte) rnd.Next(100, 256));
                colours.Add(255);
            }

            // Add triangle indices
            var blockIndices = new[]
            {
                6, 7, 2, 2, 7, 4, // Top
                0, 1, 3, 3, 1, 5, // Bottom
                7, 6, 5, 5, 6, 3, // North
                2, 4, 0, 0, 4, 1, // South
                4, 7, 1, 1, 7, 5, // East
                6, 2, 3, 3, 2, 0, // West
            };
            
            var neighbours = new Block?[6];
            neighbours[0] = GetBlockAtPos(pos with {Y = pos.Y + 1});
            neighbours[1] = GetBlockAtPos(pos with {Y = pos.Y - 1});
            neighbours[2] = GetBlockAtPos(pos with {Z = pos.Z + 1});
            neighbours[3] = GetBlockAtPos(pos with {Z = pos.Z - 1});
            neighbours[4] = GetBlockAtPos(pos with {X = pos.X + 1});
            neighbours[5] = GetBlockAtPos(pos with {X = pos.X - 1});
            
            for (int i = 0; i < 6; i++)
            {
                var neighbour = neighbours[i];
                if (neighbour != null && neighbour.BlockType != BlockType.Air) continue;
            
                for (int j = 0; j < 6; j++)
                    indices.Add((ushort)(startIdx + blockIndices[i * 6 + j]));
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