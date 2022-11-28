using ChompGame.Extensions;

namespace ChompGame.Data
{
    public abstract class GridPoint
    {
        public abstract byte X { get; set; }
        public abstract byte Y { get; set; }

        public byte Width { get; }
        public byte Height { get; }    

        protected GridPoint(byte width, byte height)
        {
            Width = width;
            Height = height;
        }

        public int Index
        {
            get => (Y * Width) + X;
            set
            {
                Y = (byte)(value / Width);
                X = (byte)(value % Width);
            }
        }

        public void Advance(int amount)
        {
            var newIndex = Index + amount;
            newIndex = newIndex.Wrap(Width * Height);
            Index = newIndex;
        }

        public void Advance(int steps, int extraRowSkip)
        {
            while(steps-- > 0)
            {
                if(Next())
                {
                    Y += (byte)extraRowSkip;
                }
            }
        }

        public virtual bool Next() 
        {
            if (X == Width - 1)
            {
                X = 0;
                if (Y == Height - 1)
                    Y = 0;
                else
                    Y++;

                return true;
            }
            else
                X++;

            return false;
        }

        public void NextColumn()
        {
            if (X == Width - 1)
                X = 0;
            else
                X++;
        }

        public override string ToString() => $"{X},{Y} ({Index})";

        public void Reset()
        {
            X = 0;
            Y = 0;
        }
    }

    public class ByteGridPoint : GridPoint
    {
        public ByteGridPoint(byte width, byte height) : base(width, height)
        {
        }

        public ByteGridPoint(int width, int height) : base((byte)width, (byte)height)
        {
        }

        private byte _x;
        private byte _y;

        public override byte X
        {
            get => _x;
            set => _x = (byte)(value % Width);
        }

        public override byte Y 
        {
            get => _y;
            set => _y = (byte)(value % Height);
        }

        public ByteGridPoint(GridPoint copyFrom) : this(copyFrom.Width, copyFrom.Height)
        {
            X = copyFrom.X;
            Y = copyFrom.Y;
        }
    }

    public class GameByteGridPoint : GridPoint
    {
        public int Address => _xByte.Address;

        private GameByte _xByte, _yByte;

        public GameByteGridPoint(GameByte xByte, GameByte yByte, byte width, byte height) :  base(width,height)
        {
            _xByte = xByte;
            _yByte = yByte;
        }

        public GameByteGridPoint(int address, SystemMemory memory, byte width, byte height) 
            :base(width,height)
        {
            _xByte = new GameByte(address, memory);
            _yByte = new GameByte(address + 1, memory);
        }

        public override byte X
        {
            get => _xByte.Value;
            set => _xByte.Value = value;
        }

        public override byte Y
        {
            get => _yByte.Value;
            set => _yByte.Value = value;
        }
    }
}
