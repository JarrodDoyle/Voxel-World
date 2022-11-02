using System.Numerics;
using Raylib_cs;

namespace VoxelWorld;

public unsafe struct VoxelMesh
{
    // This all assumes a 16x16x16 chunk.
    // Position -> vec3(0-16) = 15 bits
    // Face -> 0-5 = 3 bits (with 2 left over values)
    // Color -> 0-64? = 6 bits
    // 24 bits, 8 left over

    // Let's just get it working using the Raylib layouts for now
    private readonly int _vertexCount;
    private readonly int _triangleCount;
    private readonly float* _vertices;
    private readonly byte* _colors;
    private readonly ushort* _indices;

    private uint _vaoId;
    private readonly uint[] _vboId;

    public VoxelMesh(float[] vertices, ushort[] indices, byte[] colors)
    {
        _vertexCount = vertices.Length / 3;
        _triangleCount = indices.Length / 3;

        _vertices = (float*) Raylib.MemAlloc(sizeof(float) * vertices.Length);
        _indices = (ushort*) Raylib.MemAlloc(sizeof(ushort) * indices.Length);
        _colors = (byte*) Raylib.MemAlloc(sizeof(byte) * colors.Length);
        for (int i = 0; i < vertices.Length; i++) _vertices[i] = vertices[i];
        for (int i = 0; i < indices.Length; i++) _indices[i] = indices[i];
        for (int i = 0; i < colors.Length; i++) _colors[i] = colors[i];

        _vaoId = 0;
        _vboId = new[] {(uint) 0, (uint) 0, (uint) 0};
    }

    public void Upload()
    {
        if (_vaoId > 0)
        {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, $"VAO: [ID {_vaoId}] Trying to re-load an already loaded mesh");
            return;
        }
        
        // Create and bind VAO
        _vaoId = Rlgl.rlLoadVertexArray();
        Rlgl.rlEnableVertexArray(_vaoId);
        
        // Enable vertex attributes
        // position (shader-location = 0)
        _vboId[0] = Rlgl.rlLoadVertexBuffer(_vertices, _vertexCount * 3 * sizeof(float), false);
        Rlgl.rlSetVertexAttribute(0, 3, Rlgl.RL_FLOAT, false, 0, null);
        Rlgl.rlEnableVertexAttribute(0);

        // color (shader-location = 3)
        _vboId[1] = Rlgl.rlLoadVertexBuffer(_colors, _vertexCount * 4 * sizeof(byte), false);
        Rlgl.rlSetVertexAttribute(3, 4, Rlgl.RL_UNSIGNED_BYTE, true, 0, null);
        Rlgl.rlEnableVertexAttribute(3);
        
        // Indices
        _vboId[2] = Rlgl.rlLoadVertexBufferElement(_indices, _triangleCount * 3 * sizeof(ushort), false);
        
        if (_vaoId > 0)
            Raylib.TraceLog(TraceLogLevel.LOG_INFO, $"VAO: [ID {_vaoId}] VoxelMesh uploaded successfully to VRAM (GPU)");
        Rlgl.rlDisableVertexArray();
    }

    public void Render(Vector3 position, float scale, Color tint, Shader shader)
    {
        if (_vaoId <= 0) return;
        if (!Rlgl.rlEnableVertexArray(_vaoId))
            Raylib.TraceLog(TraceLogLevel.LOG_ERROR, $"VAO: [ID {_vaoId}] Failed to bind VAO.");
        
        // Calculate transformation matrix
        var matScale = Matrix4x4.CreateScale(scale);
        var matRotation = Matrix4x4.CreateRotationY(0);
        var matTranslation = Matrix4x4.CreateTranslation(position);
        var matTransform = Matrix4x4.Multiply(Matrix4x4.Multiply(matScale, matRotation), matTranslation);
        var transform = Matrix4x4.Transpose(matTransform);
        
        // Set shader uniforms
        Rlgl.rlEnableShader(shader.id);
        Rlgl.rlSetUniformMatrix(shader.locs[7], Rlgl.rlGetMatrixModelview()); // matView
        Rlgl.rlSetUniformMatrix(shader.locs[8], Rlgl.rlGetMatrixProjection()); // matProjection
        Rlgl.rlSetUniformMatrix(shader.locs[9], transform * Rlgl.rlGetMatrixTransform()); // matModel
        
        // Draw the mesh
        Rlgl.rlDrawVertexArrayElements(0, _triangleCount * 3, null);

        // Unbind tings
        Rlgl.rlDisableVertexArray();
        Rlgl.rlDisableShader();
    }

    public void Unload()
    {
        Rlgl.rlUnloadVertexArray(_vaoId);
        foreach (var id in _vboId)
            Rlgl.rlUnloadVertexBuffer(id);
        
        Raylib.MemFree(_vertices);
        Raylib.MemFree(_indices);
        Raylib.MemFree(_colors);
    }
}