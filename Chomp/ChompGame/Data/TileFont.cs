using ChompGame.GameSystem;
using ChompGame.Graphics;

namespace ChompGame.Data
{
    class TileFont
    {
        private GameByte _charStartIndex;
        private CoreGraphicsModule _coreGraphicsModule;

        private Specs Specs => _coreGraphicsModule.GameSystem.Specs;

        public byte CharStartIndex
        {
            get => _charStartIndex.Value;
            set => _charStartIndex.Value = value;
        }

        public TileFont(
            SystemMemoryBuilder systemMemoryBuilder,
            CoreGraphicsModule coreGraphicsModule)
        {
            _coreGraphicsModule = coreGraphicsModule;
            _charStartIndex = systemMemoryBuilder.AddByte();
        }

        public void DrawString(
            string text,
            byte screenColumn,
            byte textRow)
        {
            var patternTableTilePoint = new ByteGridPoint(
                Specs.PatternTableTilesAcross,
                Specs.PatternTableTilesDown);

            var patternTablePoint = new ByteGridPoint(
                Specs.PatternTableWidth,
                Specs.PatternTableHeight);


            for(int i = 0; i < text.Length; i++)
            {
                //todo, only supporting 0-9 for now
                int charIndex = text[i] - '0';
                patternTableTilePoint.Index = _charStartIndex + charIndex;

                patternTablePoint.X = (byte)(patternTableTilePoint.X * Specs.TileWidth);
                patternTablePoint.Y = (byte)(patternTableTilePoint.Y * Specs.TileHeight + textRow);

                for (int col = 0; col < Specs.TileWidth; col++)
                {
                    _coreGraphicsModule.ScanlineDrawBuffer[screenColumn] = _coreGraphicsModule.PatternTable[patternTablePoint.Index];
                    patternTablePoint.X++;
                    screenColumn++;
                }
            }
        }
    }
}
