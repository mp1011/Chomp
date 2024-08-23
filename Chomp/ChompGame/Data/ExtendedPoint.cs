using ChompGame.Data.Memory;

namespace ChompGame.Data
{
    /// <summary>
    /// Note, leaves higher 6 bits of last byte free
    /// </summary>
    public class ExtendedPoint
    {
        private const int NegativePad = 64;
        public int Address => _x.Address;

        private ExtendedByte _x;
        private ExtendedByte _y;

        public ExtendedPoint(SystemMemoryBuilder memoryBuilder, GameBit extraX, GameBit extraY)
        {
            _x = new ExtendedByte(memoryBuilder.AddByte(), extraX);
            _y = new ExtendedByte(memoryBuilder.AddByte(), extraY);          
            X = 0;
            Y = 0;
        }

        public int X
        {
            get => _x.Value - NegativePad;
            set => _x.Value = value + NegativePad;
        }

        public int Y
        {
            get => _y.Value - NegativePad;
            set => _y.Value = value + NegativePad;
        }
    }
}
