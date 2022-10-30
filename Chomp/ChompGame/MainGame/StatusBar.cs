using ChompGame.Data;
using ChompGame.GameSystem;

namespace ChompGame.MainGame
{
    class StatusBar : IHBlankHandler
    {
        private readonly TileModule _tileModule;
        private readonly CoreGraphicsModule _coreGraphicsModule;
        private GameInteger _score;
        private LowNibble _lives;
        private HighNibble _health;

        private readonly byte _tileEmpty = 9;
        private readonly byte _tileHalf = 10;
        private readonly byte _tileFull = 11;

        public byte Health
        {
            get => _health.Value;
            set
            {
                SetHealth(value);
            }
        }

        public StatusBar(TileModule tileModule)
        {
            _tileModule = tileModule;
            _coreGraphicsModule = _tileModule.GameSystem.CoreGraphicsModule;
        }

        public void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _score = memoryBuilder.AddInteger();
            _lives = new LowNibble(memoryBuilder);
            _health = new HighNibble(memoryBuilder);
            memoryBuilder.AddByte();
        }

        public void OnHBlank()
        {
            if (_tileModule.ScreenPoint.Y == 0)
            {
                _tileModule.TileStartX = 0;
                _tileModule.TileStartY = 4;

                var bgPalette = _coreGraphicsModule.GetPalette(0);
                bgPalette.SetColor(0, ChompGameSpecs.Black);
                bgPalette.SetColor(1, ChompGameSpecs.Blue1);
                bgPalette.SetColor(2, ChompGameSpecs.White);
                bgPalette.SetColor(3, ChompGameSpecs.Green2);
            }

            if (_tileModule.ScreenPoint.Y == 4)
            {
                _tileModule.TileStartX = 0;
                _tileModule.TileStartY = 5;
            }
        }

        public void OnStartup()
        {
        }

        public void SetLives(byte value)
        {
            _lives.Value = value;
            _tileModule.NameTable[0, 1] = GetDigitTile((char)('0' + value));
        }

        private void SetHealth(byte value)
        {
            _health.Value = value;

            int full = value / 2;
            bool hasHalf = (value % 2) != 0;

            for(int i = 0; i < 4; i++)
            {
                if (full > 0)
                {
                    _tileModule.NameTable[2 + i, 1] = _tileFull;
                    full--;
                }
                else if(hasHalf)
                {
                    _tileModule.NameTable[2 + i, 1] = _tileHalf;
                    hasHalf = false;
                }
                else
                    _tileModule.NameTable[2 + i, 1] = _tileEmpty;
            }

            _tileModule.NameTable[6, 1] = 8;
        }

        public void AddToScore(uint value)
        {
            _score.Value += value;

            string scoreText = _score.Value.ToString("00000000");
            int digitStart = 8;

            for (int digit = 0; digit < scoreText.Length; digit++)
            {
                _tileModule.NameTable[digitStart+digit, 1] = GetDigitTile(scoreText[digit]);
            }
        }

        private byte GetDigitTile(char c)
        {
            if (c <= '7')
                return (byte)(c -'0');
            else if (c == '8')
                return 14;
            else
                return 15;
        }
    }
}
