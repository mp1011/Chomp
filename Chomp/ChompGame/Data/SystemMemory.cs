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
            get => _memory[index];
            set => _memory[index] = value;
        }

        public SystemMemory(Action<SystemMemoryBuilder> configureMemory)
        {
            var memoryBuilder = new SystemMemoryBuilder(this);
            configureMemory(memoryBuilder);
            _memory = memoryBuilder.Build();
        }
    }

    public class SystemMemoryBuilder
    {
        private List<byte> _bytes = new List<byte>();
        private SystemMemory _systemMemory;

        public int CurrentAddress => _bytes.Count;

        public SystemMemoryBuilder(SystemMemory systemMemory)
        {
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
        {
            var b = new GameByteGridPoint(
                AddByte().WithMask(mask), 
                AddByte().WithMask(mask), 
                width, 
                height);

            return b;
        }
    }
}
