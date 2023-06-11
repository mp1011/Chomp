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
            _worldScroller = gameModule.WorldScroller;
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

        private bool IsBossScene => _sceneDefinition.HasSprite(SpriteLoadFlags.Boss) && _sceneDefinition.ScrollStyle == ScrollStyle.NameTable;

        private void HandleParallax()
        {

            if (_coreGraphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                _tileModule.Scroll.X = (byte)(_worldScroller.CameraPixelX / 4);
            }
            else if(_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundLayer.Back2, includeStatusBar:true))
            {
                _tileModule.Scroll.X = (byte)(_worldScroller.CameraPixelX / 2);
            }
            else if (_coreGraphicsModule.ScreenPoint.Y == _sceneDefinition.GetBackgroundLayerPixel(BackgroundLayer.ForegroundStart, includeStatusBar: true))
            {
                _tileModule.Scroll.X = _realScrollX.Value;
            }
        }

        public void Update()
        {

        }

        public void OnStartup()
        {
        }
    }
}
