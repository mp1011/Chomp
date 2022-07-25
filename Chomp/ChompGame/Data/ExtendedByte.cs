using ChompGame.GameSystem;

namespace ChompGame.Data
{
    class ExtendedByte
    {
        private int _extra;
        private GameByte _byte;
        private GameBit _bit;

        public ExtendedByte(GameByte gameByte, GameBit bit, int extra)
        {
            _extra = extra;
            _byte = gameByte;
            _bit = bit;
        }

        public int Value
        {
            get => _byte.Value + (_bit.Value ? _extra : 0);          
            set
            {
                if (value < 0)
                    value += _extra * 2;

                if (value >= _extra * 2)
                    value -= _extra * 2;

                _bit.Value = value >= _extra;
                if (_bit.Value)
                    value -= _extra;

                _byte.Value = (byte)value;
            }
        }
    }
}
