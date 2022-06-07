using ChompGame.Data;
using Microsoft.Xna.Framework.Input;
using System;

namespace ChompGame.GameSystem
{
    class TestModule : Module, ILogicUpdateHandler
    {
        private TileModule _tileModule;

        public TestModule(MainSystem mainSystem) : base(mainSystem) { }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
        }

        private bool wasLeftDown;
        private bool wasRightDown;

        public void OnLogicUpdate()
        {
            KeyboardState state = Keyboard.GetState();
            bool leftDown = state.IsKeyDown(Keys.Left);
            bool rightDown = state.IsKeyDown(Keys.Right);

            if (state.IsKeyDown(Keys.LeftShift))
            {
                if (leftDown)
                    _tileModule.Scroll.X++;

                if (rightDown)
                    _tileModule.Scroll.X--;
            }
            else
            {
                if (leftDown && !wasLeftDown)
                    _tileModule.Scroll.X++;

                if (rightDown && !wasRightDown)
                    _tileModule.Scroll.X--;
            }

            wasLeftDown = leftDown;
            wasRightDown = rightDown;

        }

        public override void OnStartup()
        {
            _tileModule = GameSystem.GetModule<TileModule>();
        }
    }
}
