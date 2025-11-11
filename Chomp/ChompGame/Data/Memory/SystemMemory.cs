using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.Data
{

    public class SystemMemory
    {
        private MemoryBlock _memory;

        private Dictionary<AddressLabels, int> _addressLabels = new Dictionary<AddressLabels, int>();


        public byte this[int index] 
        {
            get => _memory[index];
            set => _memory[index] = value;
        }

        public byte[] Span(int index, int length) => _memory.Span(index, length);

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
        }


        public void AddLabel(AddressLabels label, int address)
        {
            _addressLabels.Add(label, address);
        }

        public int GetAddress(AddressLabels label) => _addressLabels[label];

        public override string ToString() => _memory.ToString();

        public byte[] ToArray() => _memory.ToArray();
    }

 }
