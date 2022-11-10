using ChompGame.GameSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.Data
{
    public abstract class MemoryBlock
    {
        public abstract byte this[int index] { get; set; }
        public abstract void BlockCopy(int sourceStart, int destinationStart, int length);
    }

    public class FixedMemoryBlock : MemoryBlock
    {
        private byte[] _memory;

        public FixedMemoryBlock(byte[] memory)
        {
            _memory = memory;
        }

        public override byte this[int index]
        {
            get
            {
                if (index >= _memory.Length)
                    index = _memory.Length - 1;
                return _memory[index];
            }
            set
            {
                _memory[index] = value;
            }
        }

        public override void BlockCopy(int sourceStart, int destinationStart, int length)
        {
            Array.Copy(sourceArray: _memory,
                sourceIndex: sourceStart,
                destinationArray: _memory,
                destinationIndex: destinationStart,
                length: length);
        }
    }

    public class DynamicMemoryBlock : MemoryBlock
    {
        private List<byte> _memory = new List<byte>();

        public DynamicMemoryBlock()
        {
        }

        public int Count => _memory.Count;

        public override byte this[int index]
        {
            get => _memory[index];
            set => _memory[index] = value;
        }

        public override void BlockCopy(int sourceStart, int destinationStart, int length)
        {
            throw new NotSupportedException();
        }

        public void Add(byte value)
        {
            _memory.Add(value);
        }

        public FixedMemoryBlock ToFixed() => new FixedMemoryBlock(_memory.ToArray());
    }

    public class SystemMemory
    {
        public SystemMemoryBuilder CurrentBuilder { get; set; }
        private MemoryBlock _memory;
        private int _romStartAddress;

        public int RAMSize => _romStartAddress;

        public byte this[int index] 
        {
            get => _memory[index];
            set => _memory[index] = value;
        }

        public void BlockCopy(int sourceStart, int destinationStart, int length)
        {
            _memory.BlockCopy(sourceStart, destinationStart, length);
        }

        public SystemMemory(Action<SystemMemoryBuilder> configureMemory, Specs specs)
        {
            var memoryBuilder = new SystemMemoryBuilder(this, specs);

            _memory = memoryBuilder.Bytes;
            configureMemory(memoryBuilder);
            _memory = memoryBuilder.Build();

            _romStartAddress = memoryBuilder.ROMStartAddress;
        }

        public void Ready()
        {
        }
    }

    public class SystemMemoryBuilder
    {
        public DynamicMemoryBlock Bytes { get; } = new DynamicMemoryBlock();

        private SystemMemory _systemMemory;
        private Specs _specs;

        public int ROMStartAddress { get; private set; } = -1;
        public int CurrentAddress => Bytes.Count;

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

        public FixedMemoryBlock Build()
        {
            return Bytes.ToFixed();
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
            Bytes.Add(value);
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
            Bytes.Add(0);
            Bytes.Add(0);
            return s;
        }

        public GameInteger AddInteger()
        {
            var s = new GameInteger(CurrentAddress, _systemMemory);
            Bytes.Add(0);
            Bytes.Add(0);
            Bytes.Add(0);
            Bytes.Add(0);
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
