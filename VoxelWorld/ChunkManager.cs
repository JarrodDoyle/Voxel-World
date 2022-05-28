using System.Numerics;
using Raylib_cs;
using Utils;

namespace VoxelWorld;

public class ChunkManager
{
    private World _world;
    private Vector3 _loadPosition;
    private int _loadRadius;
    private List<Vector3> _loadedChunks;
    private List<Vector3> _chunksToLoad;
    private Frustum _viewFrustum;


    public ChunkManager(World world)
    {
        _world = world;
        _loadPosition = Vector3.Zero;
        _loadRadius = -1;
        _loadedChunks = new List<Vector3>();
        _chunksToLoad = new List<Vector3>();
        _viewFrustum = new Frustum();
    }

    public void LoadChunksAroundPosition(Vector3 position, int radius)
    {
        if (position == _loadPosition && radius == _loadRadius) return;
        _loadPosition = position;
        _loadRadius = radius;

        // Build list of chunks that need loading
        _chunksToLoad.Clear();
        for (int x = -radius; x <= radius; x++)
        for (int y = -radius; y <= radius; y++)
        for (int z = -radius; z <= radius; z++)
        {
            var chunkPos = position + new Vector3(x, y, z);
            if (_world.ChunkIsLoaded(chunkPos))
            {
                _loadedChunks.Add(chunkPos);
                continue;
            }

            _chunksToLoad.Add(chunkPos);
        }
        
        // Sort by distance to loading position
        _chunksToLoad = _chunksToLoad.OrderBy(chunkPos => (chunkPos - position).LengthSquared()).ToList();

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
        var chunkPosArr = _chunksToLoad.ToArray();
        var idxToRemove = -1;
        for (int i = 0; i < chunkPosArr.Length; i++)
        {
            var chunkPos = chunkPosArr[i];
            if (!ChunkInFrustum(chunkPos)) continue;

            _world.LoadChunk(chunkPos);
            _loadedChunks.Add(chunkPos);
            idxToRemove = i;
            break;
        }

        if (idxToRemove != -1) _chunksToLoad.RemoveAt(idxToRemove);
    }

    public void Render()
    {
        _viewFrustum = new Frustum();
        foreach (var chunkPos in _loadedChunks)
        {
            if (!ChunkInFrustum(chunkPos)) continue;
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