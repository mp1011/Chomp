using ChompGame.Data.Memory;

namespace ChompGame.Data
{
    public class ExtendedPoint
    {
        public int Address => _xByte.Address;

        private GameByte _xByte, _yByte;
        private NibblePoint _extra;

        public ExtendedPoint(SystemMemoryBuilder memoryBuilder)
        {
            _xByte = memoryBuilder.AddByte();
            _yByte = memoryBuilder.AddByte();
            _extra = memoryBuilder.AddNibblePoint();
        }

        public int X
        {
            get => _xByte.Value + (_extra.X * 256);
            set
            {
                _extra.X = (byte)(value / 256);
                _xByte.Value = (byte)(value % 256);
            }
        }

        public int Y
        {
            get => _yByte.Value + (_extra.Y * 256);
            set
            {
                _extra.Y = (byte)(value / 256);
                _yByte.Value = (byte)(value % 256);
            }
        }
    }
}
