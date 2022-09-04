using ChompGame.GameSystem;

namespace ChompGame.Data
{
    public class PatternTablePoint
    {
        private Specs _specs;
        private ByteGridPoint _tilePoint;
        private ByteGridPoint _pixelPoint;

        public int TileIndex
        {
            get => _tilePoint.Index;
            set
            {
                _tilePoint.Index = value;
                _pixelPoint.X = (byte)(_tilePoint.X * _specs.TileWidth);
                _pixelPoint.Y = (byte)(_tilePoint.Y * _specs.TileHeight);
            }
        }

        public int PixelIndex
        {
            get => _pixelPoint.Index;
            set => _pixelPoint.Index = value;
        }

        public byte Y
        {
            get => _pixelPoint.Y;
            set => _pixelPoint.Y = value;
        }

        public PatternTablePoint(Specs specs)
        {
            _specs = specs;

            _tilePoint = new ByteGridPoint(
              specs.PatternTableTilesAcross,
              specs.PatternTableTilesDown);

            _pixelPoint = new ByteGridPoint(
                specs.PatternTableWidth,
                specs.PatternTableHeight);
        }
       
    }
}
