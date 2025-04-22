using ChompGame.GameSystem;
using ChompGame.MainGame;
using System;

namespace ChompGame.Data
{
    public class GameRAM
    {
        private readonly Specs _specs;
        private GameShort _freeRamOffset;
        private SystemMemory _memory;
      
        public GameRAM(Specs specs)
        {
            _specs = specs;
        }

        public void Initialize(GameShort freeRamOffset, SystemMemory memory)
        {
            _memory = memory;
            _freeRamOffset = freeRamOffset;
        }

        public void Reset()
        {
            _freeRamOffset.Value = 0;
            int beginAddress = _memory.GetAddress(AddressLabels.FreeRAM);
            int endAddress = beginAddress + _specs.GameRAMSize;

            for (int i = beginAddress; i < endAddress; i++)
            {
                _memory[i] = 0;
            }
        }

        public int ClaimMemory(int size)
        {
            int currentAddress = CurrentAddress;
            CurrentAddress += size;
            return currentAddress;
        }

        public void StoreData(IMemoryBlock data, int address)
        {
            _memory.BlockCopy(data.Address, address, data.Bytes);
        }

        public void RetrieveData(IMemoryBlock data, int source)
        {
            _memory.BlockCopy(source, data.Address, data.Bytes);
        }

        public int CurrentAddress
        {
            get => _memory.GetAddress(AddressLabels.FreeRAM) + _freeRamOffset.Value;
            set 
            {
                int newOffset = value - _memory.GetAddress(AddressLabels.FreeRAM);
                if(newOffset > _specs.GameRAMSize)
                    throw new Exception("Access Violation");

                _freeRamOffset.Value = (ushort)newOffset;
            }
        }
       
    }
}
