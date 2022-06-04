using System;
using System.Collections.Generic;
using System.Text;

namespace ChompGame.Data
{
    /// <summary>
    /// byte representing an address in a block that's bookended with FF bytes
    /// </summary>
    public class ByteAddressWithFPad  
    {
        private readonly GameByte _byte;
        private readonly SystemMemory _memory;

        public ByteAddressWithFPad(GameByte b, SystemMemory systemMemory)
        {
            _byte = b;
            _memory = systemMemory;
        }

        public byte Value
        {
            get => _byte.Value;
            set => _byte.Value = value;
        }

        /// <summary>
        /// Advances the current address by 1. If the value equals FF, backtracks to
        /// to the first address preceded by FF.
        /// </summary>
        public void Next()
        {
            _byte.Value++;

            if(_memory[_byte] == 0xFF)
            {
                while(_byte > 0)
                {
                    _byte.Value--;
                    if(_memory[_byte] == 0xFF)
                    {
                        _byte.Value++;
                        break;
                    }
                }
            }
        }
    }
}
