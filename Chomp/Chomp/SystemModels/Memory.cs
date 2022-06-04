using Chomp.Models;
using Chomp.Services;

namespace Chomp.SystemModels
{
    public class Memory
    {
        private byte[] _memory;
        private UnsignedInt _currentAddress;
        private Enum<BitOffset> _currentBitOffset;
        
        public Memory(SystemSpecs specs)
        {
            _memory = new byte[ComputeRequiredSpace(specs)];
            _currentAddress = new UnsignedInt(this);
            _currentAddress.Set(16);
            _currentBitOffset = new Enum<BitOffset>(new Byte(this));
        }

        public int GetAndUpdateWritePointer(MemoryValue value)
        {
            if (_currentAddress == null)
                return 0;

            var currentAddress = _currentAddress.Value;
            _currentAddress.Add((ushort)(value.BitWidth / 8));
            return currentAddress;
        }

        public BitOffset GetAndUpdateBitOffset(MemoryValue value)
        {
            var current = _currentBitOffset.Value;
            throw new System.NotImplementedException();
        }

        private int ComputeRequiredSpace(SystemSpecs specs)
        {
            var spaceRequired = specs.WorkingRAM;

            spaceRequired += specs.PatternTableSpecs.Cells * specs.BitsPerPixel;

            var tileCount = specs.PatternTableSpecs.Cells / (specs.TileSize*specs.TileSize);

            var tileMemoryValue = MemoryValueFactory.GetSmallestForValue(tileCount);

            spaceRequired += (specs.NameTableSpecs.Cells * tileMemoryValue.BitWidth) / 8;

            return spaceRequired;
        }

        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= _memory.Length)
                    throw new System.Exception("Illegal memory access");

                return _memory[index];
            }
            set
            {
                //todo, forbid writing to ROM
                if (index < 0 || index >= _memory.Length)
                    throw new System.Exception("Illegal memory access");

                _memory[index] = value;
            }
        }

        public Bit DeclareBit()
            => new Bit(this);

        public TwoBit DeclareTwoBit()
            => new TwoBit(this);

        public Nibble DeclareNibble()
            => new Nibble(this);

        public Byte DeclareByte() =>
            new Byte(this);

        public Point DeclarePoint()
            =>new Point(DeclareSignedInt(), DeclareSignedInt());

        public SignedInt DeclareSignedInt()
            => new SignedInt(this);
    }
}
