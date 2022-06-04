using Chomp.Models;
using Chomp.Services;

namespace Chomp.SystemModels
{
    public class GameSystem
    {
        public Pointer<IPalette> ActivePalette { get; }
        public SystemSpecs Specs { get; }
        public Background Background { get; }
        public PatternTable PatternTable { get; }
        public Memory Memory { get; }

        public GameSystem(SystemSpecs specs, MemoryValueFactory memoryValueFactory)
        {
            Specs = specs;
            Memory = new Memory(specs);
            Background = new Background(this, memoryValueFactory);
            PatternTable = memoryValueFactory.CreatePatternTable();
        }
    }
}
