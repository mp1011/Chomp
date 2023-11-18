﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
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

        public byte RealScrollX => _realScrollX;

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
                && _sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
                HandleOcean();
            else if (_sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
                HandleParallax();
        }

        private void HandleAutoScroll()
        {
            var waterBegin = _sceneDefinition.GetBackgroundLayerPixel(BackgroundLayer.Back2, includeStatusBar: true);
            if (_coreGraphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                _tileModule.Scroll.X = (byte)(_autoScroll.Value / 4);
            }
            else if (_coreGraphicsModule.ScreenPoint.Y >= waterBegin)
            {
                int waterY = _coreGraphicsModule.ScreenPoint.Y - waterBegin;
                _tileModule.Scroll.X = (byte)(_autoScroll.Value * ((waterY+5)/16.0));
            }
        }

        private void HandleParallax()
        {

            if (_coreGraphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                _tileModule.Scroll.X = (byte)(_worldScroller.ScrollWindowBegin / 4);
            }
            else if(_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundLayer.Back2, includeStatusBar:true))
            {
                _tileModule.Scroll.X = (byte)(_worldScroller.ScrollWindowBegin / 2);
            }
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundLayer.ForegroundStart, includeStatusBar: true))
            {
                _tileModule.Scroll.X = _realScrollX.Value;
            }
        }

        private void HandleOcean()
        {
            int waterBegin = _sceneDefinition.GetBackgroundLayerPixel(BackgroundLayer.Back2, includeStatusBar: true);

            if (_coreGraphicsModule.ScreenPoint.Y < Constants.StatusBarHeight)
            {
                _tileModule.Scroll.X =0;
            }
            else if(_coreGraphicsModule.ScreenPoint.Y < waterBegin)
            {
                _tileModule.Scroll.X = (byte)(_worldScroller.ScrollWindowBegin / 2);
            }
            else if (_coreGraphicsModule.ScreenPoint.Y >= waterBegin && _coreGraphicsModule.ScreenPoint.Y < waterBegin + 16)
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
            if (_sceneDefinition.IsAutoScroll)
                _autoScroll.Value++;
        }

        public void OnStartup()
        {
        }
    }
}
