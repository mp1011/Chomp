using ChompGame.Extensions;
using System;

namespace ChompGame.Data
{
    public enum Bit : byte
    {
        None = 0,
        Bit0 = 1,
        Bit1 = 2,
        Bit2 = 4,
        Bit3 = 8,
        Bit4 = 16,
        Bit5 = 32,
        Bit6 = 64,
        Bit7 = 128,
        Right2 = 3,
        Right3 = 7,
        Right4 = 15,
        Right5 = 31,
        Right6 = 63,
        Right7 = 127,
        Left2 = 192,
        Left3 = 224,
        Left4 = 240,
        Left5 = 248,
        Left6 = 252,
        Left7 = 254
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

        public static implicit operator bool(GameBit b) => b.Value;
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
        private SystemMemory _memory;

        public int Address { get; set; }

        public virtual byte Value
        {
            get => _memory[Address];
            set => _memory[Address] = value;
        }

        public GameByte(int address, SystemMemory memory)
        {
            _memory = memory;
            Address = address;
        }     
        
        public bool GetBit(Bit b)
        {
            return (_memory[Address] & (byte)b) != 0;
        }

        public void SetBit(Bit b, bool value)
        {
            if (value)
                _memory[Address] = (byte)(_memory[Address] | (byte)b);
            else
                _memory[Address] = (byte)(_memory[Address] & (byte)~b);
        }
        
        public static implicit operator byte(GameByte g) => g.Value;

        public override string ToString() => $"{Value} {Value.ToString("X2")}";

        public MaskedByte WithMask(Bit mask) => new MaskedByte(Address, mask, _memory);
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

    public class NibbleArray
    {
        private int _address;
        private SystemMemory _memory;

        public int Address => _address;

        public NibbleArray(int address, SystemMemory memory)
        {
            _memory = memory;
            _address = address;
        }

        public byte this[int index]
        {
            get
            {
                int memoryIndex = _address + (index / 2);
                if((index % 2)==0)
                    return (byte)(_memory[memoryIndex] & 15);
                else
                    return (byte)((_memory[memoryIndex] & 240) >> 4);
            }
            set
            {
                int memoryIndex = _address + (index / 2);
                if ((index % 2) == 0)
                {
                    _memory[memoryIndex] = (byte)(_memory[memoryIndex] & 240);
                    _memory[memoryIndex] = (byte)(_memory[memoryIndex] | value);
                }
                else
                {
                    _memory[memoryIndex] = (byte)(_memory[memoryIndex] & 15);
                    _memory[memoryIndex] = (byte)(_memory[memoryIndex] | (value << 4));
                }
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
