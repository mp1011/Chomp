using ChompGame.GameSystem;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace ChompGame.MainGame.Editors
{
    static class EditorInputHelper
    {
        private static bool _leftWasPressed, _rightWasPressed;
        private static Keys[] _lastPressedKeys, _currentPressedKeys;

        public static int MouseX { get; private set; }
        public static int MouseY { get; private set; }

        public static bool LeftClicked { get; private set; }
        public static bool RightClicked { get; private set; }

        public static bool IsKeyDown(Keys k) => _currentPressedKeys.Contains(k);
        public static bool IsKeyPressed(Keys k) => IsKeyDown(k) && !_lastPressedKeys.Contains(k);

        public static void Update(ScreenRenderSize screenRenderSize,
            TileModule tileModule)
        {
            var state = Mouse.GetState();

            int screenX = state.X - screenRenderSize.X;
            int screenY = state.Y - screenRenderSize.Y;

            screenX = (int)(screenX * ((double)tileModule.Specs.ScreenWidth / screenRenderSize.Width));
            screenY = (int)(screenY * ((double)tileModule.Specs.ScreenWidth / screenRenderSize.Width));

            MouseX = screenX + tileModule.Scroll.X;
            MouseY = screenY + tileModule.Scroll.Y - Constants.StatusBarHeight;

            LeftClicked = state.LeftButton == ButtonState.Pressed && !_leftWasPressed;
            RightClicked = state.RightButton == ButtonState.Pressed && !_rightWasPressed;

            _leftWasPressed = state.LeftButton == ButtonState.Pressed;
            _rightWasPressed = state.RightButton == ButtonState.Pressed;

            _lastPressedKeys = _currentPressedKeys;
            _currentPressedKeys = Keyboard.GetState().GetPressedKeys();
        }
    }
}
