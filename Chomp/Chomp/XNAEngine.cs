using Chomp.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chomp
{
    public class XNAEngine : Game
    {
        private readonly SystemSpecs _systemSpecs;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private Texture2D _canvas;

        public delegate void OnRenderDelegate(SpriteBatch spriteBatch, Texture2D canvas);

        public OnRenderDelegate OnRender;

        public XNAEngine(SystemSpecs systemSpecs)
        {
            _systemSpecs = systemSpecs;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(GraphicsDevice, _systemSpecs.ScreenWidth, _systemSpecs.ScreenHeight);
            _canvas = new Texture2D(GraphicsDevice, _systemSpecs.ScreenWidth, _systemSpecs.ScreenHeight);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            OnRender?.Invoke(_spriteBatch, _canvas);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            var aspectRatio = (double)_renderTarget.Height / _renderTarget.Width;
            _spriteBatch.Begin();
            _spriteBatch.Draw(_renderTarget, new Rectangle(0, 0, Window.ClientBounds.Width, (int)(Window.ClientBounds.Width * aspectRatio)), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
