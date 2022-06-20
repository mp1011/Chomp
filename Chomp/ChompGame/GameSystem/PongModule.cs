using ChompGame.Data;
using ChompGame.ROM;
using System;

namespace ChompGame.GameSystem
{
    class PongModule : Module, ILogicUpdateHandler
    {
        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private readonly TileModule _tileModule;

        private ByteVector _ballMotion;
        private GameShort _timer;
        private GameByte _p1Score;
        private GameByte _p2Score;

        public PongModule(MainSystem mainSystem, InputModule inputModule, SpritesModule spritesModule,
            TileModule tileModule) 
            : base(mainSystem)
        {
            _inputModule = inputModule;
            _spritesModule = spritesModule;
            _tileModule = tileModule;
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _ballMotion =new ByteVector(memoryBuilder.AddByte(),memoryBuilder.AddByte());
            _timer = memoryBuilder.AddShort();
            _p1Score = memoryBuilder.AddByte();
            _p2Score = memoryBuilder.AddByte();
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

        public void UpdatePaddle(Sprite playerPaddle, GameInput input)
        {
            if (playerPaddle.Y > 0 && input.UpKey.IsDown())
                playerPaddle.Y--;
            else if (playerPaddle.Y < Specs.ScreenHeight - Specs.TileHeight && input.DownKey.IsDown())
                playerPaddle.Y++;
        }

        public void OnLogicUpdate()
        {
            ShowScores();

            _timer.Value++;

            UpdatePaddle(_spritesModule.GetSprite(0), _inputModule.Player1);
            UpdatePaddle(_spritesModule.GetSprite(1), _inputModule.Player2);


            if (_ballMotion.X == 0 && _ballMotion.Y == 0 && _inputModule.Player1.StartKey == GameKeyState.Pressed)
            {
                _ballMotion.X = 1;
                _p1Score.Value = 0;
                _p2Score.Value = 0;
            }
            else if (_inputModule.Player1.StartKey == GameKeyState.Pressed)
            {
                _ballMotion.X = 0;
                _ballMotion.Y = 0;
            }

            if((_timer.Value % 3) == 0)
                HandleBallMotion();
        }

        private void ShowScores()
        {
            switch(_p1Score.Value)
            {
                case 1:
                    _tileModule.NameTable[1, 1] = 0xD;
                    _tileModule.NameTable[2, 1] = 0xC;
                    break;
                case 2:
                    _tileModule.NameTable[1, 1] = 0xD;
                    _tileModule.NameTable[2, 1] = 0xD;
                    break;
                default:
                    _tileModule.NameTable[1, 1] = 0xC;
                    _tileModule.NameTable[2, 1] = 0xC;
                    break;
            }

            switch (_p2Score.Value)
            {
                case 1:
                    _tileModule.NameTable[5, 1] = 0xD;
                    _tileModule.NameTable[6, 1] = 0xC;
                    break;
                case 2:
                    _tileModule.NameTable[5, 1] = 0xD;
                    _tileModule.NameTable[6, 1] = 0xD;
                    break;
                default:
                    _tileModule.NameTable[5, 1] = 0xC;
                    _tileModule.NameTable[6, 1] = 0xC;
                    break;
            }

        }

        private bool CheckCollision(Sprite s1, Sprite s2)
        {
            return !(s2.Right < s1.X
                || s2.X > s1.Right
                || s2.Bottom < s1.Y
                || s2.Y > s1.Bottom);                
        }

        private void HandleBallMotion()
        {
            var ball = _spritesModule.GetSprite(2);
            ball.X = (byte)(ball.X + _ballMotion.X);
            ball.Y = (byte)(ball.Y + _ballMotion.Y);

            var paddle1 = _spritesModule.GetSprite(0);
            var paddle2 = _spritesModule.GetSprite(1);


            if (CheckCollision(paddle1,ball))
            {
                _ballMotion.X = 1;
                _ballMotion.Y = Random(-2, -1, 1, 2);
            }
            else if (CheckCollision(paddle2, ball))
            {
                _ballMotion.X = -1;
                _ballMotion.Y = Random(-2, -1, 1, 2);
            }
            else if (ball.X > Specs.ScreenWidth-Specs.TileWidth)
            {
                if (_p1Score < 2)
                    _p1Score.Value++;

                _ballMotion.X = -1;
                _ballMotion.Y = Random(-2, -1, 1, 2);
            }
            else if (ball.X < 2)
            {
                if (_p2Score < 2)
                    _p2Score.Value++;

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
            //todo, don't rely on Random
            var r = new Random(_timer.Value);
            return choices[r.Next(choices.Length)];
        }
    }
}
