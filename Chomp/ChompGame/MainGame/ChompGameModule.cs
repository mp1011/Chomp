﻿using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteModels;
using ChompGame.ROM;

namespace ChompGame.MainGame
{
    public enum AddressLabels
    {
        NameTables,
        SceneDefinitions,
        SpriteDefinitions,
        FreeRAM
    }

    class ChompGameModule : Module, IMasterModule
    {
        enum GameState : byte
        {
            LoadScene,
            PlayScene,
            Test
        }

       

        private readonly CollisionDetector _collisionDetector;
        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private readonly ChompAudioService _audioService;
        private readonly TileModule _tileModule;
        private readonly StatusBarModule _statusBarModule;
        private readonly MusicModule _musicModule;
        private readonly StatusBar _statusBar;
        private NBitPlane _masterPatternTable;
        private LevelBuilder _levelBuilder;
       
        private GameByteEnum<GameState> _gameState;
        private GameByte _timer;
        private GameByte _currentLevel;
        private PlayerController _playerController;

        private SpriteControllerPool<LizardEnemyController> _lizardEnemyControllers;
        private SpriteControllerPool<BulletController> _lizardBulletControllers;
        private SpriteControllerPool<BirdEnemyController> _birdEnemyControllers;
        private SpriteControllerPool<BombController> _bombControllers;

        private WorldScroller _worldScroller;
        private RasterInterrupts _rasterInterrupts;

        public ChompGameModule(MainSystem mainSystem, InputModule inputModule, BankAudioModule audioModule,
           SpritesModule spritesModule, TileModule tileModule, StatusBarModule statusBarModule, MusicModule musicModule)
           : base(mainSystem)
        {
            _audioService = new ChompAudioService(audioModule);
            _inputModule = inputModule;
            _spritesModule = spritesModule;
            _tileModule = tileModule;
            _statusBarModule = statusBarModule;
            _musicModule = musicModule;
            _collisionDetector = new CollisionDetector(Specs);
            _statusBar = new StatusBar(_tileModule);
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());
            _timer = memoryBuilder.AddByte();
            _currentLevel = memoryBuilder.AddByte();

            _worldScroller = new WorldScroller(memoryBuilder, Specs, _tileModule, _spritesModule);

            _rasterInterrupts = new RasterInterrupts(Specs,
                GameSystem.CoreGraphicsModule,
                _worldScroller,
                _tileModule,
                _timer,
                _currentLevel);

            _rasterInterrupts.BuildMemory(memoryBuilder);

           
            memoryBuilder.AddLabel(AddressLabels.FreeRAM);
            memoryBuilder.AddBytes(1024);

            memoryBuilder.AddLabel(AddressLabels.SpriteDefinitions);

            AddSpriteDefinitions(memoryBuilder);

            _statusBar.BuildMemory(memoryBuilder);

            AddSpriteControllers(memoryBuilder);

            _masterPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, 64, 64);

            memoryBuilder.AddLabel(AddressLabels.NameTables);
            memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);

            memoryBuilder.AddLabel(AddressLabels.SceneDefinitions);

            SceneBuilder.SetupTestScene(memoryBuilder, Specs);
        }

        private void AddSpriteDefinitions(SystemMemoryBuilder memoryBuilder)
        {
            //player
            new SpriteDefinition(memoryBuilder,
                tile: 1,
                secondTileOffset: 1,
                sizeX: 1,
                sizeY: 2,
                gravityStrength: GravityStrength.Medium,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.AnimateLowerTileOnly,
                collidesWithBackground: true,
                flipXWhenMovingLeft: true);

            //lizard
            new SpriteDefinition(memoryBuilder,
                tile: 3,
                secondTileOffset: 1,
                sizeX: 1,
                sizeY: 2,
                gravityStrength: GravityStrength.High,
                movementSpeed: MovementSpeed.Slow,
                animationStyle: AnimationStyle.AnimateWhenMoving,
                collidesWithBackground: true,
                flipXWhenMovingLeft: true);

            //lizard fireball
            new SpriteDefinition(memoryBuilder,
                tile: 5,
                secondTileOffset: 0,
                sizeX: 1,
                sizeY: 1,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.NoAnimation,
                collidesWithBackground: false,
                flipXWhenMovingLeft: true);

            //bird
            new SpriteDefinition(memoryBuilder,
                tile: 17,
                secondTileOffset: 0,
                sizeX: 2,
                sizeY: 1,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.AlwaysAnimate,
                collidesWithBackground: false,
                flipXWhenMovingLeft: true);

            //bomb
            new SpriteDefinition(memoryBuilder,
               tile: 2,
               secondTileOffset: 0,
               sizeX: 1,
               sizeY: 1,
               gravityStrength: GravityStrength.Medium,
               movementSpeed: MovementSpeed.Fast,
               animationStyle: AnimationStyle.NoAnimation,
               collidesWithBackground: true,
               flipXWhenMovingLeft: false);
        }

        private void AddSpriteControllers(SystemMemoryBuilder memoryBuilder)
        {
            _playerController = new PlayerController(_spritesModule, _worldScroller, _inputModule, _statusBar, _audioService, _collisionDetector, _timer, memoryBuilder);

            _lizardBulletControllers = new SpriteControllerPool<BulletController>(
               2,
               _spritesModule,
               () => new BulletController(_spritesModule, _worldScroller, _timer, memoryBuilder, SpriteType.LizardBullet));

            _lizardEnemyControllers = new SpriteControllerPool<LizardEnemyController>(
                2,
                _spritesModule,
                () => new LizardEnemyController(_lizardBulletControllers, _spritesModule, _worldScroller, _audioService, _collisionDetector, _timer, memoryBuilder));

            _birdEnemyControllers = new SpriteControllerPool<BirdEnemyController>(
               1,
               _spritesModule,
               () => new BirdEnemyController(_playerController.WorldSprite, _spritesModule, _worldScroller, _audioService, _timer, memoryBuilder));

            _bombControllers = new SpriteControllerPool<BombController>(
                size: 2,
                _spritesModule,
                () => new BombController(_spritesModule, _worldScroller, _playerController, _collisionDetector, memoryBuilder, _timer));
        }

        public override void OnStartup()
        {
            PatternTableCreator.SetupMasterPatternTable(
                _masterPatternTable,
                GameSystem.GraphicsDevice,
                Specs);

            new DiskNBitPlaneLoader()
                .Load(new DiskFile(ContentFolder.PatternTables, "master.pt"),
                    _masterPatternTable);

            _gameState.Value = GameState.LoadScene;
            _audioService.OnStartup();
           // _musicModule.CurrentSong = MusicModule.SongName.SeaDreams;
        }
    
        public void OnLogicUpdate()
        {
            _musicModule.Update();
            _audioService.Update();
            _timer.Value++;
          
            switch (_gameState.Value)
            {
                case GameState.LoadScene:
                    LoadScene();
                    break;
                case GameState.PlayScene:
                    PlayScene();
                    break;
                case GameState.Test:
                    break;
            }
        }

        public void LoadScene()
        {
            //todo, ability to load scene by key 
            SceneDefinition testScene = new SceneDefinition(
               GameSystem.Memory.GetAddress(AddressLabels.SceneDefinitions), GameSystem.Memory, Specs);

            _levelBuilder = new LevelBuilder(testScene, _tileModule, Specs);

            _levelBuilder.SetupVRAMPatternTable(
                _masterPatternTable,
                GameSystem.CoreGraphicsModule.PatternTable,
                GameSystem.Memory);

            PatternTableExporter.ExportPatternTable(
                GameSystem.GraphicsDevice, 
                GameSystem.CoreGraphicsModule.PatternTable);

            //todo, define level palettes elsewhere

            var bgPalette = GameSystem.CoreGraphicsModule.GetBackgroundPalette(0);
            bgPalette.SetColor(0, ChompGameSpecs.LightBlue);
            bgPalette.SetColor(1, ChompGameSpecs.Gray1);
            bgPalette.SetColor(2, ChompGameSpecs.Gray2);
            bgPalette.SetColor(3, ChompGameSpecs.Gray3);

            var bgPalette2 = GameSystem.CoreGraphicsModule.GetBackgroundPalette(1);
            bgPalette2.SetColor(0, ChompGameSpecs.BlueGray2);
            bgPalette2.SetColor(1, ChompGameSpecs.DarkBrown);
            bgPalette2.SetColor(2, ChompGameSpecs.DarkBrown);
            bgPalette2.SetColor(3, ChompGameSpecs.Gray3);

            var bombPalette = GameSystem.CoreGraphicsModule.GetSpritePalette(0);
            bombPalette.SetColor(1, ChompGameSpecs.Black); 
            bombPalette.SetColor(2, ChompGameSpecs.Gray1);
            bombPalette.SetColor(3, ChompGameSpecs.Gray2); 


            var playerPalette = GameSystem.CoreGraphicsModule.GetSpritePalette(1);
            playerPalette.SetColor(1, ChompGameSpecs.Orange); //hair
            playerPalette.SetColor(2, ChompGameSpecs.LightTan); //face
            playerPalette.SetColor(3, ChompGameSpecs.DarkBrown); //legs

            var enemyPallete = GameSystem.CoreGraphicsModule.GetSpritePalette(2);
            enemyPallete.SetColor(1, ChompGameSpecs.Green1); 
            enemyPallete.SetColor(2, ChompGameSpecs.Green2); 
            enemyPallete.SetColor(3, ChompGameSpecs.Red3); 

            var bulletPallete = GameSystem.CoreGraphicsModule.GetSpritePalette(3);
            bulletPallete.SetColor(0, ChompGameSpecs.Black);
            bulletPallete.SetColor(1, ChompGameSpecs.Red2);
            bulletPallete.SetColor(2, ChompGameSpecs.Red3);
            bulletPallete.SetColor(3, ChompGameSpecs.LightYellow);

            var playerSpriteDefinition = new SpriteDefinition(SpriteType.Player, GameSystem.Memory);
            _playerController.WorldSprite.X = 16;
            _playerController.WorldSprite.Y = 16;
            _playerController.Palette = 1;
            _playerController.ConfigureSprite(_playerController.GetSprite());


            //var lizard1 = _lizardEnemyControllers
            //    .TryAddNew();

            //lizard1.WorldSprite.X = 32;
            //lizard1.WorldSprite.Y = 40;
            //lizard1.GetSprite().Palette = 2;

            //var lizard2 = _lizardEnemyControllers
            //    .TryAddNew()
            //    .GetSprite();
            //lizard2.X = 64;
            //lizard2.Y = 40;
            //lizard2.Palette = 1;

            //var bird = _birdEnemyControllers
            //    .TryAddNew();
            //bird.WorldSprite.X = 32;
            //bird.WorldSprite.Y = 32;
            //bird.GetSprite().Palette = 2;

            var bomb = _bombControllers
                .TryAddNew();

            bomb.WorldSprite.X = 32;
            bomb.WorldSprite.Y = 16;
            bomb.Palette = 0;

            var bomb2 = _bombControllers
              .TryAddNew();

            bomb2.WorldSprite.X = 50;
            bomb2.WorldSprite.Y = 16;
            bomb2.Palette = 0;

            _lizardBulletControllers.Execute(b => b.Palette = 3, 
                skipIfInactive:false);

            _gameState.Value = GameState.PlayScene;

            _playerController.Motion.XSpeed = 0;
            _playerController.Motion.YSpeed = 0;

            _lizardEnemyControllers.Execute(c =>
            {
                c.Motion.XSpeed = 0;
                c.Motion.YSpeed = 0;
            });

            GameDebug.Watch1 = new DebugWatch(
                "Player X",
                () =>_playerController.WorldSprite.X);

            GameDebug.Watch2 = new DebugWatch(
                name: "Player Sprite X",
                () => _playerController.GetSprite().X);

            GameDebug.Watch3 = new DebugWatch(
               name: "Player Sprite Y",
               () => _playerController.GetSprite().Y);


            _statusBar.AddToScore(123456789);
            _statusBar.SetLives(3);
            _statusBar.Health = 8;

            _levelBuilder.BuildBackgroundNametable();

            //todo, use level number as seed
            var levelMap =_levelBuilder.BuildNameTable(GameSystem.Memory, 1);
            var levelAttributeTable = _levelBuilder.BuildAttributeTable(GameSystem.Memory, levelMap.Bytes);

            _worldScroller.Initialize(testScene, _playerController.WorldSprite, levelMap, levelAttributeTable);
            _worldScroller.UpdateVram();

            _collisionDetector.Initialize(testScene, levelMap);
            _rasterInterrupts.SetScene(testScene);
        }

        public void PlayScene()
        {
            _playerController.Update();
            _lizardEnemyControllers.Execute(c => c.Update());
            _lizardBulletControllers.Execute(c => c.Update());
            _birdEnemyControllers.Execute(c => c.Update());
            _bombControllers.Execute(c => c.Update());

            if(_worldScroller.Update())
            {
                _playerController.WorldSprite.UpdateSpritePosition();
                _lizardEnemyControllers.Execute(c => c.WorldSprite.UpdateSpritePosition());
                _lizardBulletControllers.Execute(c => c.WorldSprite.UpdateSpritePosition());
                _birdEnemyControllers.Execute(c => c.WorldSprite.UpdateSpritePosition());
                _bombControllers.Execute(c => c.WorldSprite.UpdateSpritePosition());
            }

            _rasterInterrupts.Update();

            _playerController.CheckEnemyOrBulletCollisions(_lizardBulletControllers);
            _playerController.CheckEnemyOrBulletCollisions(_lizardEnemyControllers);
            _playerController.CheckEnemyOrBulletCollisions(_birdEnemyControllers);
            _playerController.CheckBombPickup(_bombControllers);

            _bombControllers.Execute(b =>
            {
                b.CheckEnemyCollisions(_lizardEnemyControllers);
                b.CheckEnemyCollisions(_birdEnemyControllers);
            });

            if (_timer.Value % 8 == 0)
            {
                var bulletPallete = GameSystem.CoreGraphicsModule.GetSpritePalette(3);
                if (bulletPallete.GetColorIndex(1) == ChompGameSpecs.Red2)
                {
                    bulletPallete.SetColor(1, ChompGameSpecs.Red3);
                    bulletPallete.SetColor(2, ChompGameSpecs.Orange);
                    bulletPallete.SetColor(3, ChompGameSpecs.White);
                }
                else
                {
                    bulletPallete.SetColor(1, ChompGameSpecs.Red2);
                    bulletPallete.SetColor(2, ChompGameSpecs.Red3);
                    bulletPallete.SetColor(3, ChompGameSpecs.LightYellow);
                }
            }
        }

        public void OnVBlank()
        {
            _tileModule.OnVBlank();
            _spritesModule.OnVBlank();
        }

        public void OnHBlank()
        {
            _statusBar.OnHBlank();
            _rasterInterrupts.OnHBlank();
            _tileModule.OnHBlank();
            _spritesModule.OnHBlank();
        }
    }
}
