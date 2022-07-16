using ChompGame.Data;

namespace ChompGame.GameSystem
{
    public class TileModule : ScanlineGraphicsModule
    {
        public NBitPlane NameTable { get; private set; }

        public TileModule(MainSystem gameSystem) : base(gameSystem) 
        { 
        }

        public override void OnStartup()
        {
           
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            base.BuildMemory(builder);
            NameTable = builder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);
        }

        public override void OnHBlank()
        {
            _coreGraphicsModule.ScanlineDrawCommands[Layer].BeginAddDrawInstructions();
            PatternTablePoint.Reset();
            
            var commands = _coreGraphicsModule.ScanlineDrawCommands[Layer];
            commands.AddAttributeChangeCommand(0, false, false);

            var nameTablePoint = new ByteGridPoint(Specs.NameTableWidth, Specs.NameTableHeight);
            nameTablePoint.X = (byte)(Scroll.X / Specs.TileHeight);
            nameTablePoint.Y = (byte)((ScreenPoint.Y + Scroll.Y) / Specs.TileHeight);

            int row = (ScreenPoint.Y + Scroll.Y) % Specs.TileHeight;
            int col = (ScreenPoint.X + Scroll.X) % Specs.TileWidth;
            var tilePoint = new ByteGridPoint(Specs.PatternTableTilesAcross, Specs.PatternTableTilesDown);
            var nextPatternTablePoint = new ByteGridPoint(Specs.PatternTableWidth, Specs.PatternTableHeight);

            int colsRemaining = Specs.ScreenWidth / Specs.TileWidth;
            if (col != 0)
                colsRemaining++;

            int screenColumn = 0;

            while (colsRemaining-- > 0)
            {
                tilePoint.Index = NameTable[nameTablePoint.Index];
                nextPatternTablePoint.X = (byte)((tilePoint.X * Specs.TileWidth) + col);
                nextPatternTablePoint.Y = (byte)((tilePoint.Y * Specs.TileHeight) + row);

                _coreGraphicsModule.ScanlineDrawCommands[Layer].AddTileMoveCommand(
                    pixelDestination: nextPatternTablePoint.Index,
                    currentPixelIndex: PatternTablePoint.Index);

                PatternTablePoint.Index = nextPatternTablePoint.Index;
                var hold = Specs.TileWidth - col;
                screenColumn += hold;

                _coreGraphicsModule.ScanlineDrawCommands[0].AddDrawCommand(moveIndex: true, amount: (byte)hold);
                PatternTablePoint.Advance(hold);
                col = 0;

                nameTablePoint.NextColumn();
            }

            PatternTablePoint.X = 0;
            PatternTablePoint.Y = 0;
            DrawHoldCounter.Value = 0;

            DrawInstructionAddressOffset.Value = 255;
            PatternTablePoint.Reset();
        }

        public override void OnVBlank()
        {
            PatternTablePoint.X = 0;
            PatternTablePoint.Y = 0;
            DrawHoldCounter.Value = 0;
            DrawInstructionAddressOffset.Value = 0;
        }
    }
}
