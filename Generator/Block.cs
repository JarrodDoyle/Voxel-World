using System.Numerics;

namespace Generator;

public class Block
{
    public Vector3 Position { get; }
    public BlockType BlockType { get; }

    public Block(Vector3 position, BlockType blockType)
    {
        Position = position;
        BlockType = blockType;
    }
}