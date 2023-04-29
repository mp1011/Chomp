using ChompGame.GameSystem;
using ChompGame.Graphics;
using Microsoft.Xna.Framework;

namespace ChompGame.Data
{
    public class Palette : IMemoryBlock
    {
        private Specs _specs;
        private NibbleArray _colorsLow;
        private DenseTwoBitArray _colorsHi;

        public int Address => _colorsLow.Address;
        public int Bytes => _specs.BytesPerPalette;

        public Palette(Specs specs, NibbleArray colorsLow, DenseTwoBitArray colorsHi)
        {
            _specs = specs;
            _colorsLow = colorsLow;
            _colorsHi = colorsHi;
        }

        public Palette(Specs specs, int address, SystemMemory systemMemory)
        {
            _specs = specs;
            _colorsLow = new NibbleArray(address, systemMemory);
            _colorsHi = new DenseTwoBitArray(address+2, systemMemory); //todo, avoid hard coding palette info 
        }

        public Color GetColor(byte index, byte fade)
        {
            var colorIndex = new ColorIndex(GetColorIndex(index));
            while (fade-- > 0)
                colorIndex = colorIndex.Darker();

            return _specs.SystemColors[colorIndex.Value];
        }

        public int GetColorIndex(int index)
        {
            var low = _colorsLow[index];
            var hi = _colorsHi[index] << 4;
            return low + hi;
        }

        public void SetColor(int paletteIndex, byte systemColorIndex)
        {
            _colorsLow[paletteIndex] =  (byte)(systemColorIndex & (byte)Bit.Right4);
            _colorsHi[paletteIndex] = (byte)((systemColorIndex & 48) >> 4);
        }

    }
}
