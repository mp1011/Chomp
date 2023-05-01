namespace ChompGame.Graphics
{
    enum GameColor
    {
        DarkGray,
        LightGray,
        BlueGray,
        Blue,
        Red,
        Brown,
        Green,
        Purple
    }
    struct ColorIndex
    {
        private const byte Cols = 8;

        public byte Value { get; }

        public byte ColorColumn => (byte)(Value % Cols);

        public byte ColorRow => (byte)(Value / Cols);

        public GameColor Color => (GameColor)ColorRow;

        public ColorIndex(byte value)
        {
            Value = value;
        }

        public ColorIndex(int value) : this((byte)value) { }

        public ColorIndex(GameColor color, int shade) : this ((byte)(((byte)color * Cols) + shade))
        {

        }

        public ColorIndex Lighter()
        {
            if (ColorColumn < Cols - 1)
                return new ColorIndex(Value + 1);
            else
                return this;
        }

        public ColorIndex LighterCycle()
        {
            if (ColorColumn < Cols - 1)
                return new ColorIndex(Value + 1);
            else
                return new ColorIndex(ColorRow * Cols); 
        }

        public ColorIndex Darker()
        {
            if (ColorColumn > 0)
                return new ColorIndex(Value - 1);
            else if (ColorRow == (byte)GameColor.LightGray)
                return DarkGray(7);
            else
                return new ColorIndex(0);
        }

        public ColorIndex ToGrayscale()
        {
            return new ColorIndex(ColorColumn);
        }

        public static ColorIndex DarkGray(int shade) => new ColorIndex(Graphics.GameColor.DarkGray, shade);
        public static ColorIndex LightGray(int shade) => new ColorIndex(Graphics.GameColor.LightGray, shade);
        public static ColorIndex Blue(int shade) => new ColorIndex(Graphics.GameColor.Blue, shade);
        public static ColorIndex BlueGray(int shade) => new ColorIndex(Graphics.GameColor.BlueGray, shade);
        public static ColorIndex Brown(int shade) => new ColorIndex(Graphics.GameColor.Brown, shade);
        public static ColorIndex Red(int shade) => new ColorIndex(Graphics.GameColor.Red, shade);
        public static ColorIndex Green(int shade) => new ColorIndex(Graphics.GameColor.Green, shade);
        public static ColorIndex Purple(int shade) => new ColorIndex(Graphics.GameColor.Purple, shade);

        public static byte LightBlue => Blue(7).Value;
        public static byte Black => DarkGray(0).Value;

        public static byte Gray1 => DarkGray(3).Value;
        public static byte Gray2 => DarkGray(4).Value;
        public static byte Gray3 => DarkGray(5).Value;

        public static byte White => LightGray(7).Value;
        public static byte Gold => Red(4).Value;

        public static byte Green1 => Green(1).Value;
        public static byte Green2 => Green(3).Value;
        public static byte Green3 => Green(4).Value;

        public static byte Red1 => Red(0).Value;
        public static byte Red2 => Red(1).Value;
        public static byte Red3 => Red(2).Value;
        public static byte Orange => Brown(5).Value;
        public static byte DarkBrown => Brown(0).Value;


        public static byte BlueGray1 => BlueGray(0).Value;
        public static byte BlueGray2 => BlueGray(2).Value;

        public static byte Blue1 => Blue(0).Value;
        public static byte LightTan => Brown(7).Value;
        public static byte LightYellow => Red(7).Value;

    }
}
