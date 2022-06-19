using ChompGame.GameSystem;
using ChompGame.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ChompGame
{
    public class Game1 : Game
    {
        private readonly Specs _specs;
        private readonly MainSystem _gameSystem;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private Texture2D _canvas;

        public Game1(Specs specs, MainSystem mainSystem)
        {
            _specs = specs;
            _gameSystem = mainSystem;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(16.7);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(GraphicsDevice, _specs.ScreenWidth, _specs.ScreenHeight);
            _canvas = new Texture2D(GraphicsDevice, _specs.ScreenWidth, _specs.ScreenHeight);
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _gameSystem.OnLogicUpdate();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            _gameSystem.CoreGraphicsModule.DrawFrame(_spriteBatch, _canvas);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

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

            base.Draw(gameTime);
        }
    }
}
