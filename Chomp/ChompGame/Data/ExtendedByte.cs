namespace ChompGame.Data
{
    class ExtendedByte
    {
        private GameByte _byte;
        private GameBit _bit;

        public int Address => _byte.Address;

        public ExtendedByte(GameByte gameByte, GameBit bit)
        {
            _byte = gameByte;
            _bit = bit;
        }

        public int Value
        {
            get => _byte.Value + (_bit.Value ? 256 : 0);          
            set
            {
                _byte.Value = (byte)value;
                _bit.Value = (value & 256) != 0;                
            }
        }
    }

    public class ExtendedByte2
    {
        private GameByte _byte;
        private TwoBit _extra;

        public ExtendedByte2(GameByte gameByte, TwoBit extra)
        {
            _extra = extra;
            _byte = gameByte;
        }

        public int Value
        {
            get => _byte.Value | (_extra.Value << 8);
            set
            {
                _byte.Value = (byte)value;
                _extra.Value = (byte)(value >> 8);
            }
        }
    }

}
