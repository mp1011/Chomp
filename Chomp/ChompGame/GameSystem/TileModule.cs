using ChompGame.Data;
using ChompGame.ROM;

namespace ChompGame.GameSystem
{
    public class TileModule : Module, IHBlankHandler, IVBlankHandler
    {
        private CoreGraphicsModule _coreGraphicsModule => GameSystem.CoreGraphicsModule;

        public GameByteGridPoint PatternTablePoint => _coreGraphicsModule.PatternTablePoint;
        public GameByte DrawInstructionAddressOffset => _coreGraphicsModule.DrawInstructionAddressOffset;
        public GameByte DrawHoldCounter => _coreGraphicsModule.DrawHoldCounter;
        public GameByteGridPoint ScreenPoint => _coreGraphicsModule.ScreenPoint;
        public NBitPlane NameTable { get; private set; }

        public TileModule(MainSystem gameSystem) : base(gameSystem) 
        { 
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            NameTable = builder.AddNBitPlane(2, Specs.NameTableWidth, Specs.NameTableHeight);
        }

        public override void OnStartup()
        {
            var nameTableLoader = new DiskNBitPlaneLoader();
            nameTableLoader.Load(
               new DiskFile(ContentFolder.NameTables, "test.nt"),
               NameTable);
        }

        public void OnHBlank()
        {
            DrawInstructionAddressOffset.Value = 0;
            PatternTablePoint.Reset();
            var pt = new ByteGridPoint(PatternTablePoint);

            var nameTablePoint = new ByteGridPoint(Specs.NameTableWidth, Specs.NameTableHeight);
            nameTablePoint.X = 0;
            nameTablePoint.Y = (byte)(ScreenPoint.Y / Specs.TileHeight);

            int row = ScreenPoint.Y % Specs.TileHeight;

            var tilePoint = new ByteGridPoint(Specs.PatternTableTilesAcross, Specs.PatternTableTilesDown);
            var nextPatternTablePoint = new ByteGridPoint(Specs.PatternTableWidth, Specs.PatternTableHeight);

            do
            {
                tilePoint.Index = NameTable[nameTablePoint.Index];
                nextPatternTablePoint.X = (byte)(tilePoint.X * Specs.TileWidth);
                nextPatternTablePoint.Y = (byte)((tilePoint.Y * Specs.TileHeight) + row);

                PatternTablePoint.Advance(_coreGraphicsModule.AddMoveBrushToCommand(
                    destination: nextPatternTablePoint.Index,
                    currentOffset: PatternTablePoint.Index));

                PatternTablePoint.Advance(_coreGraphicsModule.AddDrawHoldCommand(Specs.TileWidth));
            }
            while (nameTablePoint.NextColumn());

            PatternTablePoint.X = 0;
            PatternTablePoint.Y = 0;
            DrawHoldCounter.Value = 0;
            DrawInstructionAddressOffset.Value = 0;
        }

        public void OnVBlank()
        {
            PatternTablePoint.X = 0;
            PatternTablePoint.Y = 0;
            DrawHoldCounter.Value = 0;
            DrawInstructionAddressOffset.Value = 0;
        }
    }
}
