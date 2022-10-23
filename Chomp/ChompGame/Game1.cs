using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Diagnostics;
using System.Text;

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
        private SpriteFont _font;
        public MainSystem GameSystem => _gameSystem;

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
            _font = Content.Load<SpriteFont>("Font");
        }

        protected override void Update(GameTime gameTime)
        {
            _gameSystem.OnLogicUpdate();
            base.Update(gameTime);
        }

        private Stopwatch _renderTimer = new Stopwatch();
        private long _drawMS;
        private long _totalDrawMS;
        private long _totalDrawFrames;
        protected override void Draw(GameTime gameTime)
        {
            _renderTimer.Restart();
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();
            _gameSystem.CoreGraphicsModule.DrawFrame(_spriteBatch, _canvas);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            DrawDebugText();

            var aspectRatio = (double)_renderTarget.Height / _renderTarget.Width;

            int renderWidth = Window.ClientBounds.Width;
            int renderHeight = (int)(renderWidth * aspectRatio);

            if (renderHeight > Window.ClientBounds.Height)
            {
                renderHeight = Window.ClientBounds.Height;
                renderWidth = (int)(renderHeight / aspectRatio);
            }
 
            int renderColumn = 16 + (Window.ClientBounds.Width - renderWidth) / 2;
            int renderRow = (Window.ClientBounds.Height - renderHeight) / 2;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.Draw(_renderTarget, new Rectangle(
                               x: renderColumn,
                               y: renderRow,
                               width: renderWidth,
                               height: renderHeight), Color.White);

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

            _spriteBatch.End();
        }
    }
}
