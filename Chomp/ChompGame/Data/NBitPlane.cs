using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ChompGame.Data
{
    public abstract class NBitPlane : IGrid<byte>
    {
        protected BitPlane[] _planes;

        public int Address { get; }

        public static NBitPlane Create(int address, SystemMemory memory, int planeCount, int width, int height)
        {
            switch (planeCount)
            {
                case 2:
                    return new TwoBitPlane(address, memory, width, height);
                case 4:
                    return new FourBitPlane(address, memory, width, height);
                case 8:
                    return new BytePlane(address, memory, width, height);
                default:
                    throw new Exception("Invalid plane count");
            }
        }

        protected NBitPlane(int address, SystemMemory memory, int planeCount, int width, int height)
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

        public virtual byte this[int index]
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

        public void CopyTilesTo(
            NBitPlane destination, 
            ByteRectangleBase source, 
            Point destinationPoint, 
            Specs specs,
            SystemMemory memory)
        {
            int planeIndex = 0;
            foreach(var bitPlane in _planes)
            {
                bitPlane.CopyTilesTo(destination._planes[planeIndex], source, destinationPoint, specs, memory);
                planeIndex++;
            }
        }
    }

    public class TwoBitPlane : NBitPlane
    {
        public TwoBitPlane(int address, SystemMemory memory, int width, int height) 
            : base(address, memory, 2, width, height)
        {
        }

        public override byte this[int index]
        {
            get
            {
                var p0 = _planes[0][index] ? 1 : 0;
                var p1 = _planes[1][index] ? 2 : 0;
                return (byte)(p0 + p1);
            }
            set
            {
                _planes[0][index] = (value & 1) > 0;
                _planes[1][index] = (value & 2) > 0;
            }
        }
    }

    public class FourBitPlane : NBitPlane
    {
        public FourBitPlane(int address, SystemMemory memory, int width, int height)
            : base(address, memory, 4, width, height)
        {
        }

        public override byte this[int index]
        {
            get
            {
                var p0 = _planes[0][index] ? 1 : 0;
                var p1 = _planes[1][index] ? 2 : 0;
                var p2 = _planes[2][index] ? 4 : 0;
                var p3 = _planes[3][index] ? 8 : 0;

                return (byte)(p0 + p1 + p2 + p3);
            }
            set
            {
                _planes[0][index] = (value & 1) > 0;
                _planes[1][index] = (value & 2) > 0;
                _planes[2][index] = (value & 4) > 0;
                _planes[3][index] = (value & 8) > 0;
            }
        }
    }

    public class BytePlane : NBitPlane
    {
        public BytePlane(int address, SystemMemory memory, int width, int height) 
            : base(address, memory, 8, width, height)
        {
        }
    }
}
