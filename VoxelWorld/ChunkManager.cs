using System.Numerics;
using Raylib_cs;
using Utils;

namespace VoxelWorld;

public class ChunkManager
{
    private readonly World _world;
    private Vector3 _loadPosition;
    private int _loadRadius;
    private readonly Frustum _viewFrustum;


    public ChunkManager(World world)
    {
        _world = world;
        _loadPosition = Vector3.Zero;
        _loadRadius = -1;
        _viewFrustum = new Frustum();
    }

    public void LoadChunksAroundPosition(Vector3 position, int radius)
    {
        if (position == _loadPosition && radius == _loadRadius) return;
        _loadPosition = position;
        _loadRadius = radius;

        // Build list of chunks that need loading
        var chunksToLoad = new List<Vector3>();
        for (var x = -radius; x <= radius; x++)
        for (var y = -radius; y <= radius; y++)
        for (var z = -radius; z <= radius; z++)
        {
            var chunkPos = position + new Vector3(x, y, z);
            if (!_world.ChunkIsLoaded(chunkPos) && !_world.ChunkIsLoading(chunkPos))
                chunksToLoad.Add(chunkPos);
        }

        // Sort by distance to loading position
        chunksToLoad = chunksToLoad.OrderBy(chunkPos => (chunkPos - position).LengthSquared()).ToList();
        foreach (var chunkPos in chunksToLoad)
            _world.LoadChunk(chunkPos);

        // Unload any chunks outside of the radius
        // TODO: This doesn't unload chunks that are currently loading!
        foreach (var chunkPos in _world.GetLoadedChunkPositions())
        {
            if (chunkPos.X < position.X - radius || chunkPos.X > position.X + radius ||
                chunkPos.Y < position.Y - radius || chunkPos.Y > position.Y + radius ||
                chunkPos.Z < position.Z - radius || chunkPos.Z > position.Z + radius)
            {
                _world.UnloadChunk(chunkPos);
            }
        }
    }

    public void Update()
    {
        var loaded = _world.GetLoadedChunkPositions().ToList().Count;
        var loading = _world.GetLoadingChunkPositions().ToList().Count;
        Console.WriteLine($"Loaded chunks: {loaded}/{loaded + loading} ({loaded}, {loading})");
    }

    public void Render()
    {
        _viewFrustum.UpdatePlanes();
        foreach (var chunkPos in _world.GetLoadedChunkPositions())
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