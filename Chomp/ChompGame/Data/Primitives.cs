using ChompGame.Data.Memory;
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
            get => _bit.Value ? _trueValue : _falseValue;
            set => _bit.Value = value.Equals(_trueValue);
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

        public GameByte WithMask(Bit mask) => (byte)mask == 255 ? this : new MaskedByte(Address, mask, _memory);
    }

    public class MaskedByte : GameByte
    {
        private byte _mask;
        public MaskedByte(int address, Bit mask, SystemMemory memory) : base(address, memory)
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
                _memory[_address + 1] = (byte)(value >> 8);
            }
        }

        public GameShort(int address, SystemMemory memory)
        {
            _memory = memory;
            _address = address;
        }
    }

    public class GameInteger
    {
        private int _address;
        private SystemMemory _memory;

        public uint Value
        {
            get
            {
                var a = (uint)_memory[_address];
                var b = (uint)_memory[_address + 1];
                var c = (uint)_memory[_address + 2];
                var d = (uint)_memory[_address + 3];

                var unsigned = (d << 24) + (c << 16) + (b << 8) + a;
                return unsigned;
            }
            set
            {
                _memory[_address] = (byte)value;
                _memory[_address + 1] = (byte)(value >> 8);
                _memory[_address + 2] = (byte)(value >> 16);
                _memory[_address + 3] = (byte)(value >> 24);
            }
        }

        public GameInteger(int address, SystemMemory memory)
        {
            _memory = memory;
            _address = address;
        }
    }


    public abstract class Nibble
    {
        public abstract byte Value { get; set; }
        
        public abstract int Address { get; }
    }

    public class LowNibble : Nibble
    {
        private int _address;
        private SystemMemory _memory;

        public override int Address => _address;

        public LowNibble(SystemMemoryBuilder memoryBuilder)
        {
            _address = memoryBuilder.CurrentAddress;
            _memory = memoryBuilder.Memory;
        }
        public LowNibble(int address, SystemMemory memory)
        {
            _address = address;
            _memory = memory;
        }

        public override byte Value
        {
            get
            {
                return (byte)(_memory[_address] & 15);
            }
            set
            {
                _memory[_address] = (byte)(_memory[_address] & 240);
                _memory[_address] = (byte)(_memory[_address] | value);
            }
        }
    }


    public class HighNibble : Nibble
    {
        private int _address;
        private SystemMemory _memory;

        public override int Address => _address;

        public HighNibble(SystemMemoryBuilder memoryBuilder)
        {
            _address = memoryBuilder.CurrentAddress;
            _memory = memoryBuilder.Memory;
        }

        public HighNibble(int address, SystemMemory memory)
        {
            _address = address;
            _memory = memory;
        }

        public override byte Value
        {
            get
            {
                return (byte)((_memory[_address] & 240) >> 4);
            }
            set
            {
                _memory[_address] = (byte)(_memory[_address] & 15);
                _memory[_address] = (byte)(_memory[_address] | (value << 4));
            }
        }
    }


    public class TwoBit
    {
        private readonly SystemMemory _memory;
        private readonly int _address;
        private readonly byte _shift;
        private readonly byte _mask;

        public int Address => _address;
            
        public TwoBit(SystemMemory memory, int address, int shift)
        {
            _memory = memory;
            _shift = (byte)shift;
            _address = address;
            _mask = (byte)(2.Power(shift) + 2.Power(shift + 1));
        }

        public byte Value
        {
            get
            {
                byte masked = (byte)(_memory[_address] & _mask);
                return (byte)(masked >> _shift);
            }
            set
            {
                value = (byte)(value << _shift);
                value = (byte)(value & _mask);

                _memory[_address] = (byte)(_memory[_address] & ~_mask);
                _memory[_address] = (byte)(_memory[_address] | value);
            }
        }
    }

    public class TwoBitEnum<T> 
        where T : Enum
    {
        private TwoBit _value;

        public int Address => _value.Address;

        public TwoBitEnum(SystemMemory memory, int address, int shift)  
        {
            _value = new TwoBit(memory, address, shift);
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

    public class FourBitEnum<T>
        where T : Enum
    {
        private Nibble _value;

        public int Address => _value.Address;

        public FourBitEnum(SystemMemory memory, int address, bool low)
        {
            if (low)
                _value = new LowNibble(address, memory);
            else 
                _value = new HighNibble(address, memory);
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


}
