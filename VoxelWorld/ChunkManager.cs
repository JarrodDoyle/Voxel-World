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
    private readonly HashSet<Vector3> _chunksToUnload;

    public ChunkManager(World world)
    {
        _world = world;
        _loadPosition = Vector3.Zero;
        _loadRadius = -1;
        _viewFrustum = new Frustum();
        _chunksToUnload = new HashSet<Vector3>();
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
            _chunksToUnload.Remove(chunkPos);
            if (!_world.ChunkIsLoaded(chunkPos) && !_world.ChunkIsLoading(chunkPos))
                chunksToLoad.Add(chunkPos);
        }

        // Sort by distance to loading position
        chunksToLoad = chunksToLoad.OrderBy(chunkPos => (chunkPos - position).LengthSquared()).ToList();
        foreach (var chunkPos in chunksToLoad)
            _world.LoadChunk(chunkPos);

        // Unload any chunks outside of the radius
        foreach (var chunkPos in _world.GetLoadedChunkPositions())
            if (!PointInRadius(position, radius, chunkPos))
                _world.UnloadChunk(chunkPos);

        foreach (var chunkPos in _world.GetLoadingChunkPositions())
            if (!PointInRadius(position, radius, chunkPos))
                _chunksToUnload.Add(chunkPos);
    }

    private bool PointInRadius(Vector3 center, int radius, Vector3 point)
    {
        return center.X - radius <= point.X && point.X <= center.X + radius &&
               center.Y - radius <= point.Y && point.Y <= center.Y + radius &&
               center.Z - radius <= point.Z && point.Z <= center.Z + radius;
    }

    public void Update()
    {
        var removedChunks = new List<Vector3>();
        foreach (var chunkPos in _chunksToUnload)
        {
            if (_world.ChunkIsLoaded(chunkPos) && !_world.ChunkIsLoading(chunkPos))
            {
                _world.UnloadChunk(chunkPos);
                removedChunks.Add(chunkPos);
            }
        }

        foreach (var chunkPos in removedChunks)
            _chunksToUnload.Remove(chunkPos);
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