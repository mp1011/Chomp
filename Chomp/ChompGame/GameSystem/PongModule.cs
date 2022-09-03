using ChompGame.Data;
using ChompGame.Helpers;
using ChompGame.ROM;
using System;

namespace ChompGame.GameSystem
{
    class PongModule : Module, IMasterModule
    {
        enum GameState : byte 
        {
            Intro,
            BeforePlay,
            Playing,
            GoalScored,
            GameOver
        }

        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private readonly TileModule _tileModule;
        private readonly AudioModule _audioModule;

        private NBitPlane _romPatternTable;
        private NBitPlane _romNameTable;

        private ByteVector _ballMotion;
        private GameShort _timer;
        private GameByte _p1Score;
        private GameByte _p2Score;
        private GameByte _soundTimer;

        private GameByteEnum<GameState> _gameState;
        private GameByte _var;
        public PongModule(MainSystem mainSystem, InputModule inputModule, AudioModule audioModule, SpritesModule spritesModule,
            TileModule tileModule) 
            : base(mainSystem)
        {
            _audioModule = audioModule;
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
            _soundTimer = memoryBuilder.AddByte();
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());
            _var = memoryBuilder.AddByte();
            memoryBuilder.BeginROM();

            _romPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            _romNameTable = memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);
        }

        public void PlayBeep(MusicNote note, int octave, byte duration=4)
        {
            _audioModule.Channels[0].Volume = 128;
            _audioModule.Channels[0].Value = SoundHelper.GetNote(note, octave);
            _audioModule.Channels[0].Playing = true;
            _soundTimer.Value = duration;
        }

        public override void OnStartup()
        {
            _tileModule.Layer = 0;
            _spritesModule.Layer = 1;

            new DiskNBitPlaneLoader()
               .Load(new DiskFile(ContentFolder.PatternTables, "pong.pt"),
                   _romPatternTable);

            new DiskNBitPlaneLoader()
                .Load(
                    new DiskFile(ContentFolder.NameTables, "pong.nt"),
                    _romNameTable);

            var tileModule = GameSystem.GetModule<TileModule>();
            var graphicsModule = GameSystem.CoreGraphicsModule;

            _romNameTable.CopyTo(tileModule.NameTable, GameSystem.Memory);
            _romPatternTable.CopyTo(graphicsModule.PatternTable, GameSystem.Memory);
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
            _audioModule.OnLogicUpdate();
            _inputModule.OnLogicUpdate();

            if(_soundTimer.Value > 0)
            {
                _soundTimer.Value--;
                if (_soundTimer == 0)
                    _audioModule.Channels[0].Playing = false;
            }

            switch(_gameState.Value)
            {
                case GameState.Intro:
                    UpdateIntro();
                    break;
                case GameState.BeforePlay:
                    UpdateBeforePlay();
                    break;
                case GameState.GoalScored:
                    UpdateGoalScored();
                    break;
                case GameState.Playing:
                    UpdatePlaying();
                    break;
                case GameState.GameOver:
                    UpdateGameOver();
                    break;

                default:

                    break;
            }

            
        }

        private void UpdateIntro()
        {
            _timer.Value++;
            if(_timer.Value == 32)
            {
                _timer.Value = 0;

                var t = _tileModule.NameTable[1, 1];

                for(int x = 1; x<=6;x++)
                {
                    for(int y =1; y<=6;y++)
                    {
                        if (t == 0)
                            _tileModule.NameTable[x, y] = 0xC;
                        else
                            _tileModule.NameTable[x, y] = 0;
                    }
                }
            }

            if (_inputModule.Player1.StartKey == GameKeyState.Pressed)
            {
                _gameState.Value = GameState.BeforePlay;
                _timer.Value = 0;
                _var.Value = 0;
                _p1Score.Value = 0;
                _p2Score.Value = 0;
            }
        }


        private void UpdateBeforePlay()
        {
            if(_var.Value==0)
            {
                _romNameTable.CopyTo(_tileModule.NameTable, GameSystem.Memory);
                _var.Value++;

                _spritesModule.Sprites[0].X = 1;
                _spritesModule.Sprites[0].Y = 12;
                _spritesModule.Sprites[0].Tile = 1;

                _spritesModule.Sprites[1].X = 27;
                _spritesModule.Sprites[1].Y = 12;
                _spritesModule.Sprites[1].Tile = 2;

                _spritesModule.Sprites[2].Tile = 3;

                CenterBall();
            }

            _timer.Value++;
            if(_timer.Value == 16)
            {
                _timer.Value = 0;
                _var.Value++;

                switch(_var.Value)
                {
                    case 2:
                        PlayBeep(MusicNote.A, 2, duration:16);
                        break;
                    case 3:
                        PlayBeep(MusicNote.C, 2, duration: 16);
                        break;
                    case 4:
                        PlayBeep(MusicNote.E, 2, duration: 16);
                        break;
                    case 5:
                        PlayBeep(MusicNote.A, 3, duration: 16);
                        break;
                    case 6:
                        _gameState.Value = GameState.Playing;
                        _ballMotion.X = 1;                        
                        break;
                }
            }
        }

        private void UpdatePlaying()
        {
            ShowScores();

            _timer.Value++;

            UpdatePaddle(_spritesModule.GetSprite(0), _inputModule.Player1);
            UpdatePaddle(_spritesModule.GetSprite(1), _inputModule.Player2);

            if ((_timer.Value % 3) == 0)
                HandleBallMotion();
        }

        private void UpdateGoalScored()
        {
            if(_var.Value == 1)
            {
                if (_p1Score < 2)
                    _p1Score.Value++;
                else
                {
                    _gameState.Value = GameState.GameOver;
                    return;
                }

                _var.Value = 3;
            }
            else if(_var.Value==2)
            {
                PlayBeep(MusicNote.A, 3);

                if (_p2Score < 2)
                    _p2Score.Value++;
                else
                {
                    _gameState.Value = GameState.GameOver;
                    return;
                }

                _var.Value = 3;
            }

            _timer.Value++;
            if(_timer.Value == 8)
            {
                _timer.Value = 0;
                if (_var.Value == 3)
                    PlayBeep(MusicNote.F, 2, 8);
                else if (_var.Value == 4)
                    PlayBeep(MusicNote.ESharp, 2, 8);
                else if (_var.Value == 5)
                    PlayBeep(MusicNote.E, 2, 8);
                else if (_var.Value == 6)
                    PlayBeep(MusicNote.DSharp, 2, 8);
                else if (_var.Value == 7)
                {
                    _gameState.Value = GameState.Playing;
                    CenterBall();
                }

                _var.Value++;
            }
        }

        private void UpdateGameOver()
        {
            _timer.Value++;
            if(_timer.Value == 16)
            {
                _timer.Value = 0;

                if (_var.Value < 10)
                    _var.Value = 10;

                if (_var.Value == 10)
                    PlayBeep(MusicNote.A, 3, 8);
                if (_var.Value == 11)
                    PlayBeep(MusicNote.A, 3, 8);
                if (_var.Value == 12)
                    PlayBeep(MusicNote.A, 3, 8);
                if (_var.Value == 13)
                    PlayBeep(MusicNote.D, 2, 8);
                if (_var.Value == 14)
                    PlayBeep(MusicNote.A, 2, 8);
                if (_var.Value == 15)
                    PlayBeep(MusicNote.A, 2, 8);
                if (_var.Value == 16)
                    PlayBeep(MusicNote.A, 3, 8);
                if (_var.Value == 17)
                    PlayBeep(MusicNote.ASharp, 3, 8);
                if (_var.Value == 18)
                    PlayBeep(MusicNote.B, 3, 8);
                if (_var.Value == 19)
                    PlayBeep(MusicNote.D, 3, 8);
                if (_var.Value == 20)
                    _gameState.Value = GameState.Intro;

                _var.Value++;
            }
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

        private void CenterBall()
        {
            var ball = _spritesModule.GetSprite(2);
            ball.X = 12;
            ball.Y = 12;
            _ballMotion.Y = 0;
            _ballMotion.X = Random(-1, 1);
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
                PlayBeep(MusicNote.G,3);
                _ballMotion.X = 1;
                _ballMotion.Y = Random(-2, -1, 1, 2);
            }
            else if (CheckCollision(paddle2, ball))
            {
                PlayBeep(MusicNote.C, 3);
                _ballMotion.X = -1;
                _ballMotion.Y = Random(-2, -1, 1, 2);
            }
            else if (ball.X > Specs.ScreenWidth-Specs.TileWidth)
            {
                _gameState.Value = GameState.GoalScored;
                _timer.Value = 0;
                _var.Value = 1;
            }
            else if (ball.X < 2)
            {
                _gameState.Value = GameState.GoalScored;
                _timer.Value = 0;
                _var.Value = 2;
            }
            else if (ball.Y > Specs.ScreenHeight-Specs.TileHeight)
            {
                PlayBeep(MusicNote.D, 3);
                _ballMotion.Y = Random(0,-1,-2);
            }
            else if (ball.Y < 2)
            {
                PlayBeep(MusicNote.D, 3);
                _ballMotion.Y = Random(0,1, 2);
            }
        }

        private int Random(params int[] choices)
        {
            //todo, don't rely on Random
            var r = new Random(_timer.Value);
            return choices[r.Next(choices.Length)];
        }

        public void OnVBlank()
        {
            _tileModule.OnVBlank();
            _spritesModule.OnVBlank();
        }

        public void OnHBlank()
        {
            _tileModule.OnHBlank();
            _spritesModule.OnHBlank();
        }

        public byte GetPalette(int pixel)
        {
            return 0;
        }
    }
}
