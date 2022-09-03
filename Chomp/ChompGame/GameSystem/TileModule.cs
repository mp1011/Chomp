using ChompGame.Data;

namespace ChompGame.GameSystem
{
    public class TileModule : ScanlineGraphicsModule
    {
        public NBitPlane NameTable { get; private set; }

        public TileModule(MainSystem gameSystem) : base(gameSystem) 
        { 
        }

        public GameByte BackgroundPaletteIndex { get; private set; }

        public override void OnStartup()
        {
           
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            base.BuildMemory(builder);
            BackgroundPaletteIndex = builder.AddByte();
            NameTable = builder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);
        }

        public override void OnHBlank()
        {
            var nameTablePoint = new ByteGridPoint(
                Specs.NameTableWidth, 
                Specs.NameTableHeight);

            var patternTableTilePoint = new ByteGridPoint(
                Specs.PatternTableTilesAcross, 
                Specs.PatternTableTilesDown);

            var patternTablePoint = new ByteGridPoint(
                Specs.PatternTableWidth,
                Specs.PatternTableHeight);

            nameTablePoint.X = (byte)(Scroll.X / Specs.TileWidth);
            nameTablePoint.Y = (byte)(ScreenPoint.Y / Specs.TileHeight); // todo, scroll

            patternTableTilePoint.Index = NameTable[nameTablePoint.Index];
          
            int col = Scroll.X % Specs.TileWidth;
            int row = ScreenPoint.Y % Specs.TileHeight; //todo, scroll
            int remainingTilePixels = Specs.TileWidth - col;

            patternTablePoint.X = (byte)(patternTableTilePoint.X * Specs.TileWidth + col);
            patternTablePoint.Y = (byte)(patternTableTilePoint.Y * Specs.TileHeight + row);

            for (int i =0; i < Specs.ScreenWidth; i++)
            {
                _coreGraphicsModule.ScanlineDrawBuffer[i] = _coreGraphicsModule.PatternTable[patternTablePoint.Index];

                remainingTilePixels--;
                if(remainingTilePixels == 0)
                {
                    nameTablePoint.X++;
                    patternTableTilePoint.Index = NameTable[nameTablePoint.Index];
                    remainingTilePixels = Specs.TileWidth;
                    patternTablePoint.X = (byte)(patternTableTilePoint.X * Specs.TileWidth);
                    patternTablePoint.Y = (byte)(patternTableTilePoint.Y * Specs.TileHeight + row);
                }
                else
                {
                    patternTablePoint.X++;
                }
            }          
        }

        public override void OnVBlank()
        {
        }
    }
}
