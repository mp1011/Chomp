using ChompGame.Extensions;
using ChompGame.GameSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.Data
{
    public class SystemMemory
    {
        private byte[] _memory;
        private byte[] _corruptionMask;

        private int _romStartAddress;
        private bool _enableSetRom = true;

        public void CorruptBit(int bit)
        {
            var index = bit / 8;
            var b = 2.Power(bit % 8);
            b = ~b;

            _corruptionMask[index] = (byte)(_corruptionMask[index] & b);
        }

        public int RAMSize => _romStartAddress;

        public byte this[int index] 
        {
            get
            {
                // return (byte)(_memory[index] & _corruptionMask[index]);
                if (index >= _memory.Length)
                    index = _memory.Length - 1;
                return _memory[index];
            }
            set
            {
                //if (!_enableSetRom && _romStartAddress != -1 && index >= _romStartAddress)
                //    throw new Exception("Memory Violation");
                _memory[index] = value;
            }
        }

        public void BlockCopy(int sourceStart, int destinationStart, int length)
        {
            Array.Copy(sourceArray: _memory,
                sourceIndex: sourceStart,
                destinationArray: _memory,
                destinationIndex: destinationStart,
                length: length);
        }

        public SystemMemory(Action<SystemMemoryBuilder> configureMemory, Specs specs)
        {
            var memoryBuilder = new SystemMemoryBuilder(this, specs);
            configureMemory(memoryBuilder);
            _memory = memoryBuilder.Build();
            _corruptionMask = Enumerable.Repeat((byte)255, _memory.Length).ToArray();

            _romStartAddress = memoryBuilder.ROMStartAddress;
        }

        public void Ready()
        {
            _enableSetRom = false;
        }
    }

    public class SystemMemoryBuilder
    {
        private List<byte> _bytes = new List<byte>();
        private SystemMemory _systemMemory;
        private Specs _specs;

        public int ROMStartAddress { get; private set; } = -1;
        public int CurrentAddress => _bytes.Count;

        public SystemMemory Memory => _systemMemory;

        public SystemMemoryBuilder(SystemMemory systemMemory, Specs specs)
        {
            _specs = specs;
            _systemMemory = systemMemory;
        }

        public void BeginROM()
        {
            if (ROMStartAddress != -1)
                throw new Exception("ROM already started");

            ROMStartAddress = CurrentAddress;
        }

        public byte[] Build()
        {
            return _bytes.ToArray();
        }

        public GameByte[] AddBytes(int count)
        {
            return Enumerable.Range(0, count)
                .Select(p => AddByte(0))
                .ToArray();
        }

        public BitPlane AddBitPlane(int width, int height)
        {
            var bitPlane = new BitPlane(CurrentAddress, _systemMemory, width, height);
            AddBytes(bitPlane.Bytes);
            return bitPlane;
        }

        public NBitPlane AddNBitPlane(int planes, int width, int height)
        {
            var bitPlane = NBitPlane.Create(CurrentAddress, _systemMemory, planes, width, height);
            AddBytes(bitPlane.Bytes);
            return bitPlane;
        }


        public ByteAddressWithFPad AddByteAddressWithFPad()
        {
            return new ByteAddressWithFPad(AddByte(), _systemMemory);
        }

        public GameByte AddByte(byte value=0)
        {
            var b = new GameByte(CurrentAddress, _systemMemory);
            _bytes.Add(value);
            return b;
        }

        public NibblePoint AddNibblePoint()
        {
            var n = new NibblePoint(CurrentAddress, Memory);
            AddByte();
            return n;
        }

        public GameShort AddShort()
        {
            var s = new GameShort(CurrentAddress, _systemMemory);
            _bytes.Add(0);
            _bytes.Add(0);
            return s;
        }

        public GameByteGridPoint AddGridPoint(byte width, byte height)
        {
            var b = new GameByteGridPoint(
                AddByte(),
                AddByte(),
                width,
                height);

            return b;
        }

        public BitArray AddBitArray(int length)
        {
            var bitArray = new BitArray(CurrentAddress, _systemMemory);
            var bytes = (int)(Math.Ceiling((float)length / 8));
            AddBytes(bytes);
            return bitArray;
        }

        public TwoBitArray AddTwoBitArray(int length)
        {
            return new TwoBitArray(AddBitArray(length), AddBitArray(length));
        }

        public FullGameByteGridPoint AddFullGridPoint() =>
            new FullGameByteGridPoint(AddByte(), AddByte());

        public GameByteGridPoint AddGridPoint(byte width, byte height, Bit mask)
            => AddGridPoint(width, height, mask, mask);

        public GameByteGridPoint AddGridPoint(byte width, byte height, Bit xMask, Bit yMask)
        {
            var b = new GameByteGridPoint(
                AddByte().WithMask(xMask), 
                AddByte().WithMask(yMask), 
                width, 
                height);

            return b;
        }

        public Sprite[] AddSprite(int count, SpritesModule spritesModule)
        {
            return Enumerable.Range(0, count)
                .Select(p =>
                {
                    var addr = CurrentAddress;
                    AddBytes(_specs.BytesPerSprite);
                    return new Sprite(addr, Memory, _specs, spritesModule.Scroll);
                })
                .ToArray();
        }
    }
}
