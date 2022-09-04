using ChompGame.GameSystem;

namespace ChompGame.Data
{
    class HealthGuage
    {
        private GameByte _tileIndex;
        private GameByte _health;
        private GameByte _maxHealth;

        private CoreGraphicsModule _coreGraphicsModule;

        private Specs Specs => _coreGraphicsModule.GameSystem.Specs;

        public byte Health
        {
            get => _health.Value;
            set => _health.Value = value;
        }

        public byte MaxHealth
        {
            get => _maxHealth.Value;
            set => _maxHealth.Value = value;
        }

        public byte TileIndex
        {
            get => _tileIndex.Value;
            set => _tileIndex.Value = value;
        }

        public HealthGuage(
           SystemMemoryBuilder systemMemoryBuilder,
           CoreGraphicsModule coreGraphicsModule)
        {
            _coreGraphicsModule = coreGraphicsModule;
            _tileIndex = systemMemoryBuilder.AddByte();
            _health = systemMemoryBuilder.AddByte();
            _maxHealth = systemMemoryBuilder.AddByte();
        }

        public void Draw(
           byte screenColumn,
           byte row)
        {
            PatternTablePoint patternTablePoint = new PatternTablePoint(Specs);

            int maxHearts = MaxHealth / 2;
            int fullHearts = Health / 2;
            bool halfHeart = Health % 2 == 1;

            int index = 0;
            while(index < fullHearts)
            {
                patternTablePoint.TileIndex = TileIndex + 3;
                patternTablePoint.Y += row;
                _coreGraphicsModule.WriteTileToScanlineBuffer(screenColumn + (index * Specs.TileWidth), patternTablePoint);
                index++;
            }

            if(halfHeart)
            {
                patternTablePoint.TileIndex = TileIndex + 2;
                patternTablePoint.Y += row;
                _coreGraphicsModule.WriteTileToScanlineBuffer(screenColumn + (index * Specs.TileWidth), patternTablePoint);
                index++;
            }

            while(index < maxHearts)
            {
                patternTablePoint.TileIndex = TileIndex + 1;
                patternTablePoint.Y += row;
                _coreGraphicsModule.WriteTileToScanlineBuffer(screenColumn + (index * Specs.TileWidth), patternTablePoint);
                index++;
            }

            patternTablePoint.TileIndex = TileIndex;
            patternTablePoint.Y += row;
            _coreGraphicsModule.WriteTileToScanlineBuffer(screenColumn + (index * Specs.TileWidth), patternTablePoint);
        }
    }
}