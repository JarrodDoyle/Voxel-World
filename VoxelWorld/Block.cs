using System.Numerics;
using System.Runtime.InteropServices;

namespace VoxelWorld;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Block
{
    private readonly byte _color;
    private readonly byte _blockType;

    public BlockType BlockType => (BlockType) _blockType;
    public Vector4 Color => World.Palette[_color];

    public Block(BlockType blockType, int colorIndex)
    {
        _blockType = (byte) blockType;
        _color = (byte) colorIndex;
    }
}