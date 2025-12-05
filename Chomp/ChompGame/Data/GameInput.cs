using ChompGame.Data.Memory;
using ChompGame.Option;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChompGame.Data
{
    class GameInput
    {
        private readonly GameBit _up, _down, _left, _right, _a, _b, _start;
        private readonly GameBit _wasUp, _wasDown, _wasLeft, _wasRight, _wasA, _wasB, _wasStart;
        private readonly GameByte _currentState, _lastState;

        private GameOptions _options;

        public GameKeyState UpKey => GetState(_up, _wasUp);
        public GameKeyState DownKey => GetState(_down, _wasDown);
        public GameKeyState LeftKey => GetState(_left, _wasLeft);
        public GameKeyState RightKey => GetState(_right, _wasRight);
        public GameKeyState AKey => GetState(_a, _wasA);
        public GameKeyState BKey => GetState(_b, _wasB);
        public GameKeyState StartKey => GetState(_start, _wasStart);

        public bool AnyWasUp() => UpKey == GameKeyState.Released 
            || DownKey == GameKeyState.Released
            || LeftKey == GameKeyState.Released || RightKey == GameKeyState.Released
            || AKey == GameKeyState.Released
            || BKey == GameKeyState.Released
            || StartKey == GameKeyState.Released;


        public GameInput(SystemMemoryBuilder memoryBuilder, GameOptions options)
        {
            _options = options;
            var currentStateAddress = memoryBuilder.CurrentAddress;
            _currentState = memoryBuilder.AddByte();
            _lastState = memoryBuilder.AddByte();

            _up = new GameBit(currentStateAddress, Bit.Bit0, memoryBuilder.Memory);
            _down = new GameBit(currentStateAddress, Bit.Bit1, memoryBuilder.Memory);
            _left = new GameBit(currentStateAddress, Bit.Bit2, memoryBuilder.Memory);
            _right = new GameBit(currentStateAddress, Bit.Bit3, memoryBuilder.Memory);
            _a = new GameBit(currentStateAddress, Bit.Bit4, memoryBuilder.Memory);
            _b = new GameBit(currentStateAddress, Bit.Bit5, memoryBuilder.Memory);
            _start = new GameBit(currentStateAddress, Bit.Bit6, memoryBuilder.Memory);

            _wasUp = new GameBit(currentStateAddress + 1, Bit.Bit0, memoryBuilder.Memory);
            _wasDown = new GameBit(currentStateAddress + 1, Bit.Bit1, memoryBuilder.Memory);
            _wasLeft = new GameBit(currentStateAddress + 1, Bit.Bit2, memoryBuilder.Memory);
            _wasRight = new GameBit(currentStateAddress + 1, Bit.Bit3, memoryBuilder.Memory);
            _wasA = new GameBit(currentStateAddress + 1, Bit.Bit4, memoryBuilder.Memory);
            _wasB = new GameBit(currentStateAddress + 1, Bit.Bit5, memoryBuilder.Memory);
            _wasStart = new GameBit(currentStateAddress + 1, Bit.Bit6, memoryBuilder.Memory);
        }

        private GameKeyState GetState(GameBit current, GameBit last)
        {
            if (current.Value && last.Value)
                return GameKeyState.Down;
            else if (current.Value && !last.Value)
                return GameKeyState.Pressed;
            else if (!current.Value && last.Value)
                return GameKeyState.Released;
            else
                return GameKeyState.Up;
        }

        public void Update()
        {
            _lastState.Value = _currentState.Value;

            var keyState = Keyboard.GetState();
            var padState = GamePad.GetState(PlayerIndex.One);

            float analogThreshold = 0.3f;

            if (_options.HasBindings)
            {
                _up.Value = IsKeyDown(GameKey.Up, keyState, padState) || padState.ThumbSticks.Left.Y >= analogThreshold;
                _down.Value = IsKeyDown(GameKey.Down, keyState, padState) || padState.ThumbSticks.Left.Y <= -analogThreshold;
                _left.Value = IsKeyDown(GameKey.Left, keyState, padState) || padState.ThumbSticks.Left.X <= -analogThreshold;
                _right.Value = IsKeyDown(GameKey.Right, keyState, padState) || padState.ThumbSticks.Left.X >= analogThreshold;
                _a.Value = IsKeyDown(GameKey.Jump, keyState, padState);
                _b.Value = IsKeyDown(GameKey.Throw, keyState, padState);
                _start.Value = IsKeyDown(GameKey.Start, keyState, padState);
            }
            else
            {
                _up.Value = keyState.IsKeyDown(Keys.Up) || padState.IsButtonDown(Buttons.DPadUp) || padState.ThumbSticks.Left.Y >= analogThreshold;
                _down.Value = keyState.IsKeyDown(Keys.Down) || padState.IsButtonDown(Buttons.DPadDown) || padState.ThumbSticks.Left.Y <= -analogThreshold;
                _left.Value = keyState.IsKeyDown(Keys.Left) || padState.IsButtonDown(Buttons.DPadLeft) || padState.ThumbSticks.Left.X <= -analogThreshold;
                _right.Value = keyState.IsKeyDown(Keys.Right) || padState.IsButtonDown(Buttons.DPadRight) || padState.ThumbSticks.Left.X >= analogThreshold;
                _a.Value = keyState.IsKeyDown(Keys.A) || padState.IsButtonDown(Buttons.A) || padState.IsButtonDown(Buttons.Y);
                _b.Value = keyState.IsKeyDown(Keys.S) || padState.IsButtonDown(Buttons.B) || padState.IsButtonDown(Buttons.X);
                _start.Value = keyState.IsKeyDown(Keys.Space) || padState.IsButtonDown(Buttons.Start);
            }
        }

        private bool IsKeyDown(GameKey key, KeyboardState keyState, GamePadState padState)
        {
            if (_options.KeyboardBindings.ContainsKey(key))
            {
                return keyState.IsKeyDown(_options.KeyboardBindings[key]);
            }
            else if (_options.GamePadBindings.ContainsKey(key))
            {
                return padState.IsButtonDown(_options.GamePadBindings[key]);
            }
            else
                return false;
        }
    }
}
