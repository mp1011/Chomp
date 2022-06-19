namespace ChompGame.Data
{
    public class ByteVector
    {
        private readonly GameByte _x, _y;

        public ByteVector(GameByte x, GameByte y)
        {
            _x = x;
            _y = y;
        }

        public int X
        {
            get => _x.Value - 128;
            set => _x.Value = (byte)(value + 128);
        }
        public int Y
        {
            get => _y.Value - 128;
            set => _y.Value = (byte)(value + 128);
        }
    }
}
