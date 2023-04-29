using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;

namespace ChompGame.MainGame
{
    class RasterInterrupts : IHBlankHandler
    {
        private readonly Specs _specs;
        private readonly TileModule _tileModule;
        private readonly CoreGraphicsModule _coreGraphicsModule;
        private readonly WorldScroller _worldScroller;
        private readonly StatusBar _statusBar;

        private GameByte _realScrollX;
        private GameByte _realScrollY;

        private GameByte _autoScroll;
        private MaskedByte _paletteCycleIndex;
        private SceneDefinition _sceneDefinition;

        public RasterInterrupts(
            Specs specs, 
            CoreGraphicsModule coreGraphicsModule,
            WorldScroller scroller,
            TileModule tileModule, 
            StatusBar statusBar)
        {
            _specs = specs;
            _statusBar = statusBar;
            _tileModule = tileModule;
            _coreGraphicsModule = coreGraphicsModule;
            _worldScroller = scroller;
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

        public void Update()
        {
            //switch(_currentLevel.Value)
            //{
            //    case 1:

            //        if ((_levelTimer.Value % 16) == 0)
            //        {
            //            _autoScroll.Value++;
            //            if (_autoScroll.Value > _specs.NameTablePixelWidth)
            //                _autoScroll.Value = 0;
            //        }

            //        if ((_levelTimer.Value % 4) == 0)
            //        {
            //            _paletteCycleIndex.Value++;
            //        }

            //        break;
            //}
           
        }

        public void OnHBlank()
        {
            if (_sceneDefinition == null)
                return;

            GameDebug.Watch4 = new DebugWatch("CameraPixelX", () => _worldScroller.CameraPixelX);

            if (_coreGraphicsModule.ScreenPoint.Y <= Constants.StatusBarHeight
                && _sceneDefinition.HasSprite(SpriteLoadFlags.Player))
            {
                _statusBar.OnHBlank(_realScrollX, _realScrollY);
            }

            if (_sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
            {
                HandleParallax();
            }
        }

        private void HandleParallax()
        {

            if (_coreGraphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                _tileModule.Scroll.X = (byte)(_worldScroller.CameraPixelX / 4);
            }
            else if(_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetParallaxLayerPixel(ParallaxLayer.Back2, includeStatusBar:true))
            {
                _tileModule.Scroll.X = (byte)(_worldScroller.CameraPixelX / 2);
            }
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetParallaxLayerPixel(ParallaxLayer.ForegroundStart, includeStatusBar: true))
            {
                _tileModule.Scroll.X = _realScrollX.Value;
            }


            //int layerABegin = (2+_sceneDefinition.ParallaxLayerABeginTile) * _specs.TileHeight;
            //int layerBBegin = layerABegin + _sceneDefinition.ParallaxLayerATiles * _specs.TileHeight;
            //int layerCBegin = layerBBegin + _sceneDefinition.ParallaxLayerBTiles * _specs.TileHeight;
            //int parallaxEnd = layerCBegin + _sceneDefinition.ParallaxLayerATiles * _specs.TileHeight;


            //if (_tileModule.ScreenPoint.Y == layerABegin && _sceneDefinition.ParallaxLayerATiles > 0)
            //{
            //    _tileModule.Scroll.X = (byte)(_worldScroller.CameraPixelX / 2);
            //}

            //if (_tileModule.ScreenPoint.Y == layerBBegin && _sceneDefinition.ParallaxLayerBTiles > 0)
            //{
            //    _tileModule.Scroll.X = (byte)(_worldScroller.CameraPixelX / 4);
            //}

            //if (_tileModule.ScreenPoint.Y == layerCBegin && _sceneDefinition.ParallaxLayerATiles > 0)
            //{
            //    _tileModule.Scroll.X = (byte)(_worldScroller.CameraPixelX / 2);
            //}

            //if (_tileModule.ScreenPoint.Y == parallaxEnd)
            //{
            //    _tileModule.Scroll.X = _realScroll.Value;
            //}
        }

        public void OnStartup()
        {
        }
    }
}
