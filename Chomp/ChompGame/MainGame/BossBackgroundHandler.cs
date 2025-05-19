using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame.SpriteModels;
using System;

namespace ChompGame.MainGame
{
    public enum BackgroundEffectType : byte
    {
        None,
        SineWave,
        DissolveFromBottom,
        FadeFromTop,
        VerticalStretch,
        FinalBossLower,
        FinalBossLowerBeam
    }

    class BossBackgroundHandler : IHBlankHandler
    {
        private Specs _specs;
        private RasterInterrupts _rasterInterrupts;
        private ChompGameModule _gameModule;
        private CoreGraphicsModule _coreGraphicsModule;
        private TileModule _tileModule;
        private GameByteGridPoint _bossPosition;
        private GameByte _bossBgEffectValue;
        private LowNibble _finalBossLowerX;
        private HighNibble _finalBossLowerY;

        private GameByteEnum<BackgroundEffectType> _bossBgEffectType;
        private RandomModule _rng;
        private GameBit _showCoins;

        public bool ShowCoins
        {
            get => _showCoins.Value;
            set => _showCoins.Value = value;
        }

        public GameByteGridPoint BossPosition => _bossPosition;

        public byte BossBgEffectValue
        {
            get => _bossBgEffectValue.Value;
            set => _bossBgEffectValue.Value = value;
        }

        public byte FinalBossLowerX
        {
            get => _finalBossLowerX.Value;
            set => _finalBossLowerX.Value = value;
        }

        public byte FinalBossLowerY
        {
            get => _finalBossLowerY.Value;
            set => _finalBossLowerY.Value = value;
        }


        public BackgroundEffectType BossBgEffectType
        {
            get => _bossBgEffectType.Value;
            set => _bossBgEffectType.Value = value;
        }

        public BossBackgroundHandler(ChompGameModule gameModule, RasterInterrupts rasterInterrupts)
        {
            _gameModule = gameModule;
            _rasterInterrupts = rasterInterrupts;
            _specs = gameModule.Specs;
            _coreGraphicsModule = gameModule.GameSystem.CoreGraphicsModule;
            _tileModule = gameModule.TileModule;
            _rng = gameModule.RandomModule;
        }

        public void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _bossPosition = new GameByteGridPoint(memoryBuilder.AddByte(), memoryBuilder.AddByte(), 255, 255);
            _bossBgEffectValue = memoryBuilder.AddByte();

            _showCoins = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit7, memoryBuilder.Memory);
            _bossBgEffectType = new GameByteEnum<BackgroundEffectType>(memoryBuilder.AddMaskedByte(Bit.Right7));

            _finalBossLowerX = new LowNibble(_bossBgEffectValue.Address, memoryBuilder.Memory);
            _finalBossLowerY = new HighNibble(_bossBgEffectValue.Address, memoryBuilder.Memory);
        }

        public void OnHBlank()
        {
            if (!_gameModule.BossBackgroundHandling.Value)
                return;

            if (_bossBgEffectValue.Value == 255)
                return;

            var groundTiles = ShowCoins ? 4 : 2;
            var groundPosition = (byte)(_specs.ScreenHeight - (_specs.TileHeight * groundTiles));

            if (_gameModule.CurrentLevel == SceneModels.Level.Level5_27_Boss)
            {
                if (_coreGraphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
                    SetBossEyePriority(true);
                else if (_coreGraphicsModule.ScreenPoint.Y == groundPosition)
                    SetBossEyePriority(false);
            }

            if (_coreGraphicsModule.ScreenPoint.Y >= Constants.StatusBarHeight
                && _coreGraphicsModule.ScreenPoint.Y < groundPosition)
            {
                // WHEN Y=8, SCROLLY = 32
                // WHEN Y=12, SCROLLY = 28
                _tileModule.Scroll.X = (byte)(255 - _bossPosition.X);
                _tileModule.Scroll.Y = (byte)((4 * 15) - _bossPosition.Y);

                int realY = (_tileModule.Scroll.Y + _coreGraphicsModule.ScreenPoint.Y).NModByte(_specs.NameTablePixelHeight);
                if (realY < Constants.StatusBarHeight || realY >= _specs.NameTablePixelHeight - (_specs.TileHeight * groundTiles))
                {
                    _tileModule.Scroll.X = 0;
                    _tileModule.Scroll.Y = 0;
                }
            }

            if(_coreGraphicsModule.ScreenPoint.Y < groundPosition && _bossBgEffectValue.Value > 0)
            {
                if (BossBgEffectType == BackgroundEffectType.SineWave)
                    OnHBlank_UpdateSineWave();
                else if (BossBgEffectType == BackgroundEffectType.DissolveFromBottom)
                    OnHBlank_UpdateDissolveFromBottom();
                else if (BossBgEffectType == BackgroundEffectType.FadeFromTop)
                    OnHBlank_UpdateFadeFromTop();
                else if (BossBgEffectType == BackgroundEffectType.VerticalStretch)
                    OnHBlank_VerticalStretch();
                else if (BossBgEffectType == BackgroundEffectType.FinalBossLower)
                    OnHBlank_FinalBossLower();
                else if (BossBgEffectType == BackgroundEffectType.FinalBossLowerBeam)
                {
                    OnHBlank_FinalBossLower();
                    OnHBlank_FinalBossLowerBeam();
                }
            }

            if (_coreGraphicsModule.ScreenPoint.Y == groundPosition)
            {
                _tileModule.Scroll.X = _rasterInterrupts.RealScrollX;
                _tileModule.Scroll.Y = (byte)(_specs.ScreenHeight);
            }
        }

        private void SetBossEyePriority(bool value)
        {
            for(int i =0; i < _specs.MaxSprites; i++)
            {
                var sprite = _gameModule.SpritesModule.GetSprite(i);
                if (!sprite.Visible || sprite.Tile != 10)
                    continue;

                sprite.Priority = value;
            }            
        }

        private void OnHBlank_FinalBossLower()
        {
            var bottomScreenBegin = _bossPosition.Y - 94;
            if(_coreGraphicsModule.ScreenPoint.Y >= bottomScreenBegin &&
                _coreGraphicsModule.ScreenPoint.Y < bottomScreenBegin + 22)
            {
                _tileModule.Scroll.X -= (byte)(_finalBossLowerX.Value + 0);
                _tileModule.Scroll.Y -= (byte)(_finalBossLowerY.Value - 3);
            }
        }

        private void OnHBlank_FinalBossLowerBeam()
        {
            var beamStart = _bossPosition.Y - 100;
            int beamEnd = beamStart + _finalBossLowerY.Value + 4;

            int beamMid = beamStart + ((beamEnd - beamStart) / 2);

            if (_coreGraphicsModule.ScreenPoint.Y >= beamStart &&
                _coreGraphicsModule.ScreenPoint.Y < beamEnd)
            {
                var p = _coreGraphicsModule.GetBackgroundPalette(SceneModels.BgPalette.Background);

                int shade;
                if (_coreGraphicsModule.ScreenPoint.Y <= beamMid)
                    shade = _coreGraphicsModule.ScreenPoint.Y - beamStart;
                else
                    shade = beamEnd - _coreGraphicsModule.ScreenPoint.Y;

                p.SetColor(0, ColorIndex.Red(7 - shade).Value);
            }
            else if (_coreGraphicsModule.ScreenPoint.Y > beamEnd)
            {
                var p = _coreGraphicsModule.GetBackgroundPalette(SceneModels.BgPalette.Background);
                p.SetColor(0, ColorIndex.Black);

            }
        }

        private void OnHBlank_UpdateSineWave()
        {
            int effectY = _coreGraphicsModule.ScreenPoint.Y - _bossPosition.Y;
            if (effectY >= 0 && effectY <= 32)
            {
                double pct = (effectY + _bossBgEffectValue.Value) / 16.0;

                if (effectY.IsMod(2))
                    _tileModule.Scroll.X += (byte)(_bossBgEffectValue.Value * 1 * Math.Sin(pct * 2.0));
                else
                    _tileModule.Scroll.X -= (byte)(_bossBgEffectValue.Value * 1 * Math.Sin(pct * 2.0));
            }
        }

        private void OnHBlank_VerticalStretch()
        {
            if (_coreGraphicsModule.ScreenPoint.Y < Constants.StatusBarHeight)
                return;

            int effectY = _coreGraphicsModule.ScreenPoint.Y - _bossPosition.Y;
            if (effectY >= 0 && effectY <= 32)
            {
                var modVal = _bossBgEffectValue.Value;
                if (modVal > 8)
                    modVal = (byte)(modVal - 2*(modVal-8));

                var realY = (byte)((4 * 15) - _bossPosition.Y);
                var mod = (double)(modVal / 16.0);
                _tileModule.Scroll.Y = (byte)(realY + (effectY * mod));
                //_tileModule.Scroll.Y = (byte)(180 + (_bossBgEffectValue.Value % 4));
            //    _tileModule.Scroll.X += (byte)((_bossBgEffectValue.Value % 4));
            }
        }

        private void OnHBlank_UpdateDissolveFromBottom()
        {
            int effectY = _coreGraphicsModule.ScreenPoint.Y - _bossPosition.Y;
            if (effectY < 0 || effectY > 36)
                return;


            if(effectY == 32 - BossBgEffectValue - 3)
            {
                for (int i = 0; i < _specs.ScreenWidth; i++)
                {
                    if(_rng.RandomChance(10))
                        _coreGraphicsModule.BackgroundScanlineDrawBuffer[i] = 0;
                }
            }
            if (effectY == 32 - BossBgEffectValue - 2)
            {
                for (int i = 0; i < _specs.ScreenWidth; i++)
                {
                    if (_rng.RandomChance(50))
                        _coreGraphicsModule.BackgroundScanlineDrawBuffer[i] = 0;
                }
            }
            if (effectY == 32 - BossBgEffectValue - 1)
            {
                for (int i = 0; i < _specs.ScreenWidth; i++)
                {
                    if (_rng.RandomChance(90))
                        _coreGraphicsModule.BackgroundScanlineDrawBuffer[i] = 0;
                }
            }
            else if (effectY >= 32 - BossBgEffectValue)
            {
                for (int i = 0; i < _specs.ScreenWidth; i++)
                {
                    _coreGraphicsModule.BackgroundScanlineDrawBuffer[i] = 0;
                }
            }            
        }

        private void OnHBlank_UpdateFadeFromTop()
        {
            int effectY = _coreGraphicsModule.ScreenPoint.Y - _bossPosition.Y;
            if (effectY < -8 || effectY > 32)
                return;

            if (effectY <= (_bossBgEffectValue / 8))
            {
                if(((_bossBgEffectValue.Value / 8) - effectY) > 8)
                {
                    _tileModule.Scroll.Y = 0;
                }
                else if (effectY.IsMod(2))
                {
                    _tileModule.Scroll.X--;
                    _tileModule.Scroll.Y -= (byte)(_bossBgEffectValue % 8);
                }
                else
                {
                    _tileModule.Scroll.X++;
                    _tileModule.Scroll.Y -= (byte)((_bossBgEffectValue*2) % 8);
                }
            }
        }

        public void OnStartup() 
        {
        }
    }
}
