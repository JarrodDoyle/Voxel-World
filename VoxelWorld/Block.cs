using System.Numerics;

namespace VoxelWorld;

public class Block
{
    public Vector3 Position { get; }
    public BlockType BlockType { get; }
    public Vector4 Color { get; }

    public Block(Vector3 position, BlockType blockType, Vector4 color)
    {
        Position = position;
        BlockType = blockType;
        Color = color;
    }
}