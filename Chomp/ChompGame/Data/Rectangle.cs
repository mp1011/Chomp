using ChompGame.Data.Memory;

namespace ChompGame.Data
{

    public abstract class ByteRectangleBase
    {
        public abstract byte X { get; set; }
        public abstract byte Y { get; set; }
        public abstract byte Width { get; set; }
        public abstract byte Height { get; set; }

        public byte Right => (byte)(X + Width);
        public byte Bottom => (byte)(Y + Height);

        public ByteRectangleBase() { }
        public ByteRectangleBase(byte x, byte y, byte width, byte height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public ByteRectangleBase(int x, int y, int width, int height)
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

        public bool Intersects(ByteRectangleBase other)
        {
            if (other.Right <= X
                || other.X >= Right
                || other.Bottom <= Y
                || other.Y >= Bottom)
                return false;

            return true;
        }

        public void CopyFrom(ByteRectangleBase other)
        {
            X = other.X;
            Y = other.Y;
            Width = other.Width;
            Height = other.Height;
        }
    }

    public class InMemoryByteRectangle
        : ByteRectangleBase
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
        : ByteRectangleBase
    {

        private readonly NibbleArray _bounds;

        public NibbleRectangle(NibbleArray bounds)
        {
            _bounds = bounds;
        }

        public NibbleRectangle(SystemMemoryBuilder memoryBuilder)
        {             
            _bounds = new NibbleArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(2);
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

    public class ByteRectangle : ByteRectangleBase
    {
        private readonly GameByte _x, _y, _width, _height;

        public ByteRectangle(SystemMemoryBuilder memoryBuilder)
        {
            _x = memoryBuilder.AddByte();
            _y = memoryBuilder.AddByte();
            _width = memoryBuilder.AddByte();
            _height = memoryBuilder.AddByte();
        }

        public ByteRectangle(int address, SystemMemory memory)
        {
            _x = new GameByte(address, memory);
            _y = new GameByte(address + 1, memory);
            _width = new GameByte(address + 2, memory);
            _height = new GameByte(address + 3, memory);
        }

        public override byte X
        {
            get => _x.Value;
            set => _x.Value = value;
        }

        public override byte Y
        {
            get => _y.Value;
            set => _y.Value = value;
        }

        public override byte Width
        {
            get => _width.Value;
            set => _width.Value = value;
        }

        public override byte Height
        {
            get => _height.Value;
            set => _height.Value = value;
        }
    }
}
