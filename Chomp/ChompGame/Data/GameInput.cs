using ChompGame.Data.Memory;
using Microsoft.Xna.Framework.Input;

namespace ChompGame.Data
{
    class GameInput
    {
        private readonly GameBit _up, _down, _left, _right, _a, _b, _start;
        private readonly GameBit _wasUp, _wasDown, _wasLeft, _wasRight, _wasA, _wasB, _wasStart;
        private readonly GameByte _currentState, _lastState;

        private int _player;

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


        public GameInput(SystemMemoryBuilder memoryBuilder, int player)
        {
            _player = player;
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

            var state = Keyboard.GetState();

            if (_player == 1)
            {
                _up.Value = state.IsKeyDown(Keys.Up);
                _down.Value = state.IsKeyDown(Keys.Down);
                _left.Value = state.IsKeyDown(Keys.Left);
                _right.Value = state.IsKeyDown(Keys.Right);
                _a.Value = state.IsKeyDown(Keys.A);
                _b.Value = state.IsKeyDown(Keys.S);
                _start.Value = state.IsKeyDown(Keys.Space);
            }
            else if (_player == 2)
            {
                _up.Value = state.IsKeyDown(Keys.NumPad8);
                _down.Value = state.IsKeyDown(Keys.NumPad2);
                _left.Value = state.IsKeyDown(Keys.NumPad4);
                _right.Value = state.IsKeyDown(Keys.NumPad6);
                _a.Value = state.IsKeyDown(Keys.OemPeriod);
                _b.Value = state.IsKeyDown(Keys.OemQuestion);
                _start.Value = state.IsKeyDown(Keys.Enter);
            }
        }
    }
}
