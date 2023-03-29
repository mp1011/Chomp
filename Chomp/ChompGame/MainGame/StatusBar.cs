﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using System;

namespace ChompGame.MainGame
{
    class StatusBar
    {
        private readonly TileModule _tileModule;
        private readonly CoreGraphicsModule _coreGraphicsModule;
       
        private GameInteger _score;
        private LowNibble _lives;
        private HighNibble _health;
        private GameRAM _ram;
        private Specs _specs;

        private readonly byte _tileBlank = 8;
        private readonly byte _tileCap = 9;
        private readonly byte _tileEmpty = 10;
        private readonly byte _tileHalf = 11;
        private readonly byte _tileFull = 12;

        public int Score => (int)_score.Value;

        public byte Health
        {
            get => _health.Value;
            set
            {
                SetHealth(value);
            }
        }

        public StatusBar(ChompGameModule gameModule, GameRAM ram)
        {
            _ram = ram;
            _tileModule = gameModule.TileModule;
            _specs = _tileModule.Specs;
            _coreGraphicsModule = _tileModule.GameSystem.CoreGraphicsModule;
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
                   .SetFromString(@"0123000456730000" + Environment.NewLine + blankline);

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

                _tileModule.TileStartX = 0;
                _tileModule.TileStartY = Constants.StatusBarTopRow;
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

                _tileModule.TileStartX = 0;
                _tileModule.TileStartY = Constants.BgRow;
            }

            if (_tileModule.ScreenPoint.Y == 4)
            {
                _tileModule.TileStartX = 0;
                _tileModule.TileStartY = Constants.StatusBarBottomRow;
            }
        }

        public void OnStartup()
        {
        }

        public void SetLives(byte value)
        {
            _lives.Value = value;
            _tileModule.NameTable[0, 1] = GetDigitTile((char)('0' + value));
            _tileModule.NameTable[1, 1] = _tileBlank;

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

            _tileModule.NameTable[6, 1] = _tileCap;
            _tileModule.NameTable[7, 1] = _tileBlank;

        }

        public void AddToScore(uint value)
        {
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
                return (byte)(c -'0');
            else if (c == '8')
                return 14;
            else
                return 15;
        }
    }
}
