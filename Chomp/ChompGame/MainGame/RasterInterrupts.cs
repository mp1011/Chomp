﻿using ChompGame.Data;
using ChompGame.GameSystem;

namespace ChompGame.MainGame
{
    class RasterInterrupts : IHBlankHandler
    {
        private readonly Specs _specs;
        private readonly TileModule _tileModule;
        private readonly CoreGraphicsModule _coreGraphicsModule;

        private readonly GameByte _levelTimer;
        private readonly GameByte _currentLevel;

        private GameByte _realScroll;
        private GameByte _autoScroll;
        private MaskedByte _paletteCycleIndex;

        public RasterInterrupts(
            Specs specs, 
            CoreGraphicsModule coreGraphicsModule,
            TileModule tileModule, 
            GameByte levelTimer, 
            GameByte currentLevel)
        {
            _specs = specs;
            _tileModule = tileModule;
            _coreGraphicsModule = coreGraphicsModule;

            _levelTimer = levelTimer;
            _currentLevel = currentLevel;
        }

        public void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _realScroll = memoryBuilder.AddByte();
            _autoScroll = memoryBuilder.AddByte();
            _paletteCycleIndex = new MaskedByte(memoryBuilder.CurrentAddress, (Bit)3, memoryBuilder.Memory);
        }

        public void Update()
        {
            switch(_currentLevel.Value)
            {
                case 0:

                    if ((_levelTimer.Value % 16) == 0)
                    {
                        _autoScroll.Value++;
                        if (_autoScroll.Value > _specs.NameTablePixelWidth)
                            _autoScroll.Value = 0;
                    }

                    if ((_levelTimer.Value % 4) == 0)
                    {
                        _paletteCycleIndex.Value++;
                    }

                    break;
            }
           
        }

        public void OnHBlank()
        {
            switch (_currentLevel.Value)
            {
                case 0:
                    OnHBlank_Stage0();
                    break;
            }
        }

        private void OnHBlank_Stage0()
        {
            if (_tileModule.ScreenPoint.Y == 0)
            {
                _realScroll.Value = _tileModule.Scroll.X;
            }

            if (_tileModule.ScreenPoint.Y < 16)
            {
                _tileModule.Scroll.X = 0;
            }
            else if (_tileModule.ScreenPoint.Y < 24)
            {
                _tileModule.Scroll.X = _autoScroll.Value;
            }
            else if (_tileModule.ScreenPoint.Y < 50)
            {
                _tileModule.Scroll.X = 0;
            }
            else if (_tileModule.ScreenPoint.Y == 50)
            {
                _tileModule.Scroll.X = _realScroll.Value;
            }

            if (_tileModule.ScreenPoint.Y == 0)
            {
                var bgPalette = _coreGraphicsModule.GetPalette(0);
                bgPalette.SetColor(0, ChompGameSpecs.Black);
                bgPalette.SetColor(1, ChompGameSpecs.LightBlue);
                bgPalette.SetColor(2, ChompGameSpecs.LightYellow);
                bgPalette.SetColor(3, ChompGameSpecs.White);
            }
            else if (_tileModule.ScreenPoint.Y == 40)
            {
                var bgPalette = _coreGraphicsModule.GetPalette(0);

                switch (_paletteCycleIndex.Value)
                {
                    case 0:
                        bgPalette.SetColor(0, ChompGameSpecs.Blue4);
                        bgPalette.SetColor(1, ChompGameSpecs.Blue1);
                        bgPalette.SetColor(2, ChompGameSpecs.Blue2);
                        bgPalette.SetColor(3, ChompGameSpecs.Blue3);
                        break;
                    case 1:
                        bgPalette.SetColor(0, ChompGameSpecs.Blue1);
                        bgPalette.SetColor(1, ChompGameSpecs.Blue2);
                        bgPalette.SetColor(2, ChompGameSpecs.Blue3);
                        bgPalette.SetColor(3, ChompGameSpecs.Blue4);
                        break;
                    case 2:
                        bgPalette.SetColor(0, ChompGameSpecs.Blue2);
                        bgPalette.SetColor(1, ChompGameSpecs.Blue3);
                        bgPalette.SetColor(2, ChompGameSpecs.Blue4);
                        bgPalette.SetColor(3, ChompGameSpecs.Blue1);
                        break;
                    case 3:
                        bgPalette.SetColor(0, ChompGameSpecs.Blue3);
                        bgPalette.SetColor(1, ChompGameSpecs.Blue4);
                        bgPalette.SetColor(2, ChompGameSpecs.Blue1);
                        bgPalette.SetColor(3, ChompGameSpecs.Blue2);
                        break;
                }
            }
            else if (_tileModule.ScreenPoint.Y == 48)
            {
                var bgPalette = _coreGraphicsModule.GetPalette(0);
                bgPalette.SetColor(0, ChompGameSpecs.Green3);
                bgPalette.SetColor(1, ChompGameSpecs.Blue1);
                bgPalette.SetColor(2, ChompGameSpecs.Green1);
                bgPalette.SetColor(3, ChompGameSpecs.Green2);
            }
        }

        public void OnStartup()
        {
        }
    }
}