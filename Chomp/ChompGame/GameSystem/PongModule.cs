using ChompGame.Data;
using ChompGame.ROM;

namespace ChompGame.GameSystem
{
    class PongModule : Module, ILogicUpdateHandler
    {
        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private ByteVector _ballMotion;
        private GameShort _timer;

        public PongModule(MainSystem mainSystem, InputModule inputModule, SpritesModule spritesModule) 
            : base(mainSystem)
        {
            _inputModule = inputModule;
            _spritesModule = spritesModule;
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _ballMotion =new ByteVector(memoryBuilder.AddByte(),memoryBuilder.AddByte());
            _timer = memoryBuilder.AddShort();
        }

        public override void OnStartup()
        {
            var tileModule = GameSystem.GetModule<TileModule>();
            var graphicsModule = GameSystem.CoreGraphicsModule;
           
            var nameTableLoader = new DiskNBitPlaneLoader();
            nameTableLoader.Load(
               new DiskFile(ContentFolder.NameTables, "pong.nt"),
               tileModule.NameTable);

            var patternTableLoader = new DiskNBitPlaneLoader();
            patternTableLoader.Load(
                new DiskFile(ContentFolder.PatternTables, "pong.pt"),
                graphicsModule.PatternTable);

            _spritesModule.Sprites[0].X = 1;
            _spritesModule.Sprites[0].Y = 12;
            _spritesModule.Sprites[0].Tile = 1;

            _spritesModule.Sprites[1].X = 27;
            _spritesModule.Sprites[1].Y = 12;
            _spritesModule.Sprites[1].Tile = 2;

            _spritesModule.Sprites[2].X = 12;
            _spritesModule.Sprites[2].Y = 12;
            _spritesModule.Sprites[2].Tile = 3;

            _ballMotion.X = 0;
            _ballMotion.Y = 0;
        }

        public void OnLogicUpdate()
        {
            _timer.Value++;

            var playerPaddle = _spritesModule.GetSprite(0);
            if (playerPaddle.Y > 0 && _inputModule.UpKey.IsDown())
                playerPaddle.Y--;
            else if (playerPaddle.Y < Specs.ScreenHeight-Specs.TileHeight && _inputModule.DownKey.IsDown())
                playerPaddle.Y++;

            if(_ballMotion.X==0 && _ballMotion.Y == 0 && _inputModule.StartKey == GameKeyState.Pressed)
                _ballMotion.X = 1;
            else if(_inputModule.StartKey == GameKeyState.Pressed)
            {
                _ballMotion.X = 0;
                _ballMotion.Y = 0;
            }

            HandleBallMotion();
        }

        private void HandleBallMotion()
        {
            var ball = _spritesModule.GetSprite(2);
            ball.X = (byte)(ball.X + _ballMotion.X);
            ball.Y = (byte)(ball.Y + _ballMotion.Y);

            if (ball.X > Specs.ScreenWidth-Specs.TileWidth)
            {
                _ballMotion.X = -1;
                _ballMotion.Y = Random(-2, -1, 1, 2);
            }
            else if (ball.X < 2)
            {
                _ballMotion.X = 1;
                _ballMotion.Y = Random(-2, -1, 1, 2);
            }

            if (ball.Y > Specs.ScreenHeight-Specs.TileHeight)
            {
                _ballMotion.Y = Random(0,-1,-2);
            }
            else if (ball.Y < 2)
            {
                _ballMotion.Y = Random(0,1, 2);
            }
        }

        private int Random(params int[] choices)
        {
             var r = _timer.Value % choices.Length;
            return choices[r];
        }
    }
}
