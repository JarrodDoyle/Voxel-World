using System.Numerics;
using System.Runtime.InteropServices;

namespace VoxelWorld;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Block
{
    private readonly uint _color;
    private readonly byte _blockType;

    public BlockType BlockType => (BlockType) _blockType;
    public Vector4 Color => new((_color >> 24) & 0xFF, (_color >> 16) & 0xFF, (_color >> 8) & 0xFF, _color & 0xFF);

    public Block(BlockType blockType, Vector4 color)
    {
        _blockType = (byte) blockType;
        _color = ((uint) color.X << 24) + ((uint) color.Y << 16) + ((uint) color.Z << 8) + (uint) color.W;
    }
}