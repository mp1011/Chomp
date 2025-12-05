using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame;
using ChompGame.Menu;
using ChompGame.Option;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Linq;

namespace ChompGame
{
    public class Game1 : Game
    {
        private Specs _specs;

        private MenuItem[] _menu;
        private KeyBinder _keyBinder;

        private Func<GraphicsDevice, ContentManager, MainSystem> _createSystem;
        private MainSystem _gameSystem;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private Texture2D _canvas;
        private Texture2D _theme;
        private Texture2D _menuBack;

        private SpriteFont _font;

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
            _keyBinder = new KeyBinder(_gameSystem.Options);

            _graphics.IsFullScreen = _gameSystem.Options.FullScreen;
            _useCrtEffect = _gameSystem.Options.UseCRT;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(GraphicsDevice, _specs.ScreenWidth, _specs.ScreenHeight);
            _canvas = new Texture2D(GraphicsDevice, _specs.ScreenWidth, _specs.ScreenHeight);
            _menuBack = new Texture2D(GraphicsDevice, 1, 1);
            _theme = Content.Load<Texture2D>("CRT_Image");
          
            var colors = Content.Load<Texture2D>("SystemPalette");
            _specs.SetColors(colors);

            _crtEffect = Content.Load<Effect>("CRT");

            _menuBack.SetData(new[] { Color.White });
            _font = Content.Load<SpriteFont>("Font");

            _menu = new MenuItem[]
            {
                new MenuItem(0, "Option", 56, Option_Clicked),
                new MenuItem(1, "Key Bindings", 128, KeyBindings_Clicked),
                new MenuItem(2, "Full Screen", 128, FullScreen_Clicked),
                new MenuItem(3, "CRT Effect", 128, CrtEffect_Clicked),
                new MenuItem(4, "Reset", 128, Reset_Clicked),
                new MenuItem(5, "Exit", 128, () => { Exit(); } ),
            };

            _menu[0].Visible = true;
            _menu[1].Visible = false;
            _menu[2].Visible = false;
        }

        private void Option_Clicked()
        {
            foreach(var menu in _menu.Skip(1))
            {
                menu.Visible = true;
            }
        }

        private void FullScreen_Clicked()
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _gameSystem.Options.FullScreen = _graphics.IsFullScreen;
            _gameSystem.Options.Save();
            _graphics.ApplyChanges();
            CloseMenus();
        }

        private void KeyBindings_Clicked()
        {
            _keyBinder.Activate();
        }

        private void CrtEffect_Clicked()
        {
            _useCrtEffect = !_useCrtEffect;
            _gameSystem.Options.UseCRT = _useCrtEffect;
            _gameSystem.Options.Save();
            CloseMenus();
        }

        private void Reset_Clicked()
        {
            var m = _gameSystem.GetModule<ChompGameModule>();
            m.Reset();

            CloseMenus();
        }

        private void CloseMenus()
        {
            foreach (var menu in _menu.Skip(1))
            {
                menu.Visible = false;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (_keyBinder.Active)
            {
                _gameSystem.GetModule<MusicModule>().Pause();
                if(!_keyBinder.Update())
                {
                    _gameSystem.GetModule<MusicModule>().Resume();
                }
            }
            else
            {
                _gameSystem.OnLogicUpdate();

                var mouse = Mouse.GetState();
                foreach (var menu in _menu)
                {
                    menu.Update(mouse);
                }

                if (mouse.X < -16 || mouse.X > 150 || mouse.Y < -16 || mouse.Y > _menu.Max(p => p.Area.Bottom) + 16)
                    CloseMenus();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {           
            if(_keyBinder.Active)
            {
                GraphicsDevice.Clear(Color.Black);

                _keyBinder.Draw(_spriteBatch, _font);
                return;
            }

            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();
            _gameSystem.CoreGraphicsModule.DrawFrame(_spriteBatch, _canvas);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

           
            if (_useCrtEffect)
            {
                var aspectRatio = (double)_renderTarget.Height / _renderTarget.Width;
                _screenRenderSize.Update((int)(Window.ClientBounds.Width * 0.90), (int)(Window.ClientBounds.Height * 0.95), aspectRatio);

                var themeAspectRatio = (double)_theme.Height / _theme.Width;
                _themeScreenRenderSize.Update(Window.ClientBounds.Width, Window.ClientBounds.Height, themeAspectRatio);

                _crtEffect.Parameters["ScreenSize"]?.SetValue(new Vector2(_themeScreenRenderSize.Width * 0.60f, _themeScreenRenderSize.Height * 0.65f));
                _crtEffect.Parameters["Time"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);

                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: _crtEffect);
               
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
        }
            else
            {
                var aspectRatio = (double)_renderTarget.Height / _renderTarget.Width;
                _screenRenderSize.Update((int)Window.ClientBounds.Width, (int)Window.ClientBounds.Height, aspectRatio);

                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _spriteBatch.Draw(_renderTarget, new Rectangle(
                                  x: (int)_screenRenderSize.X,
                                  y: (int)_screenRenderSize.Y,
                                  width: (int)(_screenRenderSize.Width),
                                  height: (int)(_screenRenderSize.Height)), Color.White);
                _spriteBatch.End();
            }
               
            _spriteBatch.Begin();
            foreach (var menu in _menu.Where(p=>p.Visible))
            {
                _spriteBatch.Draw(_menuBack, menu.Area, menu.MouseOver ? Color.DarkCyan : Color.DarkGray);
                _spriteBatch.DrawString(_font, menu.Text, new Vector2(4, menu.Area.Y + 2), Color.White);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
