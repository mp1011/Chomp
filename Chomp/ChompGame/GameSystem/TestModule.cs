using ChompGame.Data;
using Microsoft.Xna.Framework.Input;
using System;

namespace ChompGame.GameSystem
{
    class TestModule : Module, ILogicUpdateHandler
    {
        private TileModule _tileModule;
        private SpritesModule _spritesModule;

        public TestModule(MainSystem mainSystem) : base(mainSystem) { }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
        }

        public override void OnStartup()
        {
            _tileModule = GameSystem.GetModule<TileModule>();
            _spritesModule = GameSystem.GetModule<SpritesModule>();

            _spritesModule.Sprites[0].Tile = 4;
            _spritesModule.Sprites[0].X = 12;
            _spritesModule.Sprites[0].Y = 12;
        }

        private bool wasLeftDown;
        private bool wasRightDown;
        public void OnLogicUpdate()
        {
            KeyboardState state = Keyboard.GetState();
            bool leftKey = state.IsKeyDown(Keys.Left);
            bool righKey = state.IsKeyDown(Keys.Right);
            bool upKey = state.IsKeyDown(Keys.Up);
            bool downKey = state.IsKeyDown(Keys.Down);

            if (state.IsKeyDown(Keys.S))
            {
                if (leftKey)
                    _spritesModule.Sprites[0].X--;
                else if (righKey)
                    _spritesModule.Sprites[0].X++;

                if (upKey)
                    _spritesModule.Sprites[0].Y--;
                else if (downKey)
                    _spritesModule.Sprites[0].Y++;
            }
            else if (state.IsKeyDown(Keys.LeftShift))
            {
                if (leftKey)
                    _tileModule.Scroll.X++;

                if (righKey)
                    _tileModule.Scroll.X--;
            }
            else
            {
                if (leftKey && !wasLeftDown)
                    _tileModule.Scroll.X++;

                if (righKey && !wasRightDown)
                    _tileModule.Scroll.X--;
            }

            wasLeftDown = leftKey;
            wasRightDown = righKey;

        }

       
    }
}
