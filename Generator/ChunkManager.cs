using System.Numerics;
using Utils;

namespace Generator;

public static class ChunkManager
{
    public static int Seed { get; set; }
    public static Vector3 ChunkDimensions { get; set; } = Vector3.One;
    private static List<Chunk> _chunks = new();

    public static void LoadChunk(Vector3 position)
    {
        var chunk = new Chunk(position, ChunkDimensions, Seed);
        chunk.GenerateBlocks();
        chunk.GenerateMesh();
        _chunks.Add(chunk);
    }

    public static void RegenerateChunks()
    {
        var startTime = DateTime.Now;
        foreach (var chunk in _chunks)
        {
            chunk.Seed = Seed;
            chunk.GenerateBlocks();
            chunk.GenerateMesh();
        }

        Console.WriteLine($"Chunk gen time: {DateTime.Now - startTime}");
    }

    public static void Render()
    {
        var frustum = new Frustum();
        foreach (var chunk in _chunks)
            if (frustum.AabbInside(chunk.BoundingBox))
                chunk.Render();
    }

    public static int LoadedChunksCount()
    {
        return _chunks.Count;
    }
}