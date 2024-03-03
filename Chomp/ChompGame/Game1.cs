using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame;
using ChompGame.MainGame.Editors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace ChompGame
{
    public class Game1 : Game
    {
        private Specs _specs;
        private TileInspector _tileInspector;

        private Func<GraphicsDevice, ContentManager, MainSystem> _createSystem;
        private MainSystem _gameSystem;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private Texture2D _canvas;
        private Texture2D _filter;
      
        private SpriteFont _font;
        public MainSystem GameSystem => _gameSystem;

        private ScreenRenderSize _screenRenderSize = new ScreenRenderSize();

        public Game1(Func<GraphicsDevice, ContentManager, MainSystem> createSystem)
        {
            _createSystem = createSystem;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(16.7);
        }

        protected override void Initialize()
        {
            _gameSystem = _createSystem(GraphicsDevice, Content);

            _tileInspector = new TileInspector(_gameSystem.GetModule<ChompGameModule>());
            _specs = _gameSystem.Specs;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(GraphicsDevice, _specs.ScreenWidth, _specs.ScreenHeight);
            _canvas = new Texture2D(GraphicsDevice, _specs.ScreenWidth, _specs.ScreenHeight);
            _filter = new Texture2D(GraphicsDevice, _specs.ScreenWidth*3, _specs.ScreenHeight * 3);
            _filterPixels = new Color[_filter.Width * _filter.Height];
            _font = Content.Load<SpriteFont>("Font");

            var colors = Content.Load<Texture2D>("SystemPalette");
            _specs.SetColors(colors);
        }

        private bool _wasMouseDown;
        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();

            _showBgVram = ks.IsKeyDown(Keys.V) && !ks.IsKeyDown(Keys.LeftAlt);
            _showSpriteVram = ks.IsKeyDown(Keys.V) && ks.IsKeyDown(Keys.LeftAlt);


            if (ks.IsKeyDown(Keys.D1))
                _paletteIndex = 0;
            if (ks.IsKeyDown(Keys.D2))
                _paletteIndex = 1;
            if (ks.IsKeyDown(Keys.D3))
                _paletteIndex = 2;
            if (ks.IsKeyDown(Keys.D4))
                _paletteIndex = 3;
            
            if (ks.IsKeyDown(Keys.OemComma))
                TargetElapsedTime = TimeSpan.FromMilliseconds(200);
            else
                TargetElapsedTime = TimeSpan.FromMilliseconds(16.7);

#if DEBUG
            EditorInputHelper.Update(_screenRenderSize, _gameSystem.GetModule<TileModule>());
            _tileInspector.Update();
#endif

            _gameSystem.OnLogicUpdate();
            base.Update(gameTime);
        }

        private bool _showBgVram;
        private bool _showSpriteVram;

        private byte _paletteIndex;

        private Stopwatch _renderTimer = new Stopwatch();
        private long _drawMS;
        private long _totalDrawMS;
        private long _totalDrawFrames;

        private Color[] _filterPixels;

        Random _rng = new Random();
        private void GenerateFilter()
        {
            byte[] rgb = new byte[3];
            for(int i =0; i< _filterPixels.Length; i++)
            {
                _filterPixels[i] = new Color(
                    (byte)_rng.Next(0, 15),
                    (byte)_rng.Next(0, 15),
                    (byte)_rng.Next(0, 15),
                    (byte)5);
            }

            _filter.SetData(_filterPixels);
        }
      
        private void WriteMouseTile(int x, int y)
        {
            var tm = _gameSystem.GetModule<TileModule>();

            int screenX = x - _screenRenderSize.X;
            int screenY = y - _screenRenderSize.Y;
            
            if (screenX < 0 || screenX >= _screenRenderSize.Width)
                return;

            if (screenY < 0 || screenY >= _screenRenderSize.Height)
                return;

            screenX = (int)(screenX * ((double)_specs.ScreenWidth / _screenRenderSize.Width));
            screenY = (int)(screenY * ((double)_specs.ScreenWidth / _screenRenderSize.Width));

            int pixelX = screenX + tm.Scroll.X;
            int pixelY = screenY + tm.Scroll.Y; 

            var attrX = (pixelX / (_specs.TileWidth * _specs.AttributeTableBlockSize)).NModByte(tm.AttributeTable.Width);
            var attrY = (pixelY / (_specs.TileHeight * _specs.AttributeTableBlockSize)).NModByte(tm.AttributeTable.Height);

            var attr = tm.AttributeTable[attrX, attrY];
            var tile = tm.NameTable[pixelX / _specs.TileWidth, pixelY / _specs.TileHeight];
            Debug.WriteLine("VRAM:");
            Debug.WriteLine($"X={pixelX} Y={pixelY} TileX={pixelX / _specs.TileWidth} TileY={pixelY / _specs.TileHeight} Tile={tile}");
            Debug.WriteLine($"AttrX={attrX} AttrY={attrY} Attr={attr}");

            //var cgm = _gameSystem.GetModule<ChompGameModule>();
            //var worldX = cgm.WorldScroller.CameraPixelX + screenX;
            //var worldY = cgm.WorldScroller.CameraPixelY + screenY;
            //tile = cgm.WorldScroller.LevelNameTable[worldX / _specs.TileWidth, worldY / _specs.TileHeight];
            //attrX = worldX / (_specs.TileWidth * _specs.AttributeTableBlockSize);
            //attrY = worldY / (_specs.TileHeight * _specs.AttributeTableBlockSize);
            //attr = cgm.WorldScroller.LevelAttributeTable[attrX, attrY];
            //Debug.WriteLine("");
            //Debug.WriteLine("World:");
            //Debug.WriteLine($"X={worldX} Y={worldY} TileX={worldX / _specs.TileWidth} TileY={worldY / _specs.TileHeight} Tile={tile}");
            //Debug.WriteLine($"AttrX={attrX} AttrY={attrY} Attr={attr}");
            //Debug.WriteLine("--------------------------------------");
        }

        protected override void Draw(GameTime gameTime)
        {
            _renderTimer.Restart();
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.White);

            GenerateFilter();

            _spriteBatch.Begin();

            if (_showBgVram)
                _gameSystem.CoreGraphicsModule.DrawVram(_spriteBatch, _canvas, _paletteIndex, false);
            else if (_showSpriteVram)
                _gameSystem.CoreGraphicsModule.DrawVram(_spriteBatch, _canvas, _paletteIndex, true);
            else
                _gameSystem.CoreGraphicsModule.DrawFrame(_spriteBatch, _canvas);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            DrawDebugText();

            var aspectRatio = (double)_renderTarget.Height / _renderTarget.Width;

            _screenRenderSize.Update(Window, aspectRatio);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.Draw(_renderTarget, new Rectangle(
                               x: _screenRenderSize.X,
                               y: _screenRenderSize.Y,
                               width: _screenRenderSize.Width,
                               height: _screenRenderSize.Height), Color.White);

            _spriteBatch.Draw(_filter, new Rectangle(
                             x: _screenRenderSize.X,
                             y: _screenRenderSize.Y,
                             width: _screenRenderSize.Width,
                             height: _screenRenderSize.Height), Color.White);
         
            _spriteBatch.End();

         
            _renderTimer.Stop();
            _drawMS = _renderTimer.ElapsedMilliseconds;
            _totalDrawMS += _drawMS;
            _totalDrawFrames++;

            base.Draw(gameTime);
        }

        private void DrawDebugText()
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate);
            int y = 0;
            foreach (var watch in GameDebug.Watches)
            {
                _spriteBatch.DrawString(_font,
                    $"{watch.Name} = {watch.GetValue()}",
                    new Vector2(0, y),
                    Color.Green);

                y += 16;
            }

            _spriteBatch.DrawString(_font, $"DrawMs = {_drawMS}", new Vector2(0, y), Color.White);
            if (_totalDrawFrames > 0)
                _spriteBatch.DrawString(_font, $"Avg DrawMs = {_totalDrawMS / _totalDrawFrames}", new Vector2(0, y + 16), Color.White);


            var memString = _gameSystem.Memory.ToString();

            y += 24;

            int index = 0;
            int lineSize = Window.ClientBounds.Width > 1900 ? 50 : 21;
            while (index < memString.Length)
            {
                string line = index + lineSize < memString.Length
                    ? memString.Substring(index, lineSize)
                    : memString.Substring(index);

                _spriteBatch.DrawString(_font, line, new Vector2(0, y), Color.Green);
                y += 8;
                index += line.Length;
            }
            
            _spriteBatch.End();
        }
    }
}
