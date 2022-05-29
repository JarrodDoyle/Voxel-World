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
    private readonly List<Vector3> _loadingChunks;

    public World(int seed, int worldType, Vector3 dimensions)
    {
        Seed = seed;
        ChunkDimensions = dimensions;
        _chunks = new ConcurrentDictionary<Vector3, Chunk>();
        _loadingChunks = new List<Vector3>();

        const float scale = 1 / 64f;
        _generator = worldType == 0 ? new Simplex2dWorld(seed, scale) : new Simplex3dWorld(seed, scale);
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
        return _loadingChunks.Contains(chunkPosition);
    }

    /// <summary>
    /// Load the data of the chunk at the specified position.
    /// </summary>
    /// <param name="chunkPosition">The world position of the chunk to load data for.</param>
    /// <remarks>
    /// If chunk data for the specified position is already loaded it is overwritten.
    /// Currently this just generates the chunk, no data is stored on disk.
    /// </remarks>
    public void LoadChunk(Vector3 chunkPosition)
    {
        // TODO: _loadingChunks doesn't end up empty oh no
        // TODO: Limit the number of active tasks? idk
        _loadingChunks.Add(chunkPosition);
        Task.Run(() =>
        {
            var chunk = _generator.GenerateChunk(chunkPosition, ChunkDimensions);
            if (ChunkIsLoaded(chunkPosition))
            {
                var oldChunk = _chunks[chunkPosition];
                oldChunk.UnloadModel();
                _chunks[chunkPosition] = chunk;
            }
            else _chunks.TryAdd(chunkPosition, chunk);

            _loadingChunks.Remove(chunkPosition);
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
        _chunks.TryGetValue(chunkPosition, out var chunk);
        return chunk;
    }
}