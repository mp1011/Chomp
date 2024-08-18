using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame;
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
                case 1:
                    return new OneBitPlane(address, memory, width, height);
                case 2:
                    return new TwoBitPlane(address, memory, width, height);
                case 4:
                    return new FourBitPlane(address, memory, width, height);
                case 5:
                    return new FiveBitPlane(address, memory, width, height);
                case 6:
                    return new SixBitPlane(address, memory, width, height);

                case 8:
                    return new BytePlane(address, memory, width, height);
                default:
                    throw new Exception("Invalid plane count");
            }
        }

        public void Reset()
        {
            foreach (var plane in _planes)
                plane.Reset();
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
            set
            {
                this[(y * Width) + x] = value;
            }
        }


        public byte GetValueOrDefault(int x, int y)
        {
            if (x < 0 || x >= Width)
                return 0;
            if (y < 0 || y >= Height)
                return 0;
            else
                return this[x, y];
        }

        public int Width { get; }

        public int Height { get; }

        public int Bytes { get; }

        public byte ValueFromChar(char s, int offset)
        {
            var value = s.ParseByteSpecial();
            return (byte)(value + offset);
        }

        public void CopyTo(NBitPlane destination, SystemMemory memory)
        {
            var totalLength = _planes[0].Bytes * _planes.Length;
            memory.BlockCopy(sourceStart: Address, destinationStart: destination.Address, length: totalLength);
        }

        public void CopyTo(
           NBitPlane destination,
           ByteRectangleBase source,
           Point destinationPoint,
           Specs specs,
           SystemMemory memory)
        {
            int planeIndex = 0;
            foreach (var bitPlane in _planes)
            {
                bitPlane.CopyTo(destination._planes[planeIndex], source, destinationPoint, specs);
                planeIndex++;
            }
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
                bitPlane.CopyTilesTo(destination._planes[planeIndex], source, destinationPoint, specs);
                planeIndex++;
            }
        }
    }

    public class OneBitPlane : NBitPlane
    {
        public OneBitPlane(int address, SystemMemory memory, int width, int height) 
            : base(address, memory, 1, width, height)
        {
        }

        public override byte this[int index]
        {
            get
            {
                var p0 = _planes[0][index] ? 1 : 0;
                return (byte)(p0);
            }
            set
            {
                _planes[0][index] = (value & 1) > 0;
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

    public class FiveBitPlane : NBitPlane
    {
        public FiveBitPlane(int address, SystemMemory memory, int width, int height)
            : base(address, memory, 5, width, height)
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
                var p4 = _planes[4][index] ? 16 : 0;

                return (byte)(p0 + p1 + p2 + p3 + p4);
            }
            set
            {
                _planes[0][index] = (value & 1) > 0;
                _planes[1][index] = (value & 2) > 0;
                _planes[2][index] = (value & 4) > 0;
                _planes[3][index] = (value & 8) > 0;
                _planes[4][index] = (value & 16) > 0;
            }
        }
    }

    public class SixBitPlane : NBitPlane
    {
        public SixBitPlane(int address, SystemMemory memory, int width, int height)
            : base(address, memory, 6, width, height)
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
                var p4 = _planes[4][index] ? 16 : 0;
                var p5 = _planes[5][index] ? 32 : 0;

                return (byte)(p0 + p1 + p2 + p3 + p4 + p5);
            }
            set
            {
                _planes[0][index] = (value & 1) > 0;
                _planes[1][index] = (value & 2) > 0;
                _planes[2][index] = (value & 4) > 0;
                _planes[3][index] = (value & 8) > 0;
                _planes[4][index] = (value & 16) > 0;
                _planes[5][index] = (value & 32) > 0;
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
