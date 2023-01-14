using ChompGame.GameSystem;
using ChompGame.MainGame;
using System;

namespace ChompGame.Data
{
    public class GameRAM
    {
        private readonly Specs _specs;
        private ExtendedByte2 _freeRamOffset;
        private SystemMemory _memory;
      
        public GameRAM(Specs specs)
        {
            _specs = specs;
        }

        public void Initialize(ExtendedByte2 freeRamOffset, SystemMemory memory)
        {
            _memory = memory;
            _freeRamOffset = freeRamOffset;
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

                _freeRamOffset.Value = newOffset;
            }
        }
       
    }
}
