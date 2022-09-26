using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.ROM;

namespace ChompGame.GameSystem
{
    class PlatformerModule : Module, IMasterModule
    {
        public enum PaletteIndex
        {
            Background = 0,
            Sprites,
            Info
        }

        public enum GameState : byte
        {
            Normal=0,
            LoseLife=1,
            ResetLevel=2
        }


        private CollisionDetector _collisionDetector;
        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private readonly BankAudioModule _audioModule;
        private readonly TileModule _tileModule;
        private readonly StatusBarModule _statusBarModule;

        private GameByteEnum<GameState> _gameState;
        private GameByte _timer;
        private GameByte _bulletTimer;

        private NBitPlane _romPatternTable;
        private NBitPlane _romNameTable;

        private MovingSprite _player;
        private MovingSprite _enemy;
        private MovingSprite _bullet;

        private GameByte _playerHitTimer;

        public PlatformerModule(MainSystem mainSystem, InputModule inputModule, BankAudioModule audioModule,
            SpritesModule spritesModule, TileModule tileModule, StatusBarModule statusBarModule)
            : base(mainSystem)
        {
            _audioModule = audioModule;
            _inputModule = inputModule;
            _spritesModule = spritesModule;
            _tileModule = tileModule;
            _statusBarModule = statusBarModule;
            _collisionDetector = new CollisionDetector(tileModule, Specs);
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());
            _player = new MovingSprite(_spritesModule, memoryBuilder);
            _enemy = new MovingSprite(_spritesModule, memoryBuilder);
            _bullet = new MovingSprite(_spritesModule, memoryBuilder);

            _timer = memoryBuilder.AddByte();
            _bulletTimer = memoryBuilder.AddByte();
            _playerHitTimer = memoryBuilder.AddByte();

            memoryBuilder.BeginROM();
            _romPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            _romNameTable = memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);
        }

        public override void OnStartup()
        {
            var bgPalette = GameSystem.CoreGraphicsModule.GetPalette(0);
            bgPalette.SetColor(0, 0);
            bgPalette.SetColor(1, 1);
            bgPalette.SetColor(2, 2);
            bgPalette.SetColor(3, 3);

            var playerPalette = GameSystem.CoreGraphicsModule.GetPalette(1);
            playerPalette.SetColor(0, 6);
            playerPalette.SetColor(1, 6);
            playerPalette.SetColor(2, 14);
            playerPalette.SetColor(3, 13);

            var enemyPalette = GameSystem.CoreGraphicsModule.GetPalette(2);
            enemyPalette.SetColor(0, 3);
            enemyPalette.SetColor(1, 7);
            enemyPalette.SetColor(2, 11);
            enemyPalette.SetColor(3, 12);

            var statusPalette = GameSystem.CoreGraphicsModule.GetPalette(3);
            statusPalette.SetColor(0, 15);
            statusPalette.SetColor(1, 12);
            statusPalette.SetColor(2, 11);
            statusPalette.SetColor(3, 15);

            _tileModule.Layer = 0;
            _spritesModule.Layer = 1;

            new DiskNBitPlaneBitmapLoader(GameSystem)
              .Load(new DiskFile(ContentFolder.PatternTables, "platformer_pt.bmp"),
                  _romPatternTable);

            new DiskNBitPlaneLoader()
             .Load(new DiskFile(ContentFolder.NameTables, "platformer.nt"),
                 _romNameTable);

            var graphicsModule = GameSystem.CoreGraphicsModule;
            _romPatternTable.CopyTo(graphicsModule.PatternTable, GameSystem.Memory);
            _romNameTable.CopyTo(_tileModule.NameTable, GameSystem.Memory);

            var playerSprite = _spritesModule.GetSprite(0);
            playerSprite.X = 16;
            playerSprite.Y = 0;
            playerSprite.Tile = 16;
            playerSprite.Orientation = Orientation.Vertical;
            playerSprite.Tile2Offset = 1;
            playerSprite.Palette = 1;

            _player.SpriteIndex = 0;
            _player.XSpeed = 0;
            _player.YSpeed = 0;

            var enemySprite = _spritesModule.GetSprite(1);
            enemySprite.X = 30;
            enemySprite.Y = 16;
            enemySprite.Tile = 18;
            enemySprite.Orientation = Orientation.Vertical;
            enemySprite.Tile2Offset = 1;
            enemySprite.Palette = 2;

            _enemy.SpriteIndex = 1;
            _enemy.XSpeed = 0; // -4;
            _enemy.YSpeed = 0;

            var bullet = _spritesModule.GetSprite(2);
            bullet.Tile = 0;
            bullet.Orientation = Orientation.Vertical;
            bullet.Tile2Offset = 0;
            bullet.Palette = 2;

            _bullet.SpriteIndex = 2;

            _statusBarModule.Score = 4123;
            _statusBarModule.Health = 5;
            _statusBarModule.MaxHealth = 10;

            _statusBarModule.OnStartup();

            byte index = 0;
            _audioModule.NoteSequence[index++] = Audio.AudioAction.OctaveUp;
            _audioModule.NoteSequence[index++] = Audio.AudioAction.OctaveUp;
            _audioModule.NoteSequence[index++] = Audio.AudioAction.PlayG;
            _audioModule.NoteSequence[index++] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[index++] = Audio.AudioAction.PlayASharp;

            _audioModule.NoteSequence[index++] = Audio.AudioAction.PlayG;
            _audioModule.NoteSequence[index++] = Audio.AudioAction.PlayFSharp;
            _audioModule.NoteSequence[index++] = Audio.AudioAction.PlayF;
            _audioModule.NoteSequence[index++] = Audio.AudioAction.PlayE;
            _audioModule.NoteSequence[index++] = Audio.AudioAction.PlayDSharp;
            _audioModule.NoteSequence[index++] = Audio.AudioAction.PlayD;
        }

        public void OnLogicUpdate()
        {
            _timer.Value++;

            _audioModule.OnLogicUpdate();
            _inputModule.OnLogicUpdate();
            
            switch(_gameState.Value)
            {
                case GameState.Normal:
                    UpdatePlayerMovement();
                    UpdateEnemy();
                    UpdateAnimation();
                    HandleScroll();
                    HandlePlayerHitTimer();
                    break;

                case GameState.LoseLife:

                    HandleLoseLife();
                    break;

                case GameState.ResetLevel:

                    _tileModule.Scroll.X = 0;
                    _spritesModule.Scroll.X = 0;

                    var bgPalette = GameSystem.CoreGraphicsModule.GetPalette(0);
                    bgPalette.SetColor(0, 0);
                    bgPalette.SetColor(1, 1);
                    bgPalette.SetColor(2, 2);
                    bgPalette.SetColor(3, 3);

                    var playerPalette = GameSystem.CoreGraphicsModule.GetPalette(1);
                    playerPalette.SetColor(0, 6);
                    playerPalette.SetColor(1, 6);
                    playerPalette.SetColor(2, 14);
                    playerPalette.SetColor(3, 13);

                    var enemyPalette = GameSystem.CoreGraphicsModule.GetPalette(2);
                    enemyPalette.SetColor(0, 3);
                    enemyPalette.SetColor(1, 7);
                    enemyPalette.SetColor(2, 11);
                    enemyPalette.SetColor(3, 12);

                    var statusPalette = GameSystem.CoreGraphicsModule.GetPalette(3);
                    statusPalette.SetColor(0, 15);
                    statusPalette.SetColor(1, 12);
                    statusPalette.SetColor(2, 11);
                    statusPalette.SetColor(3, 15);

                    _tileModule.Layer = 0;
                    _spritesModule.Layer = 1;

                    var playerSprite = _spritesModule.GetSprite(0);
                    playerSprite.X = 16;
                    playerSprite.Y = 0;
                    playerSprite.Tile = 16;
                    playerSprite.Orientation = Orientation.Vertical;
                    playerSprite.Tile2Offset = 1;
                    playerSprite.Palette = 1;

                    _player.SpriteIndex = 0;
                    _player.XSpeed = 0;
                    _player.YSpeed = 0;

                    var enemySprite = _spritesModule.GetSprite(1);
                    enemySprite.X = 30;
                    enemySprite.Y = 16;
                    enemySprite.Tile = 18;
                    enemySprite.Orientation = Orientation.Vertical;
                    enemySprite.Tile2Offset = 1;
                    enemySprite.Palette = 2;

                    _enemy.SpriteIndex = 1;
                    _enemy.XSpeed = 0; // -4;
                    _enemy.YSpeed = 0;

                    var bullet = _spritesModule.GetSprite(2);
                    bullet.Tile = 0;
                    bullet.Orientation = Orientation.Vertical;
                    bullet.Tile2Offset = 0;
                    bullet.Palette = 2;

                    _bullet.SpriteIndex = 2;

                    _statusBarModule.Score = 4123;
                    _statusBarModule.Health = 5;
                    _statusBarModule.MaxHealth = 10;

                    _gameState.Value = GameState.Normal;
                    break;
            }
        }

        private void HandleLoseLife()
        {
            if(_playerHitTimer.Value == 254)
            {
                _audioModule.GetChannel(0).Play(5, 32, 6);
                _bulletTimer.Value = 0;
            }

            if((_timer.Value % 4) == 0)
            {
                var bgPalette = GameSystem.CoreGraphicsModule.GetPalette(0);

                if (_bulletTimer.Value == 0)
                {
                    _bulletTimer.Value = 1;
                    bgPalette.SetColor(0, 0);
                    bgPalette.SetColor(1, 1);
                    bgPalette.SetColor(2, 2);
                    bgPalette.SetColor(3, 3);
                }
                else if (_bulletTimer.Value == 1)
                {
                    _bulletTimer.Value = 2;
                    bgPalette.SetColor(0, 1);
                    bgPalette.SetColor(1, 2);
                    bgPalette.SetColor(2, 3);
                    bgPalette.SetColor(3, 0);
                }
                else if (_bulletTimer.Value == 2)
                {
                    _bulletTimer.Value = 3;
                    bgPalette.SetColor(0, 2);
                    bgPalette.SetColor(1, 3);
                    bgPalette.SetColor(2, 0);
                    bgPalette.SetColor(3, 1);
                }
                else if (_bulletTimer.Value == 3)
                {
                    _bulletTimer.Value = 0;
                    bgPalette.SetColor(0, 3);
                    bgPalette.SetColor(1, 0);
                    bgPalette.SetColor(2, 1);
                    bgPalette.SetColor(3, 2);
                }
            }

            _playerHitTimer.Value--;
            if(_playerHitTimer.Value == 0)
            {
                _gameState.Value = GameState.ResetLevel;
            }
        }

        private void HandlePlayerHitTimer()
        {
            if (_playerHitTimer.Value == 0)
                return;

            _playerHitTimer.Value--;
            if ((_playerHitTimer.Value % 2) == 0)
            {
                _player.GetSprite().Palette = 1;
            }
            else
            {
                _player.GetSprite().Palette = 2;
            }
        }

        private void UpdatePlayerMovement()
        {
            if (_inputModule.Player1.LeftKey.IsDown() && _player.XSpeed > -32)
            {
                _player.XSpeed -= 4;
                _player.GetSprite().FlipX = true;
            }
            else if (_inputModule.Player1.RightKey.IsDown() && _player.XSpeed < 32)
            {
                _player.XSpeed += 4;
                _player.GetSprite().FlipX = false;
            }
            else if (_player.XSpeed > 0)
                _player.XSpeed -= 4;
            else if (_player.XSpeed < 0)
                _player.XSpeed += 4;

            if (_player.YSpeed < 90)
                _player.YSpeed += 5;

            if (_enemy.YSpeed < 90)
                _enemy.YSpeed += 5;


            _player.Update();
            _enemy.Update();
            _bullet.Update();

            CheckCollisions();
        }

        private void UpdateEnemy()
        {
            if (_enemy.X > _player.X)
            {
                _enemy.XSpeed = -4;
                _enemy.GetSprite().FlipX = false;
            }
            else
            {
                _enemy.XSpeed = 4;
                _enemy.GetSprite().FlipX = true;
            }

            if ((_timer.Value % 4) == 0)
                _bulletTimer.Value++;

            var bullet = _spritesModule.GetSprite(2);
            if (_bulletTimer.Value == 64 && bullet.Tile == 0)
            {
                bullet.Tile = 20;
                bullet.X = _enemy.X;
                bullet.Y = _enemy.Y;
                bullet.FlipX = _enemy.GetSprite().FlipX;
                _bulletTimer.Value = 0;

                if (bullet.FlipX)
                    _bullet.XSpeed = 32;
                else
                    _bullet.XSpeed = -32;

                _bullet.YSpeed = 0;
            }

            if (bullet.Tile != 0)
            {
                _bulletTimer.Value++;

                if (_bulletTimer.Value == 200)
                {
                    bullet.Tile = 0;
                    _bulletTimer.Value = 0;
                }
            }
        }

        private void UpdateAnimation()
        {
            if (_timer % 8 == 0)
            {
                if (_player.XSpeed != 0)
                {
                    var player = _player.GetSprite();
                    if (player.Tile2Offset == 1)
                        player.Tile2Offset = 2;
                    else
                        player.Tile2Offset = 1;
                }
            }

            if (_timer % 16 == 0)
            {
                var enemy = _spritesModule.GetSprite(1);
                enemy.Tile = enemy.Tile.Toggle(18, 19);
            }


            if (_player.XSpeed == 0)
            {
                _player.GetSprite().Tile2Offset = 1;
            }
        }

        private void HandleScroll()
        {
            var x = _player.X + (Specs.TileWidth / 2);
            var newScrollX = x - (Specs.ScreenWidth / 2);
            if (newScrollX < 0)
                newScrollX = 0;

            _tileModule.Scroll.X = (byte)newScrollX;
            _spritesModule.Scroll.X = _tileModule.Scroll.X;
        }

        private void CheckCollisions()
        {
            //var playerCollision = _collisionDetector.DetectCollisions(_player);
            //if (playerCollision.IsOnGround && _inputModule.Player1.AKey == GameKeyState.Pressed)
            //{
            //    _player.YSpeed = -128;
            //}

            //_collisionDetector.DetectCollisions(_enemy);

            //if (_playerHitTimer.Value == 0)
            //{
            //    if (_collisionDetector.CheckCollision(_player, _enemy)
            //        || _collisionDetector.CheckCollision(_player, _bullet))
            //    {
            //        _statusBarModule.Health--;

            //        if(_statusBarModule.Health > 0)
            //        {
            //            _audioModule.GetChannel(0).Play(0, 4, 5);
            //            _playerHitTimer.Value = 64;
            //        }
            //        else
            //        {
            //            _gameState.Value = GameState.LoseLife;
            //            _playerHitTimer.Value = 255;
            //        }
            //    }
            //}
        }

        public void OnVBlank()
        {
            _tileModule.OnVBlank();
            _spritesModule.OnVBlank();
        }

        public void OnHBlank()
        {
            if (GameSystem.CoreGraphicsModule.ScreenPoint.Y < 6)
            {
                _statusBarModule.OnHBlank();
            }
            else
            {
                _tileModule.OnHBlank();
                _spritesModule.OnHBlank();
            }
        }

        //todo, probably can optimize this
        public byte GetPalette(int pixel)
        {
            int screenX = pixel % GameSystem.Specs.ScreenWidth;
            int screenY = pixel / GameSystem.Specs.ScreenHeight;

            if (screenY < 6)
                return 3;

            int scrollX = screenX + _tileModule.Scroll.X;
            int scrollY = screenY + _tileModule.Scroll.Y;

            for (int i = 0; _spritesModule.ScanlineSprites[i] != 255; i++)
            {
                var sprite = _spritesModule.GetScanlineSprite(i);

                if (scrollY < sprite.Y || scrollY >= sprite.Bottom)
                    continue;

                if (scrollX >= sprite.X && scrollX < sprite.Right)
                {
                    if (GameSystem.CoreGraphicsModule.ScanlineDrawBuffer[screenX] == 0)
                        return _tileModule.BackgroundPaletteIndex;
                    else 
                        return sprite.Palette;
                }
            }

            return _tileModule.BackgroundPaletteIndex;
        }
    }
}
