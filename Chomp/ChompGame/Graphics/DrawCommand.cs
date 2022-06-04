using ChompGame.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChompGame.Graphics
{
    public enum DrawCommandType
    {        
        Hold,
        MoveBrush,
    }

    public class DrawCommand
    {
        private GameEnum2<DrawCommandType> _commandType;
        private MaskedByte _value;

        public DrawCommandType CommandType
        {
            get => _commandType.Value;
            set => _commandType.Value = value;
        }

        public byte Value
        {
            get => _value.Value;
            set => _value.Value = value;
        }
        

        public DrawCommand(int address, SystemMemory memory)
        {
            _commandType = new GameEnum2<DrawCommandType>(address, Bit.Bit7, memory);
            _value = new MaskedByte(address, (Bit)127, memory);
        }

        public override string ToString() => $"{CommandType} {Value}";

    }
}
