using ChompGame.Extensions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ChompGame.Data
{
    public class NBitPlane : IGrid<byte>
    {
        private BitPlane[] _planes;

        public int Address { get; }

        public NBitPlane(int address, SystemMemory memory, int planeCount, int width, int height)
        {
            Address = address;
            Width = width;
            Height = height;

            List<BitPlane> planes = new List<BitPlane>();
            while(planeCount-- > 0)
            {
                var plane = new BitPlane(address, memory, width, height);
                planes.Add(plane);
                address += plane.Bytes;
            }

            _planes = planes.ToArray();
            Bytes = _planes.Sum(p => p.Bytes);
        }

        public byte this[int index]
        {
            get
            {
                int value = 0;
                for(int i=0; i< _planes.Length;i++)
                {
                    var planeValue = _planes[i][index] ? 2.Power(i) : 0;
                    value += planeValue;
                }

                return (byte)value;
            }
            set
            {
                for (int i = 0; i < _planes.Length; i++)
                {
                    var planeValue = (value & 2.Power(i)) > 0;
                    _planes[i][index] = planeValue;
                }
            }
        }
        
        public byte this[int x, int y]
        {
            get => this[(y * Width) + x];
            set => this[(y * Width) + x] = value;
        }
        public int Width { get; }

        public int Height { get; }

        public int Bytes { get; }

        public byte ValueFromChar(char s)
        {
            var value = byte.Parse(s.ToString(), NumberStyles.HexNumber);
            return value;
        }

        public void CopyTo(NBitPlane destination, SystemMemory memory)
        {
            var totalLength = _planes[0].Bytes * _planes.Length;
            memory.BlockCopy(sourceStart: Address, destinationStart: destination.Address, length: totalLength);
        }
    }
}
