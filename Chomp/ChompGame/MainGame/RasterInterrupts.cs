using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteModels;
using System;

namespace ChompGame.MainGame
{
    class RasterInterrupts : IHBlankHandler
    {
        private readonly Specs _specs;
        private readonly TileModule _tileModule;
        private readonly CoreGraphicsModule _coreGraphicsModule;
        private WorldScroller _worldScroller => _gameModule.WorldScroller;
        private readonly StatusBar _statusBar;
        private readonly ChompGameModule _gameModule;

        private GameByte _realScrollX;
        private GameByte _realScrollY;

        private GameByte _autoScroll;
        private MaskedByte _paletteCycleIndex;
        private SceneDefinition _sceneDefinition;
        private WorldSprite _player;

        public byte RealScrollX => _realScrollX;

        private bool HasHeatWaveEffect
        {
            get
            {
                if (_sceneDefinition.Theme != ThemeType.Desert)
                    return false;

                if (_sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
                    return true;

                return _sceneDefinition.LeftTiles == 0 && _sceneDefinition.RightTiles == 0;
            }
        }
                          
        public RasterInterrupts(
            ChompGameModule gameModule,
            CoreGraphicsModule coreGraphicsModule)
        {
            _specs = gameModule.Specs;
            _statusBar = gameModule.StatusBar;
            _tileModule = gameModule.TileModule;
            _coreGraphicsModule = coreGraphicsModule;
            _gameModule = gameModule;
        }

        public void SetPlayer(WorldSprite player)
        {
            _player = player;
        }

        public void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _realScrollX = memoryBuilder.AddByte();
            _realScrollY = memoryBuilder.AddByte();
            _autoScroll = memoryBuilder.AddByte();
            _paletteCycleIndex = memoryBuilder.AddMaskedByte(Bit.Right2);
        }

        public void SetScene(SceneDefinition scene)
        {
            _sceneDefinition = scene;
        }

        public void OnHBlank()
        {
            if (_sceneDefinition == null)
                return;

            if (_coreGraphicsModule.ScreenPoint.Y <= Constants.StatusBarHeight
                && _sceneDefinition.HasSprite(SpriteType.Player))
            {
                _statusBar.OnHBlank(_realScrollX, _realScrollY);
            }

            if (_sceneDefinition.IsAutoScroll)
                HandleAutoScroll();
            else if (_sceneDefinition.Theme == ThemeType.Ocean
                && _sceneDefinition.ScrollStyle != ScrollStyle.NameTable)
                HandleOcean();
            else if (_sceneDefinition.Theme == ThemeType.Mist)
                HandleMist();
            else if (HasHeatWaveEffect)
                HandleHeatWave();
            else if (_sceneDefinition.Theme == ThemeType.CityTrain)
                HandleTrainParallax();
            else if (_sceneDefinition.Theme == ThemeType.GlitchCore)
            {
                HandleGlitchEffects();
            }
            else if (_sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
            {
                if (_sceneDefinition.HorizontalScrollStyle == HorizontalScrollStyle.Flat)
                    return;
                else if (_sceneDefinition.Theme == ThemeType.TechBase || _sceneDefinition.Theme == ThemeType.TechBase2)
                    HandleParallax2();
                else
                    HandleParallax();
            }
           
        }

        private void HandleAutoScroll()
        {
            if (_sceneDefinition.Theme == ThemeType.OceanAutoscroll)
            {
                var waterBegin = _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Middle, includeStatusBar: true);
                if (_coreGraphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
                {
                    _tileModule.Scroll.X = (byte)(_autoScroll.Value / 4);
                }
                else if (_coreGraphicsModule.ScreenPoint.Y >= waterBegin)
                {

                    int waterY = _coreGraphicsModule.ScreenPoint.Y - waterBegin;
                    _tileModule.Scroll.X = (byte)(_autoScroll.Value * ((waterY + 5) / 16.0));


                    if (waterY == 0)
                    {
                        var palette = _coreGraphicsModule.GetBackgroundPalette(BgPalette.Background);
                        _gameModule.PaletteModule.LoadPalette(_gameModule.PaletteModule.BgPalette1.Address, palette);
                    }
                    else if (waterY >= 0 && waterY.IsMod(2) && waterY < 4)
                    {
                        var palette = _coreGraphicsModule.GetBackgroundPalette(BgPalette.Background);
                        _gameModule.PaletteModule.Lighten(palette, 1);
                        _gameModule.PaletteModule.Lighten(palette, 2);
                        _gameModule.PaletteModule.Lighten(palette, 3);
                    }
                    else if (waterY >= 8 && waterY.IsMod(2) && waterY < 16)
                    {
                        var palette = _coreGraphicsModule.GetBackgroundPalette(BgPalette.Background);
                        _gameModule.PaletteModule.Darken(palette, 1);
                        _gameModule.PaletteModule.Darken(palette, 2);
                        _gameModule.PaletteModule.Darken(palette, 3);
                    }                    
                }
            }
            else
            {
                int scrollAmount = (256 - _tileModule.Scroll.Y).NMod(256);

                var cloudBegin = Constants.StatusBarHeight + scrollAmount;
                var cloudEnd = _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Middle, includeStatusBar: true) + scrollAmount
                ;

                if (_coreGraphicsModule.ScreenPoint.Y >= cloudBegin && 
                    _coreGraphicsModule.ScreenPoint.Y < cloudEnd)
                {
                    int cloudY = _coreGraphicsModule.ScreenPoint.Y - cloudEnd;
                    _tileModule.Scroll.X = (byte)(_autoScroll.Value * ((-cloudY + 5) / 16.0));
                }
                else if (_coreGraphicsModule.ScreenPoint.Y >= cloudEnd)
                {
                    _tileModule.Scroll.X = _autoScroll.Value;
                }
                else if (_coreGraphicsModule.ScreenPoint.Y < Constants.StatusBarHeight)
                {
                    _tileModule.Scroll.X = 0;
                }
                else if(_coreGraphicsModule.ScreenPoint.Y < cloudBegin)
                {
                    var r = _gameModule.RandomModule.FixedRandom((byte)(cloudBegin - _coreGraphicsModule.ScreenPoint.Y), 2);
                   _tileModule.Scroll.X = r switch {
                        0 => (byte)(_autoScroll.Value / 4),
                        1 => (byte)(_autoScroll.Value / 2),
                        2 => (byte)(_autoScroll.Value),
                        _ => (byte)(_autoScroll.Value * 2)
                    };
                }
                else if (_coreGraphicsModule.ScreenPoint.Y >= Constants.StatusBarHeight)
                {
                    _tileModule.Scroll.X = _autoScroll.Value;
                }


                var renderY = (_coreGraphicsModule.ScreenPoint.Y + _tileModule.Scroll.Y).NMod(256);
                if(_coreGraphicsModule.ScreenPoint.Y >= Constants.StatusBarHeight && renderY < Constants.StatusBarHeight)
                {
                    _tileModule.Scroll.X = (byte)_specs.ScreenWidth;
                }
            }
        }

        private void HandleHeatWave()
        {
            int baseY = _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Bottom, true) - 1;
            if (_sceneDefinition.ScrollStyle == ScrollStyle.NameTable)
                baseY = 16;

            if (_coreGraphicsModule.ScreenPoint.Y < Constants.StatusBarHeight)
            {
                _tileModule.Scroll.Y = 0;
                return;
            }

            if (_coreGraphicsModule.ScreenPoint.Y >= baseY)
            {
                _tileModule.Scroll.Y = _realScrollY.Value;
                return;
            }

            var offsetLine = _autoScroll.Value % 4;

            if ((_coreGraphicsModule.ScreenPoint.Y % 4) == offsetLine)
                _tileModule.Scroll.Y = (byte)(_realScrollY.Value + 1);
            else
                _tileModule.Scroll.Y = _realScrollY.Value;
        }

        private void HandleParallax()
        {
            if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Upper, includeStatusBar: true))
                _tileModule.Scroll.X = (byte)(_worldScroller.ScrollWindowBegin / 4);
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Top, includeStatusBar: true))
                _tileModule.Scroll.X = (byte)(_worldScroller.ScrollWindowBegin / 2);
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Middle, includeStatusBar: true))
                _tileModule.Scroll.X = (byte)(_worldScroller.ScrollWindowBegin / 2);
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Lower, includeStatusBar: true))
                _tileModule.Scroll.X = _realScrollX.Value;
            else if (_coreGraphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
                _tileModule.Scroll.X = _realScrollX.Value;  
        }
        
        private void HandleParallax2()
        {
            if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Middle, includeStatusBar: true))
                _tileModule.Scroll.X = (byte)(_worldScroller.ScrollWindowBegin / 4);
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Upper, includeStatusBar: true))
                _tileModule.Scroll.X = (byte)(_worldScroller.ScrollWindowBegin / 2);
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Lower, includeStatusBar: true))
                _tileModule.Scroll.X = (byte)(_worldScroller.ScrollWindowBegin / 2);
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Bottom, includeStatusBar: true))
                _tileModule.Scroll.X = _realScrollX.Value;
            else if (_coreGraphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
                _tileModule.Scroll.X = _realScrollX.Value;
        }

        private void HandleTrainParallax()
        {
            if (_coreGraphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                _tileModule.Scroll.X = (byte)(_autoScroll.Value / 4);
            }
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Middle, includeStatusBar: true))
            {
                _tileModule.Scroll.X = (byte)(_autoScroll.Value / 2);
            }           
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Bottom, includeStatusBar: true))
            {
                _tileModule.Scroll.X = _realScrollX.Value;
            }
            else if (_coreGraphicsModule.ScreenPoint.Y == _specs.ScreenHeight - 8)
            {
                _tileModule.Scroll.X = (byte)(_autoScroll.Value);
            }
            else if (_coreGraphicsModule.ScreenPoint.Y == _specs.ScreenHeight-1)
            {
                _tileModule.Scroll.X = _realScrollX.Value;
            }
        }

        private void HandleMist()
        {
            int sy = _coreGraphicsModule.ScreenPoint.Y + _tileModule.Scroll.Y;

            int mistBegin = _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Upper, includeStatusBar: true);

            if (sy < mistBegin)
            {
                _tileModule.Scroll.X = 0;
            }
            else if (sy < _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Lower, true))
            {
                int y = _coreGraphicsModule.ScreenPoint.Y - mistBegin;

                var b = y % 8;
                if (b > 4)
                    b = 8 - b;

                var t = (_gameModule.LevelTimer.Value % 255) / 255.0;
                var t1 = _gameModule.LevelTimer.Value / 64.0;

                var a = t1 + (b * (y + 1) * Math.Sin(1 * Math.PI * t) / 4.0);
                _tileModule.Scroll.X = (byte)(_autoScroll.Value +(a));
            }
            else
            {
                _tileModule.Scroll.X = _realScrollX.Value;
                _tileModule.Scroll.Y = _realScrollY.Value;
            }
        }

        private void HandleGlitchEffects()
        {
            if (_gameModule.LevelTimer.Value > 32 && _gameModule.CurrentLevel < Level.Level7_9_TotalGlitch)
                return;

            var sy = _coreGraphicsModule.ScreenPoint.Y;

            int psy = _player.GetSprite().Y - _gameModule.SpritesModule.Scroll.Y;

            if (sy < Constants.StatusBarHeight)
                return;
            else if(sy == _specs.ScreenHeight-1)
            {
                _tileModule.Scroll.X = _realScrollX.Value;
                _tileModule.Scroll.Y = _realScrollY.Value;
                return;
            }
            else if(sy > psy - 16 && sy < psy + 16)
            {
                _tileModule.Scroll.X = _realScrollX.Value;
                _tileModule.Scroll.Y = _realScrollY.Value;
                return;
            }

            var y = (sy % 8) / 8.0; ;
            var x = (_coreGraphicsModule.ScreenPoint.X % 4) / 4.0; ;

            var t = (_gameModule.LevelTimer.Value % 64.0) / 64.0;

            var offset = 4 * Math.Sin(y + (2 * Math.PI) * t);
            _tileModule.Scroll.X = (byte)(_realScrollX.Value + offset);

            offset = 2 * y + t;
            _tileModule.Scroll.Y = (byte)(_realScrollY.Value + offset);

        }


        private void HandleOcean()
        {
            int waterBegin = _sceneDefinition.GetBackgroundLayerPixel(BackgroundPart.Middle, includeStatusBar: true);

            if (_coreGraphicsModule.ScreenPoint.Y < Constants.StatusBarHeight)
            {
                _tileModule.Scroll.X =0;
            }
            else if(_coreGraphicsModule.ScreenPoint.Y < waterBegin)
            {
                _tileModule.Scroll.X = (byte)(_worldScroller.ScrollWindowBegin / 2);
            }
            else if (_coreGraphicsModule.ScreenPoint.Y >= waterBegin 
                && _coreGraphicsModule.ScreenPoint.Y < waterBegin + 8)
            {                
                int y = _coreGraphicsModule.ScreenPoint.Y - waterBegin;

                var b = y % 8;
                if (b > 4)
                    b = 8 - b;

                var t = (_gameModule.LevelTimer.Value % 255) / 255.0;
                var t1 = _gameModule.LevelTimer.Value / 64.0;

                var a = t1 + (b * (y+1) * Math.Sin(2 * Math.PI * t) / 2.0);
                _tileModule.Scroll.X = (byte)(a);
            }
            else
            {
                _tileModule.Scroll.X = _realScrollX.Value;
            }            
        }

        public void Update()
        {
            if (_sceneDefinition.Theme == ThemeType.Mist && _gameModule.LevelTimer.IsMod(8))
                _autoScroll.Value++;

            if (HasHeatWaveEffect && _gameModule.LevelTimer.Value.IsMod(8))
                _autoScroll.Value++;

            if (_sceneDefinition.IsAutoScroll || _sceneDefinition.Theme == ThemeType.CityTrain)
                _autoScroll.Value++;
        }

        public void OnStartup()
        {
        }
    }
}
