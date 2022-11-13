using ChompGame.Data;
using ChompGame.GameSystem;
using System;

namespace ChompGame.Graphics
{
    public class ScanlineSpritePixelPriority
    {
        private readonly Specs _specs;
        private readonly BitArray _pixelPriorities;

        public ScanlineSpritePixelPriority(SystemMemoryBuilder memoryBuilder, Specs specs)
        {
            _specs = specs;
            _pixelPriorities = new BitArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);

            //declares enough space for 2-tile wide sprites. might be a way to be more efficient?
            int bitsNeeded = specs.TileWidth * specs.SpritesPerScanline * 2;
            int bytesNeeded = (int)Math.Ceiling(bitsNeeded / 8.0);
            memoryBuilder.AddBytes(bytesNeeded);
        }

        public bool Get(int spriteIndex, int column)
        {
            return _pixelPriorities[(spriteIndex * _specs.TileWidth * 2) + column];
        }

        public void Set(int spriteIndex, int column, bool value)
        {
            _pixelPriorities[(spriteIndex * _specs.TileWidth * 2) + column] = value;
        }


    }
}
