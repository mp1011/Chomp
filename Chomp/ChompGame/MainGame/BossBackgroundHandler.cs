using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using System;

namespace ChompGame.MainGame
{
    class BossBackgroundHandler : IHBlankHandler
    {
        private Specs _specs;
        private RasterInterrupts _rasterInterrupts;
        private ChompGameModule _gameModule;
        private CoreGraphicsModule _coreGraphicsModule;
        private TileModule _tileModule;
        private GameByteGridPoint _bossPosition;
        private GameByte _bossBackgroundEnd;
        private GameByte _bossDeathTimer;

        public GameByteGridPoint BossPosition => _bossPosition;
        public GameByte BossBackgroundEnd => _bossBackgroundEnd;

        public GameByte BossDeathTimer => _bossDeathTimer;

        public BossBackgroundHandler(ChompGameModule gameModule, RasterInterrupts rasterInterrupts)
        {
            _gameModule = gameModule;
            _rasterInterrupts = rasterInterrupts;
            _specs = gameModule.Specs;
            _coreGraphicsModule = gameModule.GameSystem.CoreGraphicsModule;
            _tileModule = gameModule.TileModule;
        }

        public void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _bossPosition = new GameByteGridPoint(memoryBuilder.AddByte(), memoryBuilder.AddByte(), 255, 255);
            _bossBackgroundEnd = memoryBuilder.AddByte();
            _bossDeathTimer = memoryBuilder.AddByte();
        }

        public void OnHBlank()
        {
            if (_bossDeathTimer.Value == 255)
                return;

            var groundPosition = _bossBackgroundEnd.Value;

            if (_coreGraphicsModule.ScreenPoint.Y >= Constants.StatusBarHeight
                && _coreGraphicsModule.ScreenPoint.Y < groundPosition)
            {
                // WHEN Y=8, SCROLLY = 32
                // WHEN Y=12, SCROLLY = 28
                _tileModule.Scroll.X = (byte)(255 - _bossPosition.X);
                _tileModule.Scroll.Y = (byte)((4 * 15) - _bossPosition.Y);
            }

            if(_bossDeathTimer.Value > 0)
            {
                int effectY = _coreGraphicsModule.ScreenPoint.Y - _bossPosition.Y;
                if(effectY >= 0 && effectY <= 32)
                {
                    double pct = (effectY + _bossDeathTimer.Value) / 16.0;

                    if(effectY.IsMod(2))
                        _tileModule.Scroll.X += (byte)(_bossDeathTimer.Value * 1 * Math.Sin(pct * 2.0));
                    else
                        _tileModule.Scroll.X -= (byte)(_bossDeathTimer.Value * 1 * Math.Sin(pct * 2.0));
                }
                    
            }

            if (_coreGraphicsModule.ScreenPoint.Y == groundPosition)
            {
                _tileModule.Scroll.X = _rasterInterrupts.RealScrollX;
                _tileModule.Scroll.Y = (byte)_specs.ScreenHeight;
            }
        }

        public void OnStartup() 
        {
        }
    }
}
