namespace ChompGame.Data
{

    public abstract class ByteRectangle
    {
        public abstract byte X { get; set; }
        public abstract byte Y { get; set; }
        public abstract byte Width { get; set; }
        public abstract byte Height { get; set; }

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
            if (other.Right <= X
                || other.X >= Right
                || other.Bottom <= Y
                || other.Y >= Bottom)
                return false;

            return true;
        }

        public void CopyFrom(ByteRectangle other)
        {
            X = other.X;
            Y = other.Y;
            Width = other.Width;
            Height = other.Height;
        }
    }

    public class InMemoryByteRectangle
        : ByteRectangle
    {
        public override byte X { get; set; }
        public override byte Y { get; set; }
        public override byte Width { get; set; }
        public override byte Height { get; set; }

        public InMemoryByteRectangle(byte x=0, byte y=0, byte width=0, byte height=0)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public InMemoryByteRectangle(int x = 0, int y = 0, int width = 0, int height = 0)
        {
            X = (byte)x;
            Y = (byte)y;
            Width = (byte)width;
            Height = (byte)height;
        }
    }

    public class NibbleRectangle
        : ByteRectangle
    {

        private readonly NibbleArray _bounds;

        public NibbleRectangle(NibbleArray bounds)
        {
            _bounds = bounds;
        }

        public NibbleRectangle(int address, SystemMemory systemMemory)
        {
            _bounds = new NibbleArray(address, systemMemory);
        }

        public override byte X
        {
            get => _bounds[0];
            set => _bounds[0] = value;
        }

        public override byte Y
        {
            get => _bounds[1];
            set => _bounds[1] = value;
        }

        public override byte Width
        {
            get => _bounds[2];
            set => _bounds[2] = value;
        }

        public override byte Height
        {
            get => _bounds[3];
            set => _bounds[3] = value;
        }
    }
}
