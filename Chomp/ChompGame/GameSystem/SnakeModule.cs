using ChompGame.Data;
using ChompGame.Helpers;
using ChompGame.ROM;
using System;

namespace ChompGame.GameSystem
{
    class SnakeModule : Module, IMasterModule
    {
        enum GameState : byte 
        {
            PlaceNextTarget,
            Playing,
            CollectDot,
            LoseLife
        }

        enum Tile : byte
        {
            Dot1=1,
            Dot2=2,
            Snake_lr=4,
            Snake_ud=5,
            Snake_ur=6,
            Snake_ul=7,
            Snake_dl=8,
            Snake_dr=9
        }

        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private readonly BankAudioModule _audioModule;
        private readonly TileModule _tileModule;

        private NBitPlane _romPatternTable;
        private NBitPlane _romNameTable;

        private GameByte _soundTimer;
        private GameByte _soundVar;

        private ByteVector _snakeMotion;
        private GameByte _timer;
        
        private GameByteEnum<GameState> _gameState;
        private GameByte _nextTurnPosition;

        private GameBit _upPressed, _downPressed, _leftPressed, _rightPressed;

        private GameByte _maxSnakeSize;
        private GameByte _currentSnakeSize;

        private NibbleArray _positionHistory;
        private GameByte _throttle;
        private GameByte _collected;

        public SnakeModule(MainSystem mainSystem, InputModule inputModule, BankAudioModule audioModule, 
            SpritesModule spritesModule, TileModule tileModule) 
            : base(mainSystem)
        {
            _audioModule = audioModule;
            _inputModule = inputModule;
            _spritesModule = spritesModule;
            _tileModule = tileModule;
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _snakeMotion =new ByteVector(memoryBuilder.AddByte(),memoryBuilder.AddByte());
            _timer = memoryBuilder.AddByte();
            _soundTimer = memoryBuilder.AddByte();
            _soundVar = memoryBuilder.AddByte();
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());
            _throttle = memoryBuilder.AddByte();
            _collected = memoryBuilder.AddByte();

            _currentSnakeSize = memoryBuilder.AddByte();
            _maxSnakeSize = memoryBuilder.AddByte();

            _positionHistory = new NibbleArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(34);

            var lastInput = memoryBuilder.AddByte();
            _upPressed = new GameBit(lastInput.Address, Bit.Bit0, memoryBuilder.Memory);
            _downPressed = new GameBit(lastInput.Address, Bit.Bit1, memoryBuilder.Memory);
            _leftPressed = new GameBit(lastInput.Address, Bit.Bit2, memoryBuilder.Memory);
            _rightPressed = new GameBit(lastInput.Address, Bit.Bit3, memoryBuilder.Memory);

            _nextTurnPosition = memoryBuilder.AddByte();

            _romPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            _romNameTable = memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);
        }

        public void PlayBeep(MusicNote note, int octave, byte duration=4)
        {
            throw new NotImplementedException();
            //_audioModule.Channels[0].Volume = 128;
            //_audioModule.Channels[0].Value = SoundExtensions.GetFrequency(note, octave);
            //_audioModule.Channels[0].Playing = true;
            //_soundTimer.Value = duration;
        }

        public override void OnStartup()
        {
            _tileModule.Layer = 0;
            _spritesModule.Layer = 1;

            new DiskNBitPlaneLoader()
              .Load(new DiskFile(ContentFolder.PatternTables, "snake.pt"),
                  _romPatternTable);

            new DiskNBitPlaneLoader()
             .Load(new DiskFile(ContentFolder.NameTables, "snake.nt"),
                 _romNameTable);


            var graphicsModule = GameSystem.CoreGraphicsModule;
            _romPatternTable.CopyTo(graphicsModule.PatternTable, GameSystem.Memory);
            _romNameTable.CopyTo(_tileModule.NameTable, GameSystem.Memory);

            var snakeHead = _spritesModule.GetSprite(1);
            snakeHead.Tile = (byte)Tile.Snake_ud;
            snakeHead.X = 32;
            snakeHead.Y = 48;

            _snakeMotion.X = 0;
            _snakeMotion.Y = -1;

            _maxSnakeSize.Value = 2;
            _throttle.Value = 8;
            SetNextTurnPosition();

            var palette = graphicsModule.GetPalette(0);
            palette.SetColor(0, 13);
            palette.SetColor(1, 7);
            palette.SetColor(2, 8);
            palette.SetColor(3, 9);
        }

        private void ChangeGameState(GameState newState)
        {
            _gameState.Value = newState;
            switch(newState)
            {
                case GameState.Playing:
                    var palette = GameSystem.CoreGraphicsModule.GetPalette(0);
                    palette.SetColor(0, 13);
                    palette.SetColor(1, 7);
                    palette.SetColor(2, 8);
                    palette.SetColor(3, 9);
                    return;
                case GameState.LoseLife:
                case GameState.CollectDot:
                case GameState.PlaceNextTarget:
                    _timer.Value = 0;
                    return;
                default:
                    return;
            }
        }


        public void OnLogicUpdate()
        {
            _audioModule.OnLogicUpdate();
            _inputModule.OnLogicUpdate();

            if(_soundTimer.Value > 0)
            {
                _soundTimer.Value--;
                throw new NotImplementedException();
                //if (_soundTimer == 0)
                //    _audioModule.Channels[0].Playing = false;
            }

            switch(_gameState.Value)
            {
                case GameState.PlaceNextTarget:
                    UpdatePlaceNextTarget();
                    break;
                case GameState.Playing:
                    UpdatePlaying();
                    break;
                case GameState.CollectDot:
                    UpdateCollectDot();
                    break;
                case GameState.LoseLife:
                    UpdateLoseLife();
                    break;
                default:
                    break;
            }           
        }

        private void UpdateLoseLife()
        {
            if (_timer.Value == 0)
                PlayBeep(MusicNote.A, 0, 24);

            var palette = GameSystem.CoreGraphicsModule.GetPalette(0);

            switch (_timer.Value)
            {                
                case 4:                
                case 12:               
                case 20:    
                case 28:
                    palette.SetColor(3, 6);
                    break;
                case 8:
                case 16:
                case 24:
                    palette.SetColor(3, 9);
                    break;
            }

            _timer.Value++;
            if(_timer.Value==30)
            {
                _timer.Value = 0;
                _maxSnakeSize.Value = 2;
                _currentSnakeSize.Value = 0;
                _romNameTable.CopyTo(_tileModule.NameTable, GameSystem.Memory);

                ChangeGameState(GameState.PlaceNextTarget);

                var snakeHead = _spritesModule.GetSprite(1);
                snakeHead.Tile = (byte)Tile.Snake_ud;
                snakeHead.X = 32;
                snakeHead.Y = 48;

                _snakeMotion.X = 0;
                _snakeMotion.Y = -1;

                _maxSnakeSize.Value = 2;
                _throttle.Value = 8;
                _collected.Value = 0;
                SetNextTurnPosition();
            }
        }

        private void UpdateCollectDot()
        {
            if(_timer.Value == 0)
                PlayBeep(MusicNote.F, 2, 2);
            else if (_timer.Value == 16)
                PlayBeep(MusicNote.F, 3, 2);

            _timer.Value++;
            var target = _spritesModule.GetSprite(0);
            target.Tile = 0;

            
            if(_timer.Value==32)
            {
                _collected.Value++;

                if (_collected.Value == 2)
                    _throttle.Value = 6;
                if (_collected.Value == 4)
                    _throttle.Value = 4;
                else if (_collected.Value == 6)
                    _throttle.Value = 2;
                else if (_collected.Value == 10)
                    _throttle.Value = 1;

                if (_maxSnakeSize.Value < 32)
                {
                    _maxSnakeSize.Value += 4;
                    if (_maxSnakeSize.Value > 32)
                        _maxSnakeSize.Value = 32;
                }

                ChangeGameState(GameState.PlaceNextTarget);
            }
        }

        private void SetNextTurnPosition()
        {
            var snakeHead = _spritesModule.GetSprite(1);

            var tileX = snakeHead.X / Specs.TileWidth;
            var tileY = snakeHead.Y / Specs.TileHeight;
          
            if (_snakeMotion.Y < 0)
                _nextTurnPosition.Value = (byte)(tileY * Specs.TileHeight);
            else if (_snakeMotion.Y > 0)
                _nextTurnPosition.Value = (byte)((tileY + 1) * Specs.TileHeight);
            else if (_snakeMotion.X < 0)
                _nextTurnPosition.Value = (byte)(tileX * Specs.TileWidth);
            else
                _nextTurnPosition.Value = (byte)((tileX+1) * Specs.TileWidth);

            _upPressed.Value = false;
            _downPressed.Value = false;
            _leftPressed.Value = false;
            _rightPressed.Value = false;
        }

        private void AddSnakeTile(int tileX, int tileY, Tile tile)
        {
            if(_currentSnakeSize < _maxSnakeSize)
                _currentSnakeSize.Value++;
            else
            {
                var x = _positionHistory[0];
                var y = _positionHistory[1];
                _tileModule.NameTable[x, y] = 0;

                for(int i =0; i < _maxSnakeSize*2; i+=2)
                {
                    _positionHistory[i] = _positionHistory[i + 2];
                    _positionHistory[i+1] = _positionHistory[i + 3];
                }
            }
            
            _positionHistory[_currentSnakeSize * 2] = (byte)tileX;
            _positionHistory[(_currentSnakeSize * 2)+1] = (byte)tileY;

            if(_tileModule.NameTable[tileX, tileY] == 0)
                _tileModule.NameTable[tileX, tileY] = (byte)tile;
        }

        private bool CheckWallOrSelfCollision()
        {
            var snake = _spritesModule.GetSprite(1);
            var tileX = (byte)(snake.X / Specs.TileWidth);
            var tileY = (byte)(snake.Y / Specs.TileHeight);

            var tile = _tileModule.NameTable[tileX, tileY];

            //todo, self collision, but don't count second-to-last
            if (tile != 0)
            {
                if (tile == 14 || tile == 15)
                {
                    ChangeGameState(GameState.LoseLife);
                    return true;
                }

                var lastSnakeX = _positionHistory[_currentSnakeSize * 2];
                var lastSnakeY = _positionHistory[(_currentSnakeSize * 2)+1];

                if (tileX == lastSnakeX && tileY == lastSnakeY)
                    return false;

                ChangeGameState(GameState.LoseLife);
                return true;
            }

            return false;
        }

        private void CheckTurn()
        {
            var snakeHead = _spritesModule.GetSprite(1);
            byte tileX, tileY;

            if (_snakeMotion.Y < 0 && snakeHead.Y < _nextTurnPosition)
            {
                if (_leftPressed)
                {
                    snakeHead.Y = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = -1;
                    _snakeMotion.Y = 0;
                    AddSnakeTile(tileX, tileY,Tile.Snake_ul);
                    snakeHead.Tile = (byte)Tile.Snake_lr;
                }
                else if (_rightPressed)
                {
                    snakeHead.Y = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = 1;
                    _snakeMotion.Y = 0;
                    AddSnakeTile(tileX, tileY,Tile.Snake_ur);
                    snakeHead.Tile = (byte)Tile.Snake_lr;
                }
                else
                {
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);

                    AddSnakeTile(tileX, tileY + 1,Tile.Snake_ud);
                }

                SetNextTurnPosition();
            }
            else if (_snakeMotion.Y > 0 && snakeHead.Y > _nextTurnPosition)
            {
                if (_leftPressed)
                {
                    snakeHead.Y = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = -1;
                    _snakeMotion.Y = 0;
                    AddSnakeTile(tileX, tileY,Tile.Snake_dl);
                    snakeHead.Tile = (byte)Tile.Snake_lr;
                }
                else if (_rightPressed)
                {
                    snakeHead.Y = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = 1;
                    _snakeMotion.Y = 0;
                    AddSnakeTile(tileX, tileY,Tile.Snake_dr);
                    snakeHead.Tile = (byte)Tile.Snake_lr;
                }
                else
                {
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);

                    AddSnakeTile(tileX, tileY,Tile.Snake_ud);
                }

                SetNextTurnPosition();
            }
            else if (_snakeMotion.X < 0 && snakeHead.X < _nextTurnPosition)
            {
                if (_upPressed)
                {
                    snakeHead.X = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = 0;
                    _snakeMotion.Y = -1;

                    AddSnakeTile(tileX, tileY, Tile.Snake_dr);
                    snakeHead.Tile = (byte)Tile.Snake_ud;
                }
                else if (_downPressed)
                {
                    snakeHead.X = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = 0;
                    _snakeMotion.Y = 1;
                    AddSnakeTile(tileX, tileY,Tile.Snake_ur);
                    snakeHead.Tile = (byte)Tile.Snake_ud;
                }
                else
                {
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    AddSnakeTile(tileX + 1, tileY,Tile.Snake_lr);
                }

                SetNextTurnPosition();
            }
            else if (_snakeMotion.X > 0 && snakeHead.X > _nextTurnPosition)
            {
                if (_upPressed)
                {
                    snakeHead.X = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = 0;
                    _snakeMotion.Y = -1;
                    AddSnakeTile(tileX, tileY,Tile.Snake_dl);
                    snakeHead.Tile = (byte)Tile.Snake_ud;
                }
                else if (_downPressed)
                {
                    snakeHead.X = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = 0;
                    _snakeMotion.Y = 1;
                    AddSnakeTile(tileX, tileY,Tile.Snake_ul);
                    snakeHead.Tile = (byte)Tile.Snake_ud;
                }
                else
                {
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);

                    AddSnakeTile(tileX, tileY,Tile.Snake_lr);
                }

                SetNextTurnPosition();
            }
        }

        private void UpdatePlaying()
        {
            _timer.Value++;

            if((_timer.Value % 16) == 0)
            {
                var dot = _spritesModule.GetSprite(0);
                if (dot.Tile == (byte)Tile.Dot1 )
                    dot.Tile = (byte)Tile.Dot2;
                else
                    dot.Tile = (byte)Tile.Dot1;
            }

            if (_inputModule.Player1.UpKey.IsDown())
                _upPressed.Value = true;
            if (_inputModule.Player1.DownKey.IsDown())
                _downPressed.Value = true;
            if (_inputModule.Player1.LeftKey.IsDown())
                _leftPressed.Value = true;
            if (_inputModule.Player1.RightKey.IsDown())
                _rightPressed.Value = true;


            if ((_timer.Value % _throttle.Value) == 0)
            {
                var snakeHead = _spritesModule.GetSprite(1);
                snakeHead.Y = (byte)(snakeHead.Y + _snakeMotion.Y);
                snakeHead.X = (byte)(snakeHead.X + _snakeMotion.X);

                if (CheckWallOrSelfCollision())
                    return;

                CheckTurn();
                CheckCollectTarget();
            }

            UpdateMelody();
        }

        private void UpdateMelody()
        {
            if ((_timer.Value % 32) == 0)
            {
                switch (_soundVar.Value)
                {
                    case 0:
                    case 2:
                    case 4:
                        PlayBeep(MusicNote.C, 0, 8);
                        break;
                    case 1:
                    case 3:
                    case 5:
                    case 7:
                        PlayBeep(MusicNote.E, 0, 8);
                        break;
                    case 6:
                        PlayBeep(MusicNote.DSharp, 0, 8);
                        break;
                }

                _soundVar.Value++;

                if (_collected.Value == 0)
                    _soundVar.Value = 0;
                else if (_collected.Value < 5 && _soundVar >= 2)
                    _soundVar.Value = 0;
                else if (_soundVar >= 8)
                    _soundVar.Value = 0;

            }

        }

        private void CheckCollectTarget()
        {
            var target = _spritesModule.GetSprite(0);
            var snake = _spritesModule.GetSprite(1);

            if (snake.Right < target.X
               || snake.X > target.Right
               || snake.Bottom < target.Y
               || snake.Y > target.Bottom) return;

            ChangeGameState(GameState.CollectDot);
        }

        private void UpdatePlaceNextTarget()
        {
            //todo, more "accurate" way of doing random
            var x = 8 + new Random().Next(Specs.ScreenWidth - 16);
            var y = 8 + new Random().Next(Specs.ScreenHeight - 16);

            var dot = _spritesModule.GetSprite(0);
            dot.X = (byte)x;
            dot.Y = (byte)y;

            ChangeGameState(GameState.Playing);
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
