using System.Numerics;
using Raylib_cs;
using Utils;

namespace VoxelWorld;

public struct VoxelMesh
{
    // This all assumes a 16x16x16 chunk.
    // Position -> vec3(0-16) = 15 bits
    // Face -> 0-5 = 3 bits (with 2 left over values)
    // Color -> 0-64? = 6 bits
    // 24 bits, 8 left over

    // Let's just get it working using the Raylib layouts for now
    private Mesh _mesh;
    private Model? _model;

    public VoxelMesh(float[] vertices, ushort[] indices, byte[] colors)
    {
        _mesh = MeshBuilder.BuildMesh(vertices, indices, colors);
        _model = null;
    }

    public void Upload()
    {
        Raylib.UploadMesh(ref _mesh, false);
        _model = Raylib.LoadModelFromMesh(_mesh);
    }

    public void Render(Vector3 position, float scale, Color tint)
    {
        if (_model == null) return;
        Raylib.DrawModel((Model) _model, position, scale, tint);
    }

    public void Unload()
    {
        if (_model == null) return;
        Raylib.UnloadModel((Model) _model);
    }
}