using System.Numerics;
using Raylib_cs;
using Utils;

namespace VoxelWorld;

public class ChunkManager
{
    public int LoadRadius;
    private readonly World _world;
    private Vector3 _loadedPosition;
    private int _loadedRadius;
    private readonly Frustum _viewFrustum;
    private readonly HashSet<Vector3> _chunksToUnload;

    public ChunkManager(World world)
    {
        _world = world;
        _loadedPosition = Vector3.Zero;
        LoadRadius = 1;
        _viewFrustum = new Frustum();
        _chunksToUnload = new HashSet<Vector3>();
    }

    public void LoadChunksAroundPosition(Vector3 position)
    {
        if (position == _loadedPosition && LoadRadius == _loadedRadius) return;
        _loadedPosition = position;
        _loadedRadius = LoadRadius;

        // Build list of chunks that need loading
        var chunksToLoad = new List<Vector3>();
        for (var x = -LoadRadius; x <= LoadRadius; x++)
        for (var y = -LoadRadius; y <= LoadRadius; y++)
        for (var z = -LoadRadius; z <= LoadRadius; z++)
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
            if (!PointInRadius(position, LoadRadius, chunkPos))
                _world.UnloadChunk(chunkPos);

        foreach (var chunkPos in _world.GetLoadingChunkPositions())
            if (!PointInRadius(position, LoadRadius, chunkPos))
                _chunksToUnload.Add(chunkPos);
    }

    private bool PointInRadius(Vector3 center, int radius, Vector3 point)
    {
        return center.X - radius <= point.X && point.X <= center.X + radius &&
               center.Y - radius <= point.Y && point.Y <= center.Y + radius &&
               center.Z - radius <= point.Z && point.Z <= center.Z + radius;
    }

    public void Update(Vector3 cameraPosition)
    {
        var chunkDims = _world.ChunkDimensions;
        var cameraChunkPos = new Vector3(
            MathF.Floor(cameraPosition.X / chunkDims.X),
            MathF.Floor(cameraPosition.Y / chunkDims.Y),
            MathF.Floor(cameraPosition.Z / chunkDims.Z)
        );
        LoadChunksAroundPosition(cameraChunkPos);
        
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

    public void RebuildMeshes()
    {
        foreach (var chunkPosition in _world.GetLoadedChunkPositions())
        {
            _world.UnloadChunkModel(chunkPosition);
        }
    }
}