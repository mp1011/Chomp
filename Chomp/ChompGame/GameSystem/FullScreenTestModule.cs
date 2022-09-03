using ChompGame.Data;
using ChompGame.ROM;
using System;

namespace ChompGame.GameSystem
{
    class FullScreenTestModule : Module, IMasterModule
    {
        private NBitPlane _romPatternTable;
        private NBitPlane _romNameTable;
        private SpritesModule _spritesModule;
        private InputModule _inputModule;
        private TileModule _tileModule;

        public FullScreenTestModule(MainSystem mainSystem) : base(mainSystem)
        {
            _spritesModule = mainSystem.GetModule<SpritesModule>();
            _inputModule = mainSystem.GetModule<InputModule>();
            _tileModule = mainSystem.GetModule<TileModule>();
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            memoryBuilder.BeginROM();

            _romPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            _romNameTable = memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);
        }

        private int t = 0;
        private int d = 0;
        public void OnLogicUpdate()
        {
            _inputModule.OnLogicUpdate();

            if (_inputModule.Player1.LeftKey.IsDown() && _tileModule.Scroll.X > 0)
                _tileModule.Scroll.X--;
            else if (_inputModule.Player1.RightKey.IsDown() && _tileModule.Scroll.X < 255)
                _tileModule.Scroll.X++;
            else if (_inputModule.Player1.UpKey.IsDown() && _tileModule.Scroll.Y > 0)
                _tileModule.Scroll.Y--;
            else if (_inputModule.Player1.DownKey.IsDown() && _tileModule.Scroll.Y < 255)
                _tileModule.Scroll.Y++;

            t++;
            if (t > 90)
            {
                d++;
                if (d > 3)
                    d = 0;

                t = 0;
            }

            var s = _spritesModule.GetSprite(0);
            if (d == 0)
                s.X++;
            else if (d == 1)
                s.Y++;
            else if (d == 2)
                s.X--;
            else if (d == 3)
                s.Y--;
        }

        public override void OnStartup()
        {
            new DiskNBitPlaneLoader()
              .Load(new DiskFile(ContentFolder.PatternTables, "tile8.pt"),
                  _romPatternTable);

            new DiskNBitPlaneLoader()
                .Load(
                    new DiskFile(ContentFolder.NameTables, "tile8.nt"),
                    _romNameTable);

            var tileModule = GameSystem.GetModule<TileModule>();
            var graphicsModule = GameSystem.CoreGraphicsModule;

            _romNameTable.CopyTo(tileModule.NameTable, GameSystem.Memory);
            _romPatternTable.CopyTo(graphicsModule.PatternTable, GameSystem.Memory);

            _spritesModule.Sprites[0].X = 40;
            _spritesModule.Sprites[0].Y = 16;
            _spritesModule.Sprites[0].Tile = 2;
        }

        public void OnVBlank()
        {
            _tileModule.OnVBlank();
            _spritesModule.OnVBlank();
        }

        public void OnHBlank()
        {
            _tileModule.OnHBlank();
            _spritesModule.OnHBlank();
        }

        public byte GetPalette(int pixel)
        {
            return 0;
        }
    }
}
