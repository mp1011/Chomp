using ChompGame.Extensions;
using System;
using System.Linq;

namespace ChompGame.Data
{
    public class BitPlane : IGrid<bool>
    {
        private int _address;
        
        public int Width { get; }
        public int Height { get; }

        public int Bytes => (Width * Height) / 8;

        private SystemMemory _memory;

        public BitPlane(int address, SystemMemory memory, int width, int height)
        {
            _address = address;
            _memory = memory;
            Width = width;
            Height = height;
        }

        public bool this[int index]
        {
            get
            {
                int bitIndex = index % 8;
                int offset = (int)(index / 8);

                var gameBit = new GameBit(_address + offset, bitIndex.BitFromIndex(), _memory);
                return gameBit.Value;
            }
            set
            {
                int bitIndex = index % 8;
                int offset = (int)(index / 8);

                var gameBit = new GameBit(_address + offset, bitIndex.BitFromIndex(), _memory);
                gameBit.Value = value;
            }
        }

        public bool this[int x, int y]
        {
            get => this[(y * Width) + x];
            set => this[(y * Width) + x] = value;
        }

        public bool ValueFromChar(char s)
        {
            return s == '1';
        }
    }
}
