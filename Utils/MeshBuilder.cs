using Raylib_cs;

namespace Utils;

public static class MeshBuilder
{
    public static Mesh BuildMesh(float[] vertices, ushort[] indices, byte[] colors)
    {
        var mesh = new Mesh {vertexCount = vertices.Length / 3, triangleCount = indices.Length / 3};
        unsafe
        {
            // Need to allocate memory manually here! Don't want that pesky GC messing things up
            mesh.vertices = (float*) Raylib.MemAlloc(sizeof(float) * vertices.Length);
            mesh.indices = (ushort*) Raylib.MemAlloc(sizeof(ushort) * indices.Length);
            mesh.colors = (byte*) Raylib.MemAlloc(sizeof(byte) * colors.Length);

            // Copy data across
            for (int i = 0; i < vertices.Length; i++) mesh.vertices[i] = vertices[i];
            for (int i = 0; i < indices.Length; i++) mesh.indices[i] = indices[i];
            for (int i = 0; i < colors.Length; i++) mesh.colors[i] = colors[i];
        }

        return mesh;
    }
}