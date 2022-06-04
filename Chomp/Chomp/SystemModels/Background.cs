using Chomp.Models;
using Chomp.Services;

namespace Chomp.SystemModels
{
    public class Background
    {
        private readonly GameSystem _gameSystem;
        public Point Scroll { get; }

        public NameTable NameTable { get; }

        public Background(GameSystem gameSystem, MemoryValueFactory memoryValueFactory)
        {
            _gameSystem = gameSystem;
            Scroll = _gameSystem.Memory.DeclarePoint();
            NameTable = memoryValueFactory.CreateNameTable();
        }

        public Nibble GetTopLeftCell()
        {
            var tileX = (byte)(Scroll.X / _gameSystem.Specs.TileSize);
            var tileY = (byte)(Scroll.Y / _gameSystem.Specs.TileSize);
            return NameTable[tileX, tileY];
        }
    }
}
