using ChompGame.GameSystem;
using Microsoft.Xna.Framework;

namespace ChompGame.Data
{
    public class Palette
    {
        private Specs _specs;
        private NibbleArray _colors;

        public int Address => _colors.Address;

        public Palette(Specs specs, NibbleArray colors)
        {
            _specs = specs;
            _colors = colors;
        }

        public Palette(Specs specs, int address, SystemMemory systemMemory)
        {
            _specs = specs;
            _colors = new NibbleArray(address, systemMemory);
        }

        public Color this[byte index]
        {
            get
            {
                var c = _colors[index];
                return _specs.SystemColors[c];
            }
        }

        public byte GetColorIndex(byte index) => _colors[index];

        public void SetColor(int paletteIndex, byte systemColorIndex)
        {
            _colors[paletteIndex] = systemColorIndex;
        }

    }
}
