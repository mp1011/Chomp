namespace ChompGame.Data
{
    public class ByteRectangle
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Width { get; set; }
        public byte Height { get; set; }

        public byte Right => (byte)(X + Width);
        public byte Bottom => (byte)(Y + Height);

        public ByteRectangle() { }
        public ByteRectangle(byte x, byte y, byte width, byte height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public ByteRectangle(int x, int y, int width, int height)
        {
            X = (byte)x;
            Y = (byte)y;
            Width = (byte)width;
            Height = (byte)height;
        }


        public bool Contains(byte x, byte y)
        {
            return x >= X
                && x < Right
                && y >= Y
                && y < Bottom;
        }

        public bool Intersects(ByteRectangle other)
        {
            if (other.Right < X
                || other.X >= Right
                || other.Bottom < Y
                || other.Y >= Bottom)
                return false;

            return true;
        }
    }
}
