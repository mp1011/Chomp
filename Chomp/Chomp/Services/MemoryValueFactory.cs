using Chomp.Models;
using Chomp.SystemModels;

namespace Chomp.Services
{
    public class MemoryValueFactory
    {
        private readonly SystemSpecs _specs;
        private readonly Memory _memory;

        public MemoryValueFactory(SystemSpecs specs, Memory memory)
        {
            _specs = specs;
            _memory = memory;
        }

        public static MemoryValue GetSmallestForValue(int value)
        {
            var possibeValues = new MemoryValue[]
            {
                new Bit(null),
                new TwoBit(null),
                new Nibble(null),
                new Byte(null),
                new UnsignedInt(null)
            };

            foreach(var possibleValue in possibeValues)
            {
                if (possibleValue.MaxValue >= value)
                    return possibleValue;
            }

            throw new System.Exception("Value requires more than two bytes to represent");
        }

        public PatternTable CreatePatternTable()
        {
            return new PatternTable(_specs.PatternTableSpecs, _memory.DeclareTwoBit());
        }

        public NameTable CreateNameTable()
        {
            int tileCount = _specs.NameTableSpecs.Cells / (_specs.TileSize * _specs.TileSize);

            var tileIndexModel = GetSmallestForValue(tileCount);
            if (tileCount > tileIndexModel.MaxValue)
                throw new System.Exception("Too many tiles to represent with four bits");

            return new NameTable(_specs.NameTableSpecs, _memory.DeclareNibble());
        }
    }
}
