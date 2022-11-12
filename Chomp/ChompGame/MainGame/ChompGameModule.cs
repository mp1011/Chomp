using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteModels;
using ChompGame.ROM;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame
{
    public enum AddressLabels
    {
        NameTables,
        SceneDefinitions,
        SpriteDefinitions
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
       
        private GameByteEnum<GameState> _gameState;
        private GameByte _timer;
        private GameByte _currentLevel;


        private PlayerController _playerController;
        private SpriteControllerPool<LizardEnemyController> _lizardEnemyControllers;
        private SpriteControllerPool<BulletController> _lizardBulletControllers;

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
            _collisionDetector = new CollisionDetector(tileModule, Specs);
            _statusBar = new StatusBar(_tileModule);
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());
            _timer = memoryBuilder.AddByte();
            _currentLevel = memoryBuilder.AddByte();

            _rasterInterrupts = new RasterInterrupts(Specs,
                GameSystem.CoreGraphicsModule,
                _tileModule,
                _timer,
                _currentLevel);

            _rasterInterrupts.BuildMemory(memoryBuilder);

            memoryBuilder.AddLabel(AddressLabels.SpriteDefinitions);

            new SpriteDefinition(memoryBuilder,
                spriteType: SpriteType.Player,
                tile: 1,
                secondTileOffset: 1,
                palette: 1,
                orientation: Orientation.Vertical,
                gravityStrength: GravityStrength.Medium,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.AnimateLowerTileOnly,
                collidesWithBackground: true,
                flipXWhenMovingLeft: true);

            new SpriteDefinition(memoryBuilder,
                spriteType: SpriteType.Lizard,
                tile: 3,
                secondTileOffset: 1,
                palette: 2,
                orientation: Orientation.Vertical,
                gravityStrength: GravityStrength.High,
                movementSpeed: MovementSpeed.Slow,
                animationStyle: AnimationStyle.AnimateWhenMoving,
                collidesWithBackground: true,
                flipXWhenMovingLeft: true);

            new SpriteDefinition(memoryBuilder,
                spriteType: SpriteType.LizardBullet,
                tile: 5,
                secondTileOffset: 0,
                palette: 3,
                orientation: Orientation.Vertical,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.NoAnimation,
                collidesWithBackground: false,
                flipXWhenMovingLeft: true);

            _statusBar.BuildMemory(memoryBuilder);
            _playerController = new PlayerController(_spritesModule, _inputModule, _statusBar, _audioService, _collisionDetector, _timer, memoryBuilder);
            
            _lizardBulletControllers = new SpriteControllerPool<BulletController>(
               2,
               _spritesModule,
               () => new BulletController(_spritesModule, _timer, memoryBuilder, SpriteType.LizardBullet),
               s =>
               {
                   s.Tile = 5;
                   s.Tile2Offset = 0;
                   s.Palette = 3;
               });

            _lizardEnemyControllers = new SpriteControllerPool<LizardEnemyController>(
                2,
                _spritesModule,
                () => new LizardEnemyController(_lizardBulletControllers, _spritesModule, _collisionDetector, _timer, memoryBuilder),
                s => 
                {
                    s.Tile = 3;
                    s.Orientation = Orientation.Vertical;
                    s.Tile2Offset = 1;
                    s.Palette = 2;
                });

            _worldScroller = new WorldScroller(Specs, _tileModule, _spritesModule, _playerController.WorldSprite);

            _masterPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, 64, 64);

            memoryBuilder.AddLabel(AddressLabels.NameTables);
            memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);

            memoryBuilder.AddLabel(AddressLabels.SceneDefinitions);
            new SceneInfo(0, 7, memoryBuilder); //todo                      
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

            new DiskNBitPlaneLoader()
                .Load(new DiskFile(ContentFolder.NameTables, "testScene.nt"),
                    _tileModule.NameTable);

            var testScene = SetupTestScene();
            InitializePatternTable(testScene);

            _gameState.Value = GameState.LoadScene;
            _audioService.OnStartup();
            //_musicModule.CurrentSong = MusicModule.SongName.SeaDreams;
        }

        private void InitializePatternTable(SceneInfo testScene)
        {
            var patternTable = GameSystem.CoreGraphicsModule.PatternTable;

            for(int regionIndex = 0; regionIndex < testScene.RegionCount; regionIndex++)
            {
                var region = testScene.GetRegion(regionIndex);
                _masterPatternTable.CopyTilesTo(
                    patternTable, 
                    region.TileRegion, 
                    region.TileDestination, 
                    Specs,
                    GameSystem.Memory);
            }

            PatternTableExporter.ExportPatternTable(GameSystem.GraphicsDevice, patternTable);
        }

        private SceneInfo SetupTestScene()
        {
            SceneInfo testScene = new SceneInfo(GameSystem.Memory.GetAddress(AddressLabels.SceneDefinitions), GameSystem.Memory);

            //sprites
            testScene.DefineRegion(
                index: 0,
                region: new InMemoryByteRectangle(0, 0, 8, 2),
                destination: new Point(0, 2),
                systemMemory: GameSystem.Memory);

            //sky
            testScene.DefineRegion(
               index: 1,
               region: new InMemoryByteRectangle(0, 6, 8, 1),
               destination: new Point(0, 0),
               systemMemory: GameSystem.Memory);

            //bg1
            testScene.DefineRegion(
                index: 2,
                region: new InMemoryByteRectangle(0, 7, 6, 1),
                destination: new Point(0, 1),
                systemMemory: GameSystem.Memory);

            //bg2
            testScene.DefineRegion(
                index: 3,
                region: new InMemoryByteRectangle(5, 7, 3, 1),
                destination: new Point(5, 1),
                systemMemory: GameSystem.Memory);

            //text
            testScene.DefineRegion(
               index: 4,
               region: new InMemoryByteRectangle(4, 3, 8, 2),
               destination: new Point(0, 4),
               systemMemory: GameSystem.Memory);

            //text2
            testScene.DefineRegion(
               index: 5,
               region: new InMemoryByteRectangle(12, 4, 2, 1),
               destination: new Point(6, 6),
               systemMemory: GameSystem.Memory);

            //health guage
            testScene.DefineRegion(
             index: 6,
             region: new InMemoryByteRectangle(0, 4, 4, 1),
             destination: new Point(0, 6),
             systemMemory: GameSystem.Memory);

            _tileModule.BackgroundPaletteIndex.Value = 0;

            return testScene;
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
            var playerPalette = GameSystem.CoreGraphicsModule.GetPalette(1);
            playerPalette.SetColor(0, ChompGameSpecs.Black);
            playerPalette.SetColor(1, ChompGameSpecs.Orange); //hair
            playerPalette.SetColor(2, ChompGameSpecs.LightTan); //face
            playerPalette.SetColor(3, ChompGameSpecs.DarkBrown); //legs

            var enemyPallete = GameSystem.CoreGraphicsModule.GetPalette(2);
            enemyPallete.SetColor(0, ChompGameSpecs.Black);
            enemyPallete.SetColor(1, ChompGameSpecs.Green1); 
            enemyPallete.SetColor(2, ChompGameSpecs.Green2); 
            enemyPallete.SetColor(3, ChompGameSpecs.Red3); 

            var bulletPallete = GameSystem.CoreGraphicsModule.GetPalette(3);
            bulletPallete.SetColor(0, ChompGameSpecs.Black);
            bulletPallete.SetColor(1, ChompGameSpecs.Red2);
            bulletPallete.SetColor(2, ChompGameSpecs.Red3);
            bulletPallete.SetColor(3, ChompGameSpecs.LightYellow);

            var playerSprite = _spritesModule.GetSprite(0);
            playerSprite.X = 16;
            playerSprite.Y = 16;
            playerSprite.Tile = 1;
            playerSprite.Orientation = Orientation.Vertical;
            playerSprite.Tile2Offset = 1;
            playerSprite.Palette = 1;

            var lizard1 = _lizardEnemyControllers
                .TryAddNew()
                .GetSprite();
            lizard1.X = 32;
            lizard1.Y = 40;

            var lizard2 = _lizardEnemyControllers
                .TryAddNew()
                .GetSprite();

            lizard2.X = 64;
            lizard2.Y = 40;

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
                () => playerSprite.X);

            GameDebug.Watch3 = new DebugWatch(
               name: "Player Sprite Y",
               () => playerSprite.Y);


            _statusBar.AddToScore(123456789);
            _statusBar.SetLives(3);
            _statusBar.Health = 8;
        }

        public void PlayScene()
        {
            _playerController.Update();
            _lizardEnemyControllers.Execute(c => c.Update());
            _lizardBulletControllers.Execute(c => c.Update());
            _worldScroller.Update();
            _rasterInterrupts.Update();

            _playerController.CheckEnemyOrBulletCollisions(_lizardBulletControllers);
            _playerController.CheckEnemyOrBulletCollisions(_lizardEnemyControllers);
            
            if (_timer.Value % 8 == 0)
            {
                var bulletPallete = GameSystem.CoreGraphicsModule.GetPalette(3);
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
