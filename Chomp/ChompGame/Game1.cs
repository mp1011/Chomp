using ChompGame.GameSystem;
using ChompGame.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Text;

namespace ChompGame
{
    public class Game1 : Game
    {
        private Specs _specs;

        private Func<GraphicsDevice, MainSystem> _createSystem;
        private MainSystem _gameSystem;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private Texture2D _canvas;
        private SpriteFont _font;
        private string _memoryString="";

        public MainSystem GameSystem => _gameSystem;

        public Game1(Func<GraphicsDevice, MainSystem> createSystem)
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
            _gameSystem = _createSystem(GraphicsDevice);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _gameSystem.OnLogicUpdate();

            var sb = new StringBuilder();
            int w = 0;
            for(int i =0; i < _gameSystem.Memory.RAMSize; i++)
            {
                sb.Append(_gameSystem.Memory[i].ToString("X2"));
                w++;
                if(w==20)
                {
                    w = 0;
                    sb.AppendLine();
                }
            }
            _memoryString = sb.ToString();

            base.Update(gameTime);
        }

        private Stopwatch _renderTimer = new Stopwatch();
        private long _drawMS;
     
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

            _spriteBatch.Begin(SpriteSortMode.Immediate);
            _spriteBatch.DrawString(_font, _drawMS.ToString(), new Vector2(0, 0), Color.Green);
            _spriteBatch.DrawString(_font, _memoryString, new Vector2(0, 16), Color.White);
            _spriteBatch.End();

            var aspectRatio = (double)_renderTarget.Height / _renderTarget.Width;

            int renderWidth = Window.ClientBounds.Width;
            int renderHeight = (int)(renderWidth * aspectRatio);

            if(renderHeight > Window.ClientBounds.Height)
            {
                renderHeight = Window.ClientBounds.Height;
                renderWidth = (int)(renderHeight / aspectRatio);
            }
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_renderTarget, new Rectangle(
                x:(Window.ClientBounds.Width - renderWidth)/2,
                y:(Window.ClientBounds.Height - renderHeight)/2, 
                width: renderWidth,
                height: renderHeight), Color.White);
            _spriteBatch.End();

            _renderTimer.Stop();
            _drawMS = _renderTimer.ElapsedMilliseconds;


            base.Draw(gameTime);
        }
    }
}
