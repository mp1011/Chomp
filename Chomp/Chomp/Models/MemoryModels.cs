using System.Collections.Generic;
using System;
using Chomp.SystemModels;

namespace Chomp.Models
{
    public interface IAddressable
    {
        int Address { get; }
    }

    public abstract class MemoryValue : IAddressable
    {        
        public int Address { get; }
        public abstract int BitWidth { get; }
        protected Memory Memory { get; }

        public MemoryValue(Memory memory)
        {
            Memory = memory;
            if(memory != null)
                Address = memory.GetAndUpdateWritePointer(this);
        }

        public int MaxValue => (int)Math.Pow(2, BitWidth);
    }

    public abstract class AlignedValue : MemoryValue
    {
        public AlignedValue(Memory memory) : base(memory) { }
    }

    public abstract class UnalignedValue : MemoryValue
    {
        protected BitOffset BitOffset { get; }
        public UnalignedValue(Memory memory) : base(memory) 
        {
            if(memory != null)
                BitOffset = memory.GetAndUpdateBitOffset(this);
        }
    }

    public class Bit : UnalignedValue
    {
        public override int BitWidth => 1;

        public bool Value => throw new System.NotImplementedException();

        public Bit(Memory memory) : base(memory) { }
    }

    public class TwoBit : UnalignedValue
    {
        public override int BitWidth => 2;

        public int Value => throw new System.NotImplementedException();

        public TwoBit(Memory memory) : base(memory) { }
    }

    public class Nibble : UnalignedValue
    {
        public override int BitWidth => 4;

        public byte Value => throw new System.NotImplementedException();

        public Nibble(Memory memory) : base(memory) { }
    }

    public class Byte : AlignedValue
    {
        public Byte(Memory memory) : base(memory) { }

        public override int BitWidth => 8;

        public void Set(byte value)
        {
            throw new NotImplementedException();
        }

        public byte Value => throw new System.NotImplementedException();
    }

    public class SignedInt : AlignedValue
    {
        private const ushort _zeroValue = (ushort)32768;

        public SignedInt(Memory memory) : base(memory)
        {
        }

        public override int BitWidth => 16;

        public int Value
        {
            get
            {
                var low = (int)Memory[Address];
                var high = (int)Memory[Address + 1];

                var unsigned = (high << 8) + low;

                return unsigned - _zeroValue;
            }
        }

        public static implicit operator int(SignedInt u) => u.Value;

    }

    public class UnsignedInt : AlignedValue
    {
        public UnsignedInt(Memory memory) : base(memory)
        {
        }

        public void Set(ushort value)
        {
            throw new NotImplementedException();
        }

        public void Add(ushort add) => Set((ushort)Value + add);

        public override int BitWidth => 16;

        public int Value
        {
            get
            {
                var low = (int)Memory[Address];
                var high = (int)Memory[Address + 1];

                var unsigned = (high << 8) + low;
                return unsigned;
            }
        }

        public static implicit operator int(UnsignedInt u) => u.Value;

        public void Set(int value)
        {
            //only lower 2 bytes used
            Memory[Address] = (byte)value;
            Memory[Address + 1] = (byte)(value >> 8);
        }


    }

    public class Array<T> : MemoryValue
        where T:MemoryValue
    {
        private readonly Byte _length;
        public int Length => _length.Value;

        public override int BitWidth => throw new NotImplementedException();

        public Array(Memory memory, Byte length) : base(memory)
        {
            _length = length;   
        }
            

        public T this[int index]
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public IEnumerable<T> AsEnumerable()
        {
            //for (int i = 0; i < Length; i++)
            //    yield return this[i];
            throw new System.NotImplementedException();
        }
    }

    public abstract class Pointer<T>
    {
        public static implicit operator T(Pointer<T> p)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Enum<T> : IAddressable
        where T:Enum
    {
        private Byte _value;

        public Enum(Byte value)
        {
            _value = value;
        }

        public T Value => (T)(object)_value.Value;

        public void Set(T newValue) => _value.Set((byte)(object)newValue);

        public bool GetFlag(T flag)
        {
            var flagByte = (byte)(object)flag;
            var thisByte = _value.Value;
            return (thisByte & flagByte) > 0;

        }

        public void SetFlag(T flag, bool on)
        {
            var flagByte = (byte)(object)flag;
            var thisByte = _value.Value;

            if (on)
                _value.Set((byte)(thisByte | flagByte));
            else
                _value.Set((byte)(thisByte & ~flagByte));
        }

        public override string ToString() 
            => Value.ToString();

        //public static bool operator ==(Enum<T> r, T value) => r.Memory[r.Address] == (byte)(object)value;
        //public static bool operator !=(Enum<T> r, T value) => r.Memory[r.Address] != (byte)(object)value;

        //public static implicit operator T(Enum<T> ramEnum) => (T)Enum.ToObject(typeof(T), ramEnum.Memory[ramEnum.Address]);

        public override bool Equals(object obj)
        {
            if (obj is Enum<T> r)
                return _value.Value == r._value.Value;
            else
                return false;
        }

        public override int GetHashCode()
            => _value.Value;

        public int Address => _value.Address;
    }


}
