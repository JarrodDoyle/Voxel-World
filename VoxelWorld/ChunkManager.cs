using System.Numerics;
using Raylib_cs;
using Utils;

namespace VoxelWorld;

public class ChunkManager
{
    private readonly World _world;
    private Vector3 _loadPosition;
    private int _loadRadius;
    private readonly HashSet<Vector3> _loadedChunks;
    private readonly HashSet<Vector3> _loadingChunks;
    private readonly Frustum _viewFrustum;


    public ChunkManager(World world)
    {
        _world = world;
        _loadPosition = Vector3.Zero;
        _loadRadius = -1;
        _loadedChunks = new HashSet<Vector3>();
        _loadingChunks = new HashSet<Vector3>();
        _viewFrustum = new Frustum();
    }

    public void LoadChunksAroundPosition(Vector3 position, int radius)
    {
        if (position == _loadPosition && radius == _loadRadius) return;
        _loadPosition = position;
        _loadRadius = radius;

        // Build list of chunks that need loading
        var chunksToLoad = new List<Vector3>();
        for (int x = -radius; x <= radius; x++)
        for (int y = -radius; y <= radius; y++)
        for (int z = -radius; z <= radius; z++)
        {
            var chunkPos = position + new Vector3(x, y, z);
            var loaded = _world.ChunkIsLoaded(chunkPos);
            var loading = _world.ChunkIsLoading(chunkPos);

            if (loaded && !_loadedChunks.Contains(chunkPos))
                _loadedChunks.Add(chunkPos);

            if (loading && !_loadingChunks.Contains(chunkPos))
                _loadingChunks.Add(chunkPos);

            if (!loaded && !loading && !_loadingChunks.Contains(chunkPos))
            {
                chunksToLoad.Add(chunkPos);
                _loadingChunks.Add(chunkPos);
            }
        }

        // Sort by distance to loading position
        chunksToLoad = chunksToLoad.OrderBy(chunkPos => (chunkPos - position).LengthSquared()).ToList();
        foreach (var chunkPos in chunksToLoad)
            _world.LoadChunk(chunkPos);

        // Unload any chunks outside of the radius
        var chunksToUnload = new List<Vector3>();
        foreach (var chunkPos in _loadedChunks)
        {
            if (chunkPos.X < position.X - radius || chunkPos.X > position.X + radius ||
                chunkPos.Y < position.Y - radius || chunkPos.Y > position.Y + radius ||
                chunkPos.Z < position.Z - radius || chunkPos.Z > position.Z + radius)
            {
                _world.UnloadChunk(chunkPos);
                chunksToUnload.Add(chunkPos);
            }
        }

        foreach (var chunk in chunksToUnload) _loadedChunks.Remove(chunk);
    }

    public void Update()
    {
        var loadingChunksToRemove = new List<Vector3>();
        foreach (var chunkPos in _loadingChunks)
        {
            if (!_world.ChunkIsLoaded(chunkPos)) continue;

            loadingChunksToRemove.Add(chunkPos);
            _loadedChunks.Add(chunkPos);
        }

        foreach (var chunkPos in loadingChunksToRemove)
            _loadingChunks.Remove(chunkPos);
    }

    public void Render()
    {
        _viewFrustum.UpdatePlanes();
        foreach (var chunkPos in _loadedChunks)
        {
            if (ChunkInFrustum(chunkPos))
                _world.GetChunk(chunkPos)?.Render();
        }
    }

    private bool ChunkInFrustum(Vector3 chunkPos)
    {
        var min = chunkPos * _world.ChunkDimensions;
        var max = min + _world.ChunkDimensions;
        return _viewFrustum.AabbInside(new BoundingBox(min, max));
    }
}