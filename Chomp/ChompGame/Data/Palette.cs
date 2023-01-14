using ChompGame.GameSystem;
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

        public Color this[byte index]
        {
            get
            {
                return _specs.SystemColors[GetColorIndex(index)];
            }
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
