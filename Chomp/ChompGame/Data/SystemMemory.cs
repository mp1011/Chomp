using ChompGame.GameSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.Data
{
    public class SystemMemory
    {
        private byte[] _memory;

        public byte this[int index] 
        {
            get => (index >= _memory.Length) ? (byte)0 : _memory[index];
            set
            {   if (index < _memory.Length)                
                    _memory[index] = value;
            }
        }

        public SystemMemory(Action<SystemMemoryBuilder> configureMemory, Specs specs)
        {
            var memoryBuilder = new SystemMemoryBuilder(this, specs);
            configureMemory(memoryBuilder);
            _memory = memoryBuilder.Build();
        }
    }

    public class SystemMemoryBuilder
    {
        private List<byte> _bytes = new List<byte>();
        private SystemMemory _systemMemory;
        private Specs _specs;

        public int CurrentAddress => _bytes.Count;

        public SystemMemoryBuilder(SystemMemory systemMemory, Specs specs)
        {
            _specs = specs;
            _systemMemory = systemMemory;
        }

        public byte[] Build()
        {
            return _bytes.ToArray();
        }

        public void AddBytes(int count)
        {
            _bytes.AddRange(Enumerable.Repeat((byte)0, count));
        }

        public BitPlane AddBitPlane(int width, int height)
        {
            var bitPlane = new BitPlane(CurrentAddress, _systemMemory, width, height);
            AddBytes(bitPlane.Bytes);
            return bitPlane;
        }

        public NBitPlane AddNBitPlane(int planes, int width, int height)
        {
            var bitPlane = new NBitPlane(CurrentAddress, _systemMemory, planes, width, height);
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
            AddBytes(1);
            _bytes[CurrentAddress - 1] = value;
            return b;
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

        public Sprite[] AddSprite(int count)
        {
            return Enumerable.Range(0, count)
                .Select(p => new Sprite(AddGridPoint(0,0), AddByte(), _specs))
                .ToArray();
        }
    }
}
