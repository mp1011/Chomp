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
        private readonly AudioModule _audioModule;
        private readonly TileModule _tileModule;

        private NBitPlane _romPatternTable;

        private GameByte _soundTimer;
        private ByteVector _snakeMotion;
        private GameByte _timer;
        private GameByte _score;
        
        private GameByteEnum<GameState> _gameState;
        private GameByte _nextTurnPosition;

        private GameBit _upPressed, _downPressed, _leftPressed, _rightPressed;

        public SnakeModule(MainSystem mainSystem, InputModule inputModule, AudioModule audioModule, 
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
            _score = memoryBuilder.AddByte();
            _soundTimer = memoryBuilder.AddByte();
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());

            var lastInput = memoryBuilder.AddByte();
            _upPressed = new GameBit(lastInput.Address, Bit.Bit0, memoryBuilder.Memory);
            _downPressed = new GameBit(lastInput.Address, Bit.Bit1, memoryBuilder.Memory);
            _leftPressed = new GameBit(lastInput.Address, Bit.Bit2, memoryBuilder.Memory);
            _rightPressed = new GameBit(lastInput.Address, Bit.Bit3, memoryBuilder.Memory);

            _nextTurnPosition = memoryBuilder.AddByte();
            memoryBuilder.BeginROM();

            _romPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
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
              .Load(new DiskFile(ContentFolder.PatternTables, "snake.pt"),
                  _romPatternTable);

             var graphicsModule = GameSystem.CoreGraphicsModule;
            _romPatternTable.CopyTo(graphicsModule.PatternTable, GameSystem.Memory);

            var snakeHead = _spritesModule.GetSprite(1);
            snakeHead.Tile = (byte)Tile.Snake_ud;
            snakeHead.X = 32;
            snakeHead.Y = 48;

            _snakeMotion.X = 0;
            _snakeMotion.Y = -1;

            SetNextTurnPosition();
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
                case GameState.PlaceNextTarget:
                    UpdatePlaceNextTarget();
                    break;
                case GameState.Playing:
                    UpdatePlaying();
                    break;
                default:
                    break;
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
                    _tileModule.NameTable[tileX, tileY] = (byte)Tile.Snake_ul;
                    snakeHead.Tile = (byte)Tile.Snake_lr;
                }
                else if (_rightPressed)
                {
                    snakeHead.Y = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = 1;
                    _snakeMotion.Y = 0;
                    _tileModule.NameTable[tileX, tileY] = (byte)Tile.Snake_ur;
                    snakeHead.Tile = (byte)Tile.Snake_lr;
                }
                else
                {
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);

                    if (_tileModule.NameTable[tileX, tileY + 1] == 0)
                        _tileModule.NameTable[tileX, tileY + 1] = (byte)Tile.Snake_ud;
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
                    _tileModule.NameTable[tileX, tileY] = (byte)Tile.Snake_dl;
                    snakeHead.Tile = (byte)Tile.Snake_lr;
                }
                else if (_rightPressed)
                {
                    snakeHead.Y = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = 1;
                    _snakeMotion.Y = 0;
                    _tileModule.NameTable[tileX, tileY] = (byte)Tile.Snake_dr;
                    snakeHead.Tile = (byte)Tile.Snake_lr;
                }
                else
                {
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);

                    if (_tileModule.NameTable[tileX, tileY] == 0)
                        _tileModule.NameTable[tileX, tileY] = (byte)Tile.Snake_ud;
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
                    _tileModule.NameTable[tileX, tileY] = (byte)Tile.Snake_dr;
                    snakeHead.Tile = (byte)Tile.Snake_ud;
                }
                else if (_downPressed)
                {
                    snakeHead.X = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = 0;
                    _snakeMotion.Y = 1;
                    _tileModule.NameTable[tileX, tileY] = (byte)Tile.Snake_ur;
                    snakeHead.Tile = (byte)Tile.Snake_ud;
                }
                else
                {
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);

                    if (_tileModule.NameTable[tileX + 1, tileY] == 0)
                        _tileModule.NameTable[tileX + 1, tileY] = (byte)Tile.Snake_lr;
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
                    _tileModule.NameTable[tileX, tileY] = (byte)Tile.Snake_dl;
                    snakeHead.Tile = (byte)Tile.Snake_ud;
                }
                else if (_downPressed)
                {
                    snakeHead.X = _nextTurnPosition;
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);
                    _snakeMotion.X = 0;
                    _snakeMotion.Y = 1;
                    _tileModule.NameTable[tileX, tileY] = (byte)Tile.Snake_ul;
                    snakeHead.Tile = (byte)Tile.Snake_ud;
                }
                else
                {
                    tileX = (byte)(snakeHead.X / Specs.TileWidth);
                    tileY = (byte)(snakeHead.Y / Specs.TileHeight);

                    _tileModule.NameTable[tileX, tileY] = (byte)Tile.Snake_lr;
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


            if ((_timer.Value % 8) == 0)
            {
                var snakeHead = _spritesModule.GetSprite(1);
                snakeHead.Y = (byte)(snakeHead.Y + _snakeMotion.Y);
                snakeHead.X = (byte)(snakeHead.X + _snakeMotion.X);

                CheckTurn();
            }
        }

        private void UpdatePlaceNextTarget()
        {
            //todo, more "accurate" way of doing random
            var x = new Random().Next(Specs.ScreenWidth);
            var y = new Random().Next(Specs.ScreenHeight);

            var dot = _spritesModule.GetSprite(0);
            dot.X = (byte)x;
            dot.Y = (byte)y;

            _gameState.Value = GameState.Playing;
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
    }
}
