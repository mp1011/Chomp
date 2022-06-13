using ChompGame.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChompGame.Graphics
{
   
    public class DrawCommand
    {
        private SystemMemory _memory;
        private GameBit _ptMove;
        private MaskedByte _value;
        private byte _fullValue => _memory[_value.Address];

        public bool PTMove
        {
            get => _ptMove.Value;
            set => _ptMove.Value = value;
        }

        public bool IsEndMarker
        {
            get => _fullValue == 0;
            set
            {
                if (value)
                    _memory[_value.Address] = 0;
            }
        }

        public bool IsRepositionMarker
        {
            get => _fullValue == 128;
            set
            {
                if (value)
                    _memory[_value.Address] = 128;
            }
        }

        public byte Value
        {
            get => _value.Value;
            set => _value.Value = value;
        }
        

        public DrawCommand(int address, SystemMemory memory)
        {
            _memory = memory;
            _ptMove = new GameBit(address, Bit.Bit7, memory);
            _value = new MaskedByte(address, (Bit)127, memory);
        }

        public override string ToString()
        {
            if (IsRepositionMarker)
                return "Reposition";
            else if (PTMove)
                return $"Move {Value}";
            else
                return $"Stay {Value}";
        }

    }
}
