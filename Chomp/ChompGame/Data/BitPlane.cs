using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
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

        public void CopyTo(
            BitPlane destination,
            ByteRectangleBase source,
            Point destinationPoint,
            Specs specs)
        {           
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    destination[destinationPoint.X + x, destinationPoint.Y + y] =
                        this[source.X + x, source.Y + y];
                }
            }
        }

        public void CopyTilesTo(
            BitPlane destination,
            ByteRectangleBase source,
            Point destinationPoint,
            Specs specs)
        {
            var destinationPixelPoint = new Point(
                destinationPoint.X * specs.TileWidth,
                destinationPoint.Y * specs.TileHeight);

            var sourcePixelPoint = new Point(
                source.X * specs.TileWidth,
                source.Y * specs.TileHeight);

            //todo, probably could be made more efficient
            for (int y = 0; y < source.Height * specs.TileHeight; y++)
            {
                for(int x = 0; x < source.Width * specs.TileWidth; x++)
                {
                    destination[destinationPixelPoint.X + x, destinationPixelPoint.Y + y] =
                        this[sourcePixelPoint.X + x, sourcePixelPoint.Y + y];
                }
            }
        }
    }
}
