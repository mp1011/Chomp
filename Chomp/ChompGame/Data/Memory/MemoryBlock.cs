using ChompGame.GameSystem;
using ChompGame.MainGame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.Data.Memory
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
                if (index == 1183)
                    GameDebug.NoOp();

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

        public override string ToString()
        {
            return string.Join("",
                _memory.Select(i => i.ToString("X2")).ToArray());
        }
    }

    public abstract class DynamicMemoryBlock : MemoryBlock
    {
        public abstract int CurrentAddress { get; }
        public abstract FixedMemoryBlock ToFixed();
        public abstract void Add(byte value);
    }

    public class ListMemoryBlock : DynamicMemoryBlock
    {
        private List<byte> _memory = new List<byte>();

        public ListMemoryBlock()
        {
        }

        public override int CurrentAddress => _memory.Count;

        public override byte this[int index]
        {
            get => _memory[index];
            set => _memory[index] = value;
        }

        public override void BlockCopy(int sourceStart, int destinationStart, int length)
        {
            throw new NotSupportedException();
        }

        public override void Add(byte value)
        {
            _memory.Add(value);
        }

        public override FixedMemoryBlock ToFixed() => new FixedMemoryBlock(_memory.ToArray());
    }

    public class GameRamMemoryBlock : DynamicMemoryBlock
    {
        private SystemMemory _memory;
        private Specs _specs;

        public GameRamMemoryBlock(SystemMemory memory, Specs specs)
        {
            _memory = memory;
            _specs = specs;
            _currentAddress = _memory.GetAddress(AddressLabels.FreeRAM);
        }

        public override byte this[int index] { get => _memory[index]; set => _memory[index] = value; }

        private int _currentAddress;

        public override int CurrentAddress => _currentAddress;

        public override void Add(byte value)
        {
            this[_currentAddress] = value;
            _currentAddress++;

            if (_currentAddress > _memory.GetAddress(AddressLabels.FreeRAM) + _specs.GameRAMSize)
                throw new Exception("Access Violation");
        }

        public override FixedMemoryBlock ToFixed()
        {
            throw new NotImplementedException();
        }

        public override void BlockCopy(int sourceStart, int destinationStart, int length)
        {
            throw new NotImplementedException();
        }
    }
}
