using ChompGame.Data;
using ChompGame.Helpers;
using ChompGame.ROM;
using System;

namespace ChompGame.GameSystem
{
    class PlatformerModule : Module, IMasterModule
    {

        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private readonly AudioModule _audioModule;
        private readonly TileModule _tileModule;

        private NBitPlane _romPatternTable;
        private NBitPlane _romNameTable;

             
        public PlatformerModule(MainSystem mainSystem, InputModule inputModule, AudioModule audioModule, 
            SpritesModule spritesModule, TileModule tileModule) 
            : base(mainSystem)
        {
            _audioModule = audioModule;
            _inputModule = inputModule;
            _spritesModule = spritesModule;
            _tileModule = tileModule;
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _romPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            _romNameTable = memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);
        }

        public override void OnStartup()
        {
            _tileModule.Layer = 0;
            _spritesModule.Layer = 1;

            new DiskNBitPlaneBitmapLoader(GameSystem)
              .Load(new DiskFile(ContentFolder.PatternTables, "platformer_pt.bmp"),
                  _romPatternTable);

            new DiskNBitPlaneLoader()
             .Load(new DiskFile(ContentFolder.NameTables, "platformer.nt"),
                 _romNameTable);


            var graphicsModule = GameSystem.CoreGraphicsModule;
            _romPatternTable.CopyTo(graphicsModule.PatternTable, GameSystem.Memory);
            _romNameTable.CopyTo(_tileModule.NameTable, GameSystem.Memory);

            var palette = graphicsModule.GetCurrentPalette();
            palette.SetColor(0, 13);
            palette.SetColor(1, 7);
            palette.SetColor(2, 8);
            palette.SetColor(3, 9);
        }


        public void OnLogicUpdate()
        {
            _audioModule.OnLogicUpdate();
            _inputModule.OnLogicUpdate();
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
    }
}
