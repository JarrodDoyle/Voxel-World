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

    public static Vector4[] Palette =
    {
        new(new Vector3(46, 34, 47), 255),
        new(new Vector3(62, 53, 70), 255),
        new(new Vector3(98, 85, 101), 255),
        new(new Vector3(150, 108, 108), 255),
        new(new Vector3(171, 148, 122), 255),
        new(new Vector3(105, 79, 98), 255),
        new(new Vector3(127, 112, 138), 255),
        new(new Vector3(155, 171, 178), 255),
        new(new Vector3(199, 220, 208), 255),
        new(new Vector3(255, 255, 255), 255),
        new(new Vector3(110, 39, 39), 255),
        new(new Vector3(179, 56, 49), 255),
        new(new Vector3(234, 79, 54), 255),
        new(new Vector3(245, 125, 74), 255),
        new(new Vector3(174, 35, 52), 255),
        new(new Vector3(232, 59, 59), 255),
        new(new Vector3(251, 107, 29), 255),
        new(new Vector3(247, 150, 23), 255),
        new(new Vector3(249, 194, 43), 255),
        new(new Vector3(122, 48, 69), 255),
        new(new Vector3(158, 69, 57), 255),
        new(new Vector3(205, 104, 61), 255),
        new(new Vector3(230, 144, 78), 255),
        new(new Vector3(251, 185, 84), 255),
        new(new Vector3(76, 62, 36), 255),
        new(new Vector3(103, 102, 51), 255),
        new(new Vector3(162, 169, 71), 255),
        new(new Vector3(213, 224, 75), 255),
        new(new Vector3(251, 255, 134), 255),
        new(new Vector3(22, 90, 76), 255),
        new(new Vector3(35, 144, 99), 255),
        new(new Vector3(30, 188, 115), 255),
        new(new Vector3(145, 219, 105), 255),
        new(new Vector3(205, 223, 108), 255),
        new(new Vector3(49, 54, 56), 255),
        new(new Vector3(55, 78, 74), 255),
        new(new Vector3(84, 126, 100), 255),
        new(new Vector3(146, 169, 132), 255),
        new(new Vector3(178, 186, 144), 255),
        new(new Vector3(11, 94, 101), 255),
        new(new Vector3(11, 138, 143), 255),
        new(new Vector3(14, 175, 155), 255),
        new(new Vector3(48, 225, 185), 255),
        new(new Vector3(143, 248, 226), 255),
        new(new Vector3(50, 51, 83), 255),
        new(new Vector3(72, 74, 119), 255),
        new(new Vector3(77, 101, 180), 255),
        new(new Vector3(77, 155, 230), 255),
        new(new Vector3(143, 211, 255), 255),
        new(new Vector3(69, 41, 63), 255),
        new(new Vector3(107, 62, 117), 255),
        new(new Vector3(144, 94, 169), 255),
        new(new Vector3(168, 132, 243), 255),
        new(new Vector3(234, 173, 237), 255),
        new(new Vector3(117, 60, 84), 255),
        new(new Vector3(162, 75, 111), 255),
        new(new Vector3(207, 101, 127), 255),
        new(new Vector3(237, 128, 153), 255),
        new(new Vector3(131, 28, 93), 255),
        new(new Vector3(195, 36, 84), 255),
        new(new Vector3(240, 79, 120), 255),
        new(new Vector3(246, 129, 129), 255),
        new(new Vector3(252, 167, 144), 255),
        new(new Vector3(253, 203, 176), 255)
    };

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