using ChompGame.GameSystem;
using System.Linq;

namespace ChompGame.MainGame
{
    internal class SaveManager
    {
        public const int CartMemorySize = 100;
        private const int SaveSlotSize = 25;
        private ChompGameModule _gameModule;

        private MainSystem GameSystem => _gameModule.GameSystem;

        public SaveManager(ChompGameModule gameModule)
        {
            _gameModule = gameModule;
        }

        public int FreeSlot()
        {
            for (int slot = 0; slot < 4; slot++)
            {
                if (!IsSaveSlotValid(slot))
                    return slot;
            }
            return 0;
        }

        public bool IsSaveSlotValid(int slot)
        {
            byte[] buffer = GameSystem.Memory.Span(
                GameSystem.Memory.GetAddress(AddressLabels.CartMemory) + (slot * SaveSlotSize), SaveSlotSize);

            if (buffer.All(p => p == 0))
                return false;

            ushort checksum = ComputeFletcher16(buffer, buffer.Length - 2);

            return buffer[^2] == (byte)(checksum >> 8) &&
                   buffer[^1] == (byte)(checksum & 0xFF);
        }

        public bool AnySaveSlotsValid()
        {
            for (int slot = 0; slot < 4; slot++)
            {
                if (IsSaveSlotValid(slot))
                    return true;
            }
            return false;
        }

        public bool AnySaveSlotsFree()
        {
            for (int slot = 0; slot < 4; slot++)
            {
                if (!IsSaveSlotValid(slot))
                    return true;
            }
            return false;
        }

        public int SaveSlotAddress(int slot)
        {
            return _gameModule.GameSystem.Memory.GetAddress(AddressLabels.CartMemory) + (slot * SaveSlotSize);
        }

        private byte[] SaveSlotData(int slot)
        {
            var addr = SaveSlotAddress(slot);
            return _gameModule.GameSystem.Memory.Span(addr, SaveSlotSize);
        }

        public void DeleteSaveSlot(int slot)
        {
            int address = SaveSlotAddress(slot);

            for (int i = 0; i < SaveSlotSize; i++)
            {
                GameSystem.Memory[address + i] = 0;
            }
        }

        public void SaveCurrentGame(int slot, bool carryingBomb)
        {
            var statusBar = _gameModule.StatusBar;

            // 0: current level
            // 1 - 4: score
            // 5: lives and carrying bomb (high bit)
            // 6: last exit-type(low) and health(high)
            // 7-22: scene parts destroyed
            // 23-24: two-byte Fletcher-16 checksum (high, low)

            int index = SaveSlotAddress(slot);
            GameSystem.Memory[index] = (byte)_gameModule.CurrentLevel;

            GameSystem.Memory.BlockCopy(statusBar.ScorePtr, ++index, 4);
            index += 4;
            GameSystem.Memory[index++] = (byte)(statusBar.Lives | (byte)(carryingBomb ? 128 : 0)); 
            GameSystem.Memory[index++] = (byte)((byte)_gameModule.LastExitType | (byte)(statusBar.Health << 4));

            index = _gameModule.ScenePartsDestroyed.WriteToSaveBuffer(GameSystem.Memory, index);

            // compute 16-bit checksum over all bytes except the final two checksum bytes
            byte[] saveBuffer = SaveSlotData(slot);
            ushort checksum = ComputeFletcher16(saveBuffer, saveBuffer.Length - 2);

            // store checksum high then low at the end of the buffer
            GameSystem.Memory[index++] = (byte)(checksum >> 8);
            GameSystem.Memory[index++] = (byte)(checksum & 0xFF);

            // write cart to disk
            var cart = GameSystem.Memory.Span(GameSystem.Memory.GetAddress(AddressLabels.CartMemory), -1);
            System.IO.File.WriteAllBytes("chomp.cart", cart);
        }


        // Simple Fletcher-16 checksum
        private static ushort ComputeFletcher16(byte[] data, int length)
        {
            int sum1 = 0;
            int sum2 = 0;

            for (int i = 0; i < length; i++)
            {
                sum1 = (sum1 + data[i]) % 255;
                sum2 = (sum2 + sum1) % 255;
            }

            return (ushort)((sum2 << 8) | sum1);
        }

    }
}
