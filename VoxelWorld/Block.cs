using System.Numerics;

namespace VoxelWorld;

public class Block
{
    private readonly uint _position;
    private readonly uint _color;

    public Vector3 Position => new((_position >> 16) & 0xFF, (_position >> 8) & 0xFF, _position & 0xFF);
    public BlockType BlockType { get; }
    public Vector4 Color => new((_color >> 24) & 0xFF, (_color >> 16) & 0xFF, (_color >> 8) & 0xFF, _color & 0xFF);

    public Block(Vector3 position, BlockType blockType, Vector4 color)
    {
        _position = ((uint) position.X << 16) + ((uint) position.Y << 8) + (uint) position.Z;
        BlockType = blockType;
        _color = ((uint) color.X << 24) + ((uint) color.Y << 16) + ((uint) color.Z << 8) + (uint) color.W;
    }
}