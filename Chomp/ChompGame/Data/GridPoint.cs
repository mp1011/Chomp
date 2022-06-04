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

        public bool Next() 
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

        public bool NextColumn()
        {
            if (X == Width - 1)
                return false;
            else
            {
                X++;
                return true;
            }
        }

        public override string ToString() => $"{X} {Y}";

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

        public override byte X { get; set; }
        public override byte Y { get; set; }

        public ByteGridPoint(GridPoint copyFrom) : this(copyFrom.Width, copyFrom.Height)
        {
            X = copyFrom.X;
            Y = copyFrom.Y;
        }
    }

    public class GameByteGridPoint : GridPoint
    {
        private GameByte _xByte, _yByte;

        public GameByteGridPoint(GameByte xByte, GameByte yByte, byte width, byte height) :  base(width,height)
        {
            _xByte = xByte;
            _yByte = yByte;
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
