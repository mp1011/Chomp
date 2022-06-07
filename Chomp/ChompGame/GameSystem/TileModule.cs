using ChompGame.Data;
using ChompGame.ROM;

namespace ChompGame.GameSystem
{
    public class TileModule : Module, IHBlankHandler, IVBlankHandler
    {
        private CoreGraphicsModule _coreGraphicsModule => GameSystem.CoreGraphicsModule;

        public GameByteGridPoint Scroll { get; private set; }
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
            Scroll = builder.AddGridPoint(
                (byte)(Specs.NameTableWidth * Specs.TileWidth),
                (byte)(Specs.NameTableHeight * Specs.TileHeight), 
                Specs.ScrollXMask, 
                Specs.ScrollYMask);
            
            NameTable = builder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);
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

            var nameTablePoint = new ByteGridPoint(Specs.NameTableWidth, Specs.NameTableHeight);
            nameTablePoint.X = (byte)(Scroll.X / Specs.TileHeight);
            nameTablePoint.Y = (byte)((ScreenPoint.Y+Scroll.Y) / Specs.TileHeight);

            int row = (ScreenPoint.Y + Scroll.Y) % Specs.TileHeight;
            int col = (ScreenPoint.X + Scroll.X) % Specs.TileWidth;
            var tilePoint = new ByteGridPoint(Specs.PatternTableTilesAcross, Specs.PatternTableTilesDown);
            var nextPatternTablePoint = new ByteGridPoint(Specs.PatternTableWidth, Specs.PatternTableHeight);
 
            int colsRemaining = Specs.ScreenWidth / Specs.TileWidth;
            if (col != 0)
                colsRemaining++;

            while(colsRemaining-- > 0)
            {
                tilePoint.Index = NameTable[nameTablePoint.Index];
                nextPatternTablePoint.X = (byte)((tilePoint.X * Specs.TileWidth) + col);
                nextPatternTablePoint.Y = (byte)((tilePoint.Y * Specs.TileHeight) + row);

                PatternTablePoint.Advance(_coreGraphicsModule.AddMoveBrushToCommand(
                    destination: nextPatternTablePoint.Index,
                    currentOffset: PatternTablePoint.Index));

                var hold = Specs.TileWidth - col;
                PatternTablePoint.Advance(_coreGraphicsModule.AddDrawHoldCommand(hold));
                col = 0;

                nameTablePoint.NextColumn();
            }

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
