using ChompGame.Data;
using ChompGame.Helpers;
using ChompGame.ROM;
using System;

namespace ChompGame.GameSystem
{
    class PlatformerModule : Module, IMasterModule
    {
        public enum PaletteIndex
        {
            Background=0,
            Sprites,
            Info
        }

        private CollisionDetector _collisionDetector;
        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private readonly AudioModule _audioModule;
        private readonly TileModule _tileModule;

        private NBitPlane _romPatternTable;
        private NBitPlane _romNameTable;

        private MovingSprite _player;

        public PlatformerModule(MainSystem mainSystem, InputModule inputModule, AudioModule audioModule, 
            SpritesModule spritesModule, TileModule tileModule) 
            : base(mainSystem)
        {
            _audioModule = audioModule;
            _inputModule = inputModule;
            _spritesModule = spritesModule;
            _tileModule = tileModule;
            _collisionDetector = new CollisionDetector(tileModule, Specs);
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _player = new MovingSprite(_spritesModule, memoryBuilder);

            memoryBuilder.BeginROM();
            _romPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            _romNameTable = memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);

        }

        public override void OnStartup()
        {
            var bgPalette = GameSystem.CoreGraphicsModule.SetCurrentPalette(0);
            bgPalette.SetColor(0, 0);
            bgPalette.SetColor(1, 1);
            bgPalette.SetColor(2, 2);
            bgPalette.SetColor(3, 3);

            var spritePalette = GameSystem.CoreGraphicsModule.SetCurrentPalette(1);
            spritePalette.SetColor(0, 6);
            spritePalette.SetColor(1, 6);
            spritePalette.SetColor(2, 14);
            spritePalette.SetColor(3, 13);

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
            playerSprite.Y = 16;
            playerSprite.Tile = 16;
            playerSprite.Orientation = Orientation.Vertical;
            playerSprite.Tile2Offset = 1;
            playerSprite.Palette = 1;

            _player.SpriteIndex = 0;
            _player.XSpeed = 0;
            _player.YSpeed = 0;
        }


        public void OnLogicUpdate()
        {
            _audioModule.OnLogicUpdate();
            _inputModule.OnLogicUpdate();

            UpdatePlayerMovement();
        }

        private void UpdatePlayerMovement()
        {
            if (_inputModule.Player1.LeftKey.IsDown() && _player.XSpeed > -32)
                _player.XSpeed -= 16;
            else if (_inputModule.Player1.RightKey.IsDown() && _player.XSpeed < 32)
                _player.XSpeed += 16;
            else if (_player.XSpeed > 0)
                _player.XSpeed -= 8;
            else if (_player.XSpeed < 0)
                _player.XSpeed += 8;

            if (_player.YSpeed < 90)
                _player.YSpeed += 8;

            _player.Update();

            var collisionInfo = CheckPlayerBGCollision();
            if(collisionInfo.IsOnGround && _inputModule.Player1.AKey == GameKeyState.Pressed)
            {
                _player.YSpeed = -128;
            }
        }

        private CollisionInfo CheckPlayerBGCollision()
        {
            return _collisionDetector.DetectCollisions(_player);
            //var playerSprite = _spritesModule.GetSprite(_player.SpriteIndex);
            //var topLeftX = playerSprite.X / Specs.TileWidth;
            //var topLeftY = playerSprite.Y / Specs.TileHeight;
            
            //var bottomRightX = (playerSprite.Right / Specs.TileWidth)+1;
            //var bottomRightY = (playerSprite.Bottom / Specs.TileHeight)+1;

            //for(int y = topLeftY; y < bottomRightY; y++)
            //{
            //    for (int x = topLeftX; x < bottomRightX; x++)
            //    {
            //        var tile = _tileModule.NameTable[x, y];
            //        if (tile == 0)
            //            continue;

            //        var tileTop = y * Specs.TileHeight;
            //        if (playerSprite.Bottom > tileTop)
            //        {
            //            playerSprite.Y = (byte)(tileTop - (Specs.TileHeight * 2));
            //            _player.YSpeed = 0;
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
            _tileModule.OnHBlank();
            _spritesModule.OnHBlank();
        }
    }
}
