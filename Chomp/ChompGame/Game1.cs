using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame;
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

        private Func<GraphicsDevice, ContentManager, MainSystem> _createSystem;
        private MainSystem _gameSystem;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private Texture2D _canvas;
        private Texture2D _filter;
        private Texture2D _theme;

        private Effect _crtEffect;
        private bool _useCrtEffect = true;
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
          
            var colors = Content.Load<Texture2D>("SystemPalette");
            _specs.SetColors(colors);

            _crtEffect = Content.Load<Effect>("CRT");
        }

        protected override void Update(GameTime gameTime)
        {
            _gameSystem.OnLogicUpdate();
            base.Update(gameTime);
        }

        private byte _paletteIndex;
        private bool _savingOrLoading = false;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();

            _gameSystem.CoreGraphicsModule.DrawFrame(_spriteBatch, _canvas);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

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
            base.Draw(gameTime);
        }
    }
}
