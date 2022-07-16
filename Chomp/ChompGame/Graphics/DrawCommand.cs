using ChompGame.Data;
using System;

namespace ChompGame.Graphics
{
    public enum DrawOpcode
    {
        Hold=0,
        Advance=1,
        UpdateAttributes=2,
        Reposition=3
    }

    public class DrawInstruction
    {
        private GameByte _byte;
        private DrawInstructionGroup _currentGroup;

        public DrawInstruction(GameByte @byte, DrawInstructionGroup currentGroup)
        {
            _byte = @byte;
            _currentGroup = currentGroup;
        }

        public DrawOpcode OpCode 
        {
            get => _currentGroup.GetOpcode(OpCodeIndex);
            set =>_currentGroup.SetOpcode(OpCodeIndex,value);
        }

        public byte OpCodeIndex
        {
            get
            {
                return (byte)(_byte.Value & 3); 
            }
            set
            {
                var b = (byte)value;

                _byte.Value = (byte)(_byte.Value & 252);
                _byte.Value = (byte)(_byte.Value | b);
            }
        }

        public byte Value
        {
            get => _currentGroup.GetValue(OpCodeIndex);
            set => _currentGroup.SetValue(OpCodeIndex,value);
        }        

        public int ValueAddress
        {
            get => _currentGroup.Address + 1 + OpCodeIndex;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DrawInstructionGroup
    {
        private GameByte _byte;
        private SystemMemory _memory;

        public int Address
        {
            get => _byte.Address;
            set => _byte.Address = value;
        }

        public DrawOpcode Opcode0 => GetOpcode(0);
        public DrawOpcode Opcode1 => GetOpcode(1);
        public DrawOpcode Opcode2=> GetOpcode(2);
        public DrawOpcode Opcode3=> GetOpcode(3);

        public DrawOpcode GetOpcode(byte index)
        {
            switch (index)
            {
                case 0:
                    return (DrawOpcode)(_byte.Value & 3);
                case 1:
                    return (DrawOpcode)((_byte.Value & 12) >> 2);
                case 2:
                    return (DrawOpcode)((_byte.Value & 48) >> 4);
                default:
                    return (DrawOpcode)((_byte.Value & 192) >> 6);
            }
        }
        
        public void SetOpcode(byte index, DrawOpcode drawOpcode)
        {
            switch(index)
            {
                case 0:
                    var o = (byte)((byte)drawOpcode);
                    _byte.Value = (byte)(_byte & 252);
                    _byte.Value = (byte)(_byte | o);
                    break;
                case 1:
                     o = (byte)((byte)drawOpcode << 2);
                    _byte.Value = (byte)(_byte & 243);
                    _byte.Value = (byte)(_byte | o);
                    break;
                case 2:
                    o = (byte)((byte)drawOpcode << 4);
                    _byte.Value = (byte)(_byte & 207);
                    _byte.Value = (byte)(_byte | o);
                    break;
                default:
                    o = (byte)((byte)drawOpcode << 6);
                    _byte.Value = (byte)(_byte & 63);
                    _byte.Value = (byte)(_byte | o);
                    break;
            }
        }

        public byte GetValue(byte index)
        {
            return _memory[_byte.Address + 1 + index];
        }

        public void SetValue(byte index, byte value)
        {
            _memory[_byte.Address + index+1] = value;
        }

        public DrawInstructionGroup(GameByte @byte, SystemMemory memory)
        {
            _byte = @byte;
            _memory = memory;
        }
    }
}
