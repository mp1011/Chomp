using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using System;

namespace ChompGame.MainGame
{
    class StatusBar
    {
        public const int FullHealth = GameDebug.OneHp ? 1 : 8;
        public const int InitialLives = 3;
        public const int MaxLives = 9;

        private readonly TileModule _tileModule;
        private readonly CoreGraphicsModule _coreGraphicsModule;
        private readonly ChompGameModule _gameModule;
        private readonly RewardsModule _rewardsModule;

        private GameInteger _score;
        private LowNibble _lives;
        private HighNibble _health;
        private GameRAM _ram;
        private Specs _specs;

        private readonly byte _tileBlank = 56;
        private readonly byte _tileCap = 57;
        private readonly byte _tileEmpty = 58;
        private readonly byte _tileHalf = 59;
        private readonly byte _tileFull = 60;

        public int ScorePtr => _score.Address;

        public int Score
        {
            get => (int)_score.Value;
            set => _score.Value = (uint)value;
        }

        public byte Health
        {
            get => _health.Value;
            set
            {
                SetHealth(value);
            }
        }

        public byte Lives
        {
            get => _lives.Value;
            set => SetLives(value);
        }

        public StatusBar(ChompGameModule gameModule, GameRAM ram)
        {
            _gameModule = gameModule;
            _ram = ram;
            _tileModule = gameModule.TileModule;
            _specs = _tileModule.Specs;
            _coreGraphicsModule = _tileModule.GameSystem.CoreGraphicsModule;
            _rewardsModule = gameModule.RewardsModule;
        }

        public void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _score = memoryBuilder.AddInteger();
            _lives = new LowNibble(memoryBuilder);
            _health = new HighNibble(memoryBuilder);
            memoryBuilder.AddByte();
        }

        public void InitializeTiles()
        {
            var blankline = "".PadRight(16, 'C');
            _tileModule.NameTable
                   .SetFromString(40, @"0123000456730000" + Environment.NewLine + blankline);

            DrawScore();
            SetLives(_lives.Value);
            SetHealth(Health);
        }

        public void OnHBlank(GameByte realScrollX, GameByte realScrollY)
        {
            if (_tileModule.ScreenPoint.Y == 0)
            {
                realScrollX.Value = _tileModule.Scroll.X;
                realScrollY.Value = _tileModule.Scroll.Y;
            }

            if (_tileModule.ScreenPoint.Y < 8)
            {
                _tileModule.Scroll.X = 0;
                _tileModule.Scroll.Y = 0;
            }
            else if (_tileModule.ScreenPoint.Y == 8)
            {
                _tileModule.Scroll.X = realScrollX.Value;
                _tileModule.Scroll.Y = realScrollY.Value;
            }
        }

        public void OnStartup()
        {
        }

        public void SetLives(byte value)
        {
            if(value > MaxLives)
                value = MaxLives;

            _lives.Value = value;
            _tileModule.NameTable[0, 1] = GetDigitTile((char)('0' + value));
            _tileModule.NameTable[1, 1] = _tileBlank;

        }

        private void SetHealth(byte value)
        {
            if (GameDebug.InfiniteHp)
                value = FullHealth;

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

            _tileModule.NameTable[6, 1] = _tileCap;
            _tileModule.NameTable[7, 1] = _tileBlank;

        }

        public void AddToScore(uint value)
        {
            if(_lives.Value < 9 && _rewardsModule.CheckExtraLife(_score.Value, _score.Value + value))
            {
                _lives.Value++;
                SetLives(_lives.Value);
            }
            _score.Value += value;
            DrawScore();
        }

        private void DrawScore()
        {
            string scoreText = _score.Value.ToString("00000000");
            int digitStart = 8;

            for (int digit = 0; digit < scoreText.Length; digit++)
            {
                _tileModule.NameTable[digitStart + digit, 1] = GetDigitTile(scoreText[digit]);
            }
        }

        private byte GetDigitTile(char c)
        {
            if (c <= '7')
                return (byte)(48 + (c -'0'));
            else if (c == '8')
                return 62;
            else
                return 63;
        }
    }
}
