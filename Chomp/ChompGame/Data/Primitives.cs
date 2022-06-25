using ChompGame.Extensions;
using System;

namespace ChompGame.Data
{
    public enum Bit : byte
    {
        None=0,
        Bit0 = 1,
        Bit1 = 2,
        Bit2 = 4,
        Bit3 = 8,
        Bit4 = 16,
        Bit5 = 32,
        Bit6 = 64,
        Bit7 = 128
    }

    class GameBit
    {
        private int _address;
        private Bit _bit;
        private SystemMemory _memory;


        public bool Value
        {
            get
            {
                var byteValue = _memory[_address];
                var mask = _bit.ToMask();
                return (byteValue & mask) != 0;
            }
            set
            {
                var byteValue = _memory[_address];
                var mask = _bit.ToMask();
                if (value)
                    _memory[_address] = (byte)(byteValue | mask);
                else
                    _memory[_address] = (byte)(byteValue & ~mask);

            }
        }

        public GameBit(int address, Bit bit, SystemMemory memory)
        {
            _bit = bit;
            _address = address;
            _memory = memory;
        }
    }

    class GameEnum2<T>
    {
        private GameBit _bit;
        private static readonly T _falseValue;
        private static readonly T _trueValue;

        static GameEnum2()
        {
            var values = Enum.GetValues(typeof(T)) as T[];
            _falseValue = values[0];
            _trueValue = values[1];
        }

        public GameEnum2(int address, Bit bit, SystemMemory memory)
        {
            _bit = new GameBit(address, bit, memory);
        }

        public T Value
        { 
            get=> _bit.Value? _trueValue : _falseValue;
            set =>_bit.Value = value.Equals(_trueValue);            
        }                    
    }

    class GameByteEnum<T>
    {
        private GameByte _value;
        
            
        public GameByteEnum(GameByte value)
        {
            _value = value;
        }

        public T Value
        {
            get
            {
                object currentValue = _value.Value;
                return (T)currentValue;
            }
            set
            {
                var byteValue = (byte)(object)value;
                _value.Value = byteValue;
            }            
        }
    }

    public class GameByte
    {
        private int _address;
        private SystemMemory _memory;

        public int Address => _address;

        public virtual byte Value
        {
            get => _memory[_address];
            set => _memory[_address] = value;
        }

        public GameByte(int address, SystemMemory memory)
        {
            _memory = memory;
            _address = address;
        }      
        
        public static implicit operator byte(GameByte g) => g.Value;

        public override string ToString() => $"{Value} {Value.ToString("X2")}";

        public MaskedByte WithMask(Bit mask) => new MaskedByte(_address, mask, _memory);
    }

    public class MaskedByte : GameByte
    {
        private byte _mask;
        public MaskedByte(int address, Bit mask, SystemMemory memory) : base(address,memory)
        {
            _mask = (byte)mask;
        }

        public override byte Value 
        { 
            get
            {
                byte b = base.Value;
                return (byte)(b & _mask);
            }
            set
            {
                var maskedValue = (byte)(value & _mask);
                base.Value = (byte)(base.Value & (byte)~_mask);
                base.Value = (byte)(base.Value | maskedValue);
            }
        }
    }

    public class GameShort
    {
        private int _address;
        private SystemMemory _memory;

        public ushort Value
        {
            get
            {
                var low = (ushort)_memory[_address];
                var high = (ushort)_memory[_address + 1];

                var unsigned = (high << 8) + low;
                return (ushort)unsigned;
            }
            set
            {
                _memory[_address] = (byte)value;
                _memory[_address+1] = (byte)(value >> 8);
            }
        }

        public GameShort(int address, SystemMemory memory)
        {
            _memory = memory;
            _address = address;
        }
    }

}
