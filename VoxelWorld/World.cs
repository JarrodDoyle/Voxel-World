using System.Collections.Concurrent;
using System.Numerics;
using VoxelWorld.Generation;

namespace VoxelWorld;

public class World
{
    public int Seed { get; }
    public Vector3 ChunkDimensions { get; }
    private readonly WorldGenerator _generator;
    private readonly ConcurrentDictionary<Vector3, Chunk> _chunks;
    private readonly ConcurrentDictionary<Vector3, byte> _loadingChunks;

    public static Vector4[] Palette = {new(255)};

    public World(int seed, int worldType, Vector3 dimensions)
    {
        Seed = seed;
        ChunkDimensions = dimensions;
        _chunks = new ConcurrentDictionary<Vector3, Chunk>();
        _loadingChunks = new ConcurrentDictionary<Vector3, byte>();

        const float scale = 0.005f;
        _generator = worldType switch
        {
            0 => new Simplex2dWorld(seed, scale),
            1 => new Simplex3dWorld(seed, scale),
            2 => new Overworld(seed, scale),
            _ => throw new ArgumentOutOfRangeException(nameof(worldType), worldType, null)
        };
    }

    /// <summary>
    /// Set's the shared world palette
    /// </summary>
    /// <remarks>
    /// Expects a file with a RGB hex value on each line
    /// </remarks>
    /// <param name="filepath"></param>
    public static void LoadPalette(string filepath)
    {
        if (!File.Exists(filepath)) return;

        var paletteList = new List<Vector4>();
        foreach (var line in File.ReadLines(filepath))
        {
            var value = int.Parse(line, System.Globalization.NumberStyles.HexNumber);
            var r = (value >> 16) & 0xFF;
            var g = (value >> 8) & 0xFF;
            var b = value & 0xFF;
            paletteList.Add(new Vector4(r, g, b, 255));
        }

        Palette = paletteList.ToArray();
    }

    /// <summary>
    /// Gets the currently loaded chunk world positions as an enumerable collection.
    /// </summary>
    /// <returns>Enumerable collection of the currently loaded chunk world positions</returns>
    public IEnumerable<Vector3> GetLoadedChunkPositions()
    {
        return _chunks.Keys;
    }

    /// <summary>
    /// Gets the currently loading chunk world positions as an enumerable collection.
    /// </summary>
    /// <returns>Enumerable collection of the currently loading chunk world positions</returns>
    public IEnumerable<Vector3> GetLoadingChunkPositions()
    {
        return _loadingChunks.Keys;
    }

    /// <summary>
    /// Determines whether the data of the chunk at the specified position is loaded.
    /// </summary>
    /// <param name="chunkPosition">The world position of the chunk to check for.</param>
    /// <returns><c>true</c> if the chunk data is loaded; otherwise, <c>false</c>.</returns>
    public bool ChunkIsLoaded(Vector3 chunkPosition)
    {
        return _chunks.ContainsKey(chunkPosition);
    }

    /// <summary>
    /// Determines whether the data of the chunk at the specified position is currently loading.
    /// </summary>
    /// <param name="chunkPosition">The world position of the chunk to check for.</param>
    /// <returns><c>true</c> if the chunk data is currently loading; otherwise, <c>false</c>.</returns>
    public bool ChunkIsLoading(Vector3 chunkPosition)
    {
        return _loadingChunks.ContainsKey(chunkPosition);
    }

    /// <summary>
    /// Load the data of the chunk at the specified position.
    /// </summary>
    /// <param name="chunkPosition">The world position of the chunk to load data for.</param>
    /// <remarks>
    /// If chunk data for the specified position is already loaded it is overwritten. If the chunk is mid-load then an
    /// early return occurs and no new data is loaded. Currently this just generates the chunk, no data is stored on disk.
    /// </remarks>
    public void LoadChunk(Vector3 chunkPosition)
    {
        var alreadyLoading = !_loadingChunks.TryAdd(chunkPosition, 0);
        if (alreadyLoading) return;

        Task.Run(() =>
        {
            var chunk = _generator.GenerateChunk(chunkPosition, ChunkDimensions, this);
            if (ChunkIsLoaded(chunkPosition))
            {
                var oldChunk = _chunks[chunkPosition];
                oldChunk.UnloadModel();
                _chunks[chunkPosition] = chunk;
            }
            else _chunks.TryAdd(chunkPosition, chunk);

            _loadingChunks.Remove(chunkPosition, out _);
        });
    }

    /// <summary>
    /// Unload the data of the chunk at the specified position.
    /// </summary>
    /// <param name="chunkPosition">The world position of the chunk to load data for.</param>
    /// <returns><c>true</c> if the chunk is successfully found and removed; otherwise, <c>false</c>.</returns>
    public bool UnloadChunk(Vector3 chunkPosition)
    {
        var result = _chunks.Remove(chunkPosition, out var chunk);
        chunk?.UnloadModel();
        return result;
    }

    /// <summary>
    /// Unload the model (including meshes) of the chunk at the specified position from memory (RAM and/or VRAM).
    /// </summary>
    /// <param name="chunkPosition">The world position of the chunk to load data for.</param>
    /// <returns><c>true</c> if the chunk model is successfully found and unloaded; otherwise, <c>false</c>.</returns>
    public bool UnloadChunkModel(Vector3 chunkPosition)
    {
        var chunk = GetChunk(chunkPosition);
        if (chunk == null) return false;
        chunk.UnloadModel();
        return true;
    }

    /// <summary>
    /// Get the chunk at the specified position.
    /// </summary>
    /// <param name="chunkPosition">The world position of the chunk to load data for.</param>
    /// <returns>The loaded <c>Chunk</c> if successfully found; otherwise, <c>null</c>.</returns>
    public Chunk? GetChunk(Vector3 chunkPosition)
    {
        return _chunks.ContainsKey(chunkPosition) ? _chunks[chunkPosition] : null;
    }

    public Block GetBlock(Vector3 blockWorldPosition)
    {
        var chunkPos = new Vector3(
            MathF.Floor(blockWorldPosition.X / ChunkDimensions.X),
            MathF.Floor(blockWorldPosition.Y / ChunkDimensions.Y),
            MathF.Floor(blockWorldPosition.Z / ChunkDimensions.Z));
        var localPos = blockWorldPosition - chunkPos * ChunkDimensions;

        var chunk = GetChunk(chunkPos);
        if (chunk != null)
            return chunk.GetBlock(localPos);
        return _generator.GenerateBlock(blockWorldPosition);
    }
}