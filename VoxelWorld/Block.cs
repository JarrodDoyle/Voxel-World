using System.Numerics;

namespace VoxelWorld;

public class Block
{
    private readonly uint _color;

    public BlockType BlockType { get; }
    public Vector4 Color => new((_color >> 24) & 0xFF, (_color >> 16) & 0xFF, (_color >> 8) & 0xFF, _color & 0xFF);

    public Block(BlockType blockType, Vector4 color)
    {
        BlockType = blockType;
        _color = ((uint) color.X << 24) + ((uint) color.Y << 16) + ((uint) color.Z << 8) + (uint) color.W;
    }
}