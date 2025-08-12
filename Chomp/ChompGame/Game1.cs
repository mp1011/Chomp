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
        public bool DebugDrawMode = false;

        private Specs _specs;
        private TileInspector _tileInspector;

        private Func<GraphicsDevice, ContentManager, MainSystem> _createSystem;
        private MainSystem _gameSystem;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private Texture2D _canvas;
        private Texture2D _filter;
        private Texture2D _theme;

        private Effect _crtEffect;
        private bool _useCrtEffect = true; // Toggle for CRT effect

        private SpriteFont _font;
        public MainSystem GameSystem => _gameSystem;

        private ScreenRenderSize _screenRenderSize = new ScreenRenderSize();
        private ScreenRenderSize _themeScreenRenderSize = new ScreenRenderSize();

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
            _filter = new Texture2D(GraphicsDevice, _specs.ScreenWidth * 3, _specs.ScreenHeight * 3);
            _theme = Content.Load<Texture2D>("CRT_Image");
            _filterPixels = new Color[_filter.Width * _filter.Height];
            _font = Content.Load<SpriteFont>("Font");

            var colors = Content.Load<Texture2D>("SystemPalette");
            _specs.SetColors(colors);

            // Load CRT shader effect (DX11 compatible)
            _crtEffect = Content.Load<Effect>("CRT");
        }

        private bool _wasMouseDown;
        private KeyboardState _priorDebugKeyState;

        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.LeftShift) && ks.IsKeyDown(Keys.S))
                SaveState();

            if (ks.IsKeyDown(Keys.LeftShift) && ks.IsKeyDown(Keys.L))
                LoadState();

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

            // Toggle CRT effect with F1 and F2
            if (ks.IsKeyDown(Keys.F1))
                _useCrtEffect = true;
            if (ks.IsKeyDown(Keys.F2))
                _useCrtEffect = false;

#if DEBUG
            EditorInputHelper.Update(_screenRenderSize, _gameSystem.GetModule<TileModule>());

            _tileInspector.Update();
#endif

            if (ks.IsKeyDown(Keys.Pause) && !_priorDebugKeyState.IsKeyDown(Keys.Pause))
                _logicPaused = !_logicPaused; ;

            _priorDebugKeyState = ks;

            if (!_logicPaused)
                _gameSystem.OnLogicUpdate();
            base.Update(gameTime);
        }

        private bool _logicPaused = false;

        private bool _showBgVram;
        private bool _showSpriteVram;

        private byte _paletteIndex;

        private Stopwatch _renderTimer = new Stopwatch();
        private long _drawMS;
        private long _totalDrawMS;
        private long _totalDrawFrames;

        private Color[] _filterPixels;

        Random _rng = new Random();

        private bool _savingOrLoading = false;

        public void SaveState()
        {
            System.IO.File.WriteAllBytes("gamestate.bin", _gameSystem.Memory.ToArray());
        }

        public void LoadState()
        {
            var state = System.IO.File.ReadAllBytes("gamestate.bin");
            for (int i = 0; i < state.Length; i++)
                _gameSystem.Memory[i] = state[i];
        }

        private void GenerateFilter()
        {
            byte[] rgb = new byte[3];
            for (int i = 0; i < _filterPixels.Length; i++)
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

            if (DebugDrawMode)
                DrawDebugText();

            var aspectRatio = (double)_renderTarget.Height / _renderTarget.Width;
            var themeAspectRatio = (double)_theme.Height / _theme.Width;

            _themeScreenRenderSize.Update(Window.ClientBounds.Width, Window.ClientBounds.Height, themeAspectRatio);

            _screenRenderSize.Update((int)(Window.ClientBounds.Width * 0.90), (int)(Window.ClientBounds.Height * 0.95), aspectRatio);
          

            // Set CRT shader parameters (DX11 compatible)
            if (_useCrtEffect && _crtEffect != null)
            {
                _crtEffect.Parameters["ScreenSize"]?.SetValue(new Vector2(_themeScreenRenderSize.Width * 0.60f, _themeScreenRenderSize.Height * 0.65f));
                _crtEffect.Parameters["Time"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);

                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: _crtEffect);
            }
            else
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            }

            _spriteBatch.Draw(_renderTarget, new Rectangle(
                               x: (int)(_themeScreenRenderSize.X + (_themeScreenRenderSize.Width * 0.090)),
                               y: (int)(_themeScreenRenderSize.Y + (_themeScreenRenderSize.Height * 0.15)),
                               width: (int)(_themeScreenRenderSize.Width * 0.60),
                               height: (int)(_themeScreenRenderSize.Height * 0.65)), Color.White);

            _spriteBatch.End();

            _spriteBatch.Begin();
            _spriteBatch.Draw(_theme, new Rectangle(
                               x: _themeScreenRenderSize.X,
                               y: _themeScreenRenderSize.Y,
                               width: _themeScreenRenderSize.Width,
                               height: _themeScreenRenderSize.Height), Color.White);
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
