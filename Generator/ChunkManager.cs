using System.Numerics;
using LibNoise;
using LibNoise.Primitive;
using Utils;

namespace Generator;

public static class ChunkManager
{
    public static int Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            _generator = new SimplexPerlin(_seed, NoiseQuality.Best);
        }
    }

    public static Vector3 ChunkDimensions { get; set; } = Vector3.One;
    private static SimplexPerlin _generator= new();

    private static Dictionary<Vector3, Chunk> _chunks = new();
    private static int _seed;

    public static void LoadChunk(Vector3 position)
    {
        if (_chunks.ContainsKey(position)) return;

        var chunk = new Chunk(position, ChunkDimensions);
        chunk.GenerateBlocks(_generator);
        chunk.GenerateMesh();
        _chunks.Add(position, chunk);
    }

    public static void RegenerateChunks()
    {
        var startTime = DateTime.Now;
        foreach (var chunk in _chunks.Values)
        {
            chunk.GenerateBlocks(_generator);
            chunk.GenerateMesh();
        }

        Console.WriteLine($"Chunk gen time: {DateTime.Now - startTime}");
    }

    public static void Render()
    {
        var frustum = new Frustum();
        foreach (var chunk in _chunks.Values)
            if (frustum.AabbInside(chunk.BoundingBox))
                chunk.Render();
    }

    public static int LoadedChunksCount()
    {
        return _chunks.Count;
    }

    public static void LoadChunksAroundPos(Vector3 pos, int radius)
    {
        var startTime = DateTime.Now;
        var startSize = _chunks.Count;

        for (int x = -radius; x <= radius; x++)
        for (int y = -radius; y <= radius; y++)
        for (int z = -radius; z <= radius; z++)
            LoadChunk(new Vector3(pos.X + x, pos.Y + y, pos.Z + z));

        var loadedNewChunks = _chunks.Count != startSize;

        foreach (var (key, chunk) in _chunks)
        {
            if (key.X < pos.X - radius || key.X > pos.X + radius ||
                key.Y < pos.Y - radius || key.Y > pos.Y + radius ||
                key.Z < pos.Z - radius || key.Z > pos.Z + radius)
            {
                chunk.Dispose();
                _chunks.Remove(key);
            }
        }

        if (loadedNewChunks) Console.WriteLine($"Generation time: {DateTime.Now - startTime}");
    }
}