using System.Collections;

namespace VoxelWorld;

internal struct PaletteEntry
{
    public short RefCount = 1;
    public Block Block;

    public PaletteEntry(Block block = default)
    {
        Block = block;
    }
}

public class VoxelStorage
{
    private PaletteEntry[] _palette;
    private BitArray _data;
    private int _numVoxels;
    private int _indexLength;

    public VoxelStorage(int size)
    {
        _numVoxels = size;
        _indexLength = 1;
        _palette = new PaletteEntry[(int) Math.Pow(2, _indexLength)];
        _data = new BitArray(_numVoxels * _indexLength);
    }

    public void SetBlock(int blockIndex, Block block)
    {
        var paletteIndex = GetPaletteIndex(blockIndex);
        var currentEntry = _palette[paletteIndex];

        // We're removing the block currently there
        currentEntry.RefCount--;

        // If the block is already in the palette we can use it's existing palette entry
        var replace = Array.FindIndex(_palette, entry => entry.Block.BlockType == block.BlockType);
        if (replace != -1)
        {
            SetPaletteIndex(blockIndex, replace);
            _palette[replace].RefCount++;
            return;
        }
        
        // Else if the currentEntry can be replaced, replace it
        if (currentEntry.RefCount == 0)
        {
            currentEntry.Block = block;
            currentEntry.RefCount = 1;
            return;
        }

        // Else we need to make a new entry!
        var newPaletteIndex = NewPaletteEntry();
        _palette[newPaletteIndex] = new PaletteEntry(block);
        SetPaletteIndex(blockIndex, newPaletteIndex);
        // paletteCount++;
    }

    public Block GetBlock(int blockIndex)
    {
        var paletteIndex = GetPaletteIndex(blockIndex);
        return _palette[paletteIndex].Block;
    }

    private int NewPaletteEntry()
    {
        var firstFree = Array.FindIndex(_palette, entry => entry.RefCount == 0);
        if (firstFree != -1)
            return firstFree;
        
        // If there's no free spots in the palette we have to grow it and then try again
        GrowPalette();
        return NewPaletteEntry();
    }

    private void GrowPalette()
    {
        // Store all the current indices
        var indices = new int[_numVoxels];
        for (var i = 0; i < _numVoxels; i++)
            indices[i] = GetPaletteIndex(i);
        
        // Create the new palette
        // We might as well double it in size, because adding 1 more indexing bit doubles the indexing range
        _indexLength++;
        var newPalette = new PaletteEntry[(int) Math.Pow(2, _indexLength)];
        Array.Copy(_palette, newPalette, _palette.Length);
        _palette = newPalette;
        
        // Create the new bitarray and copy current indices into it
        _data = new BitArray(_numVoxels * _indexLength);
        for (var i = 0; i < _numVoxels; i++)
            SetPaletteIndex(i, indices[i]);
    }

    public void ShrinkPalette()
    {
        // TODO:
    }

    private int GetPaletteIndex(int blockIndex)
    {
        var paletteIndex = 0;
        var dataIndex = blockIndex * _indexLength;
        for (var i = 0; i < _indexLength; i++)
        {
            if (_data[dataIndex + i])
                paletteIndex += (int) Math.Pow(2, _indexLength - i - 1);
        }

        return paletteIndex;
    }

    private void SetPaletteIndex(int blockIndex, int paletteIndex)
    {
        var dataIndex = blockIndex * _indexLength;
        for (var i = 0; i < _indexLength; i++)
            _data[dataIndex + i] = ((paletteIndex >> (_indexLength - i - 1)) & 0x1) == 1;
    }
}