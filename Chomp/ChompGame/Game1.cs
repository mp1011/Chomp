﻿using ChompGame.Data;
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

        #region 3DStuff

        int vertices = 6;

        //Camera
        Vector3 camTarget;
        Vector3 camPosition;
        Matrix projectionMatrix;
        Matrix viewMatrix;
        Matrix worldMatrix;

        //BasicEffect for rendering
        BasicEffect basicEffect;

        //Geometric info
        VertexPositionColorTexture[] triangleVertices;
        VertexBuffer vertexBuffer;
        #endregion

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

            #region 3D stuff
            //Setup Camera
            camTarget = new Vector3(0f, 0f, 0f);
            camPosition = new Vector3(0f, 0f, -100f);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45f),
                               GraphicsDevice.DisplayMode.AspectRatio,
                1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         new Vector3(0f, 1f, 0f));// Y up
            worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
                          Forward, Vector3.Up);

            //BasicEffect
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.Alpha = 1f;

            triangleVertices = new VertexPositionColorTexture[vertices];
            triangleVertices[0] = new VertexPositionColorTexture(
                new Vector3(-40, 40, 0),
                Color.White,
                new Vector2(1, 0));

            triangleVertices[1] = new VertexPositionColorTexture(
                new Vector3(40, -40, 0),
                Color.White,
                new Vector2(0, 1));

            triangleVertices[2] = new VertexPositionColorTexture(
                new Vector3(40, 40, 0), 
                Color.White,
                new Vector2(0,0));
            

            triangleVertices[3] = new VertexPositionColorTexture(
               new Vector3(40, -40, 0),
               Color.White,
               new Vector2(0, 1));
            triangleVertices[4] = new VertexPositionColorTexture(
                new Vector3(-40, 40, 0),
                Color.White,
                new Vector2(1, 0));
            triangleVertices[5] = new VertexPositionColorTexture(
               new Vector3(-40, -40, 0),
               Color.White,
               new Vector2(1, 1));

            //Vert buffer
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(
                           VertexPositionColorTexture), vertices, BufferUsage.
                           WriteOnly);
            vertexBuffer.SetData(triangleVertices);
            #endregion

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
            _draw3D = !Keyboard.GetState().IsKeyDown(Keys.R);

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
        private long _totalDrawMS;
        private long _totalDrawFrames;

        bool _draw3D = false;
        bool _r = false;

        protected override void Draw(GameTime gameTime)
        {
            _renderTimer.Restart();
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();
            _gameSystem.CoreGraphicsModule.DrawFrame(_spriteBatch, _canvas);
            _spriteBatch.End();

            if (_draw3D)
            {
                basicEffect.Projection = projectionMatrix;
                basicEffect.View = viewMatrix;
                basicEffect.World = worldMatrix;
                basicEffect.Texture = _canvas;
                basicEffect.TextureEnabled = true;
                basicEffect.EnableDefaultLighting();

                _r = !_r;
                if (_r)
                {
                    basicEffect.AmbientLightColor = new Vector3(0.53f, 0.56f, 0.53f);  
                }
                else
                {
                    basicEffect.AmbientLightColor = new Vector3(0.5f, 0.54f, 0.5f);
                }

                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

                GraphicsDevice.Clear(Color.Black);
                GraphicsDevice.SetVertexBuffer(vertexBuffer);

                basicEffect.DirectionalLight0.Enabled = true;
                basicEffect.DiffuseColor = new Vector3(1, 0, 0);
                basicEffect.SpecularColor = new Vector3(0, 0, 0);



                basicEffect.DirectionalLight1.Enabled = false;
                basicEffect.DirectionalLight2.Enabled = false;


                foreach (EffectPass pass in basicEffect.CurrentTechnique.
                        Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawPrimitives(PrimitiveType.
                                                  TriangleList, 0, vertices);
                }

                _spriteBatch.Begin(SpriteSortMode.Immediate);

                var p = _gameSystem.GetModule<SpritesModule>().GetSprite(0);
                var playerX = new MaskedByte(p.Address, (Bit)(_gameSystem.Specs.NameTablePixelWidth / 2 - 1), _gameSystem.Memory);
                _spriteBatch.DrawString(_font, $"X = {playerX.Value}", new Vector2(0, 0), Color.Green);

                var screenX = new GameBit(p.Address + 3, Bit.Bit6, _gameSystem.Memory);
                var playerXExtended = new ExtendedByte(playerX, screenX, _gameSystem.Specs.ScreenWidth);
                _spriteBatch.DrawString(_font, $"Xe = {playerXExtended.Value}", new Vector2(0, 16), Color.Green);


                _spriteBatch.DrawString(_font, $"ScrollX = {_gameSystem.GetModule<SpritesModule>().Scroll.X}", new Vector2(0, 32), Color.Green);
                _spriteBatch.DrawString(_font, $"ScrollY = {_gameSystem.GetModule<SpritesModule>().Scroll.Y}", new Vector2(0, 48), Color.Green);

                _spriteBatch.DrawString(_font, $"DrawMs = {_drawMS}", new Vector2(0, 64), Color.White);
                if(_totalDrawFrames > 0)
                    _spriteBatch.DrawString(_font, $"Avg DrawMs = {_totalDrawMS / _totalDrawFrames}", new Vector2(0, 80), Color.White);

                _spriteBatch.End();

            }
            else
            {
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);

                _spriteBatch.Begin(SpriteSortMode.Immediate);
                _spriteBatch.DrawString(_font, _drawMS.ToString(), new Vector2(0, 0), Color.Green);
                _spriteBatch.DrawString(_font, _memoryString, new Vector2(0, 16), Color.White);
                _spriteBatch.End();

                var aspectRatio = (double)_renderTarget.Height / _renderTarget.Width;

                int renderWidth = Window.ClientBounds.Width;
                int renderHeight = (int)(renderWidth * aspectRatio);

                if (renderHeight > Window.ClientBounds.Height)
                {
                    renderHeight = Window.ClientBounds.Height;
                    renderWidth = (int)(renderHeight / aspectRatio);
                }
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _spriteBatch.Draw(_renderTarget, new Rectangle(
                    x: (Window.ClientBounds.Width - renderWidth) / 2,
                    y: (Window.ClientBounds.Height - renderHeight) / 2,
                    width: renderWidth,
                    height: renderHeight), Color.White);
                _spriteBatch.End();
            }

            _renderTimer.Stop();
            _drawMS = _renderTimer.ElapsedMilliseconds;
            _totalDrawMS += _drawMS;
            _totalDrawFrames++;

            base.Draw(gameTime);
        }
    }
}
