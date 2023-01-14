using ChompGame.Data;
using ChompGame.Data.Memory;
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
        SceneParts,
        FreeRAM,
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
        private GameByteEnum<Level> _currentLevel;

        private GameByte _timer;
        private SceneSpriteControllers _sceneSpriteControllers;
        private WorldScroller _worldScroller;
        private RasterInterrupts _rasterInterrupts;

        public GameRAM GameRAM { get; } 

        public TileModule TileModule => _tileModule;
        public SpritesModule SpritesModule => _spritesModule;
        public GameByte LevelTimer => _timer;
        public WorldScroller WorldScroller => _worldScroller;
        public CollisionDetector CollissionDetector => _collisionDetector;
        public StatusBar StatusBar => _statusBar;
        public ChompAudioService AudioService => _audioService;
        public InputModule InputModule => _inputModule;

        public Level CurrentLevel
        {
            get => _currentLevel.Value;
            set => _currentLevel.Value = value;
        }

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

            GameRAM = new GameRAM(Specs);
            _statusBar = new StatusBar(_tileModule, GameRAM);
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());
            _timer = memoryBuilder.AddByte();
            _currentLevel = new GameByteEnum<Level>(memoryBuilder.AddByte());

            _worldScroller = new WorldScroller(memoryBuilder, Specs, _tileModule, _spritesModule);

            _rasterInterrupts = new RasterInterrupts(Specs,
                GameSystem.CoreGraphicsModule,
                _worldScroller,
                _tileModule,
                _statusBar,
                _timer,
                _currentLevel);

            _rasterInterrupts.BuildMemory(memoryBuilder);

            //note, have unused bits here
            var freeRamOffset = new ExtendedByte2(
                memoryBuilder.AddByte(),
                new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0));

            GameRAM.Initialize(freeRamOffset, memoryBuilder.Memory);

            memoryBuilder.AddByte();

            memoryBuilder.AddLabel(AddressLabels.FreeRAM);
            memoryBuilder.AddBytes(Specs.GameRAMSize);

            memoryBuilder.AddLabel(AddressLabels.SpriteDefinitions);

            AddSpriteDefinitions(memoryBuilder);

            _statusBar.BuildMemory(memoryBuilder);
            _masterPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, 64, 64);

            memoryBuilder.AddLabel(AddressLabels.NameTables);
            memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);

            memoryBuilder.AddLabel(AddressLabels.SceneDefinitions);

            SceneBuilder.AddSceneHeaders(memoryBuilder, Specs);

            memoryBuilder.AddLabel(AddressLabels.SceneParts);
            SceneBuilder.AddSceneParts(memoryBuilder, Specs);
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

            _currentLevel.Value = Level.TestSceneNoScrollFlat;
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
            SceneDefinition scene = new SceneDefinition(_currentLevel.Value, GameSystem.Memory, Specs);
            _levelBuilder = new LevelBuilder(this, scene);

            SystemMemoryBuilder memoryBuilder = new SystemMemoryBuilder(GameSystem.Memory, 
                Specs,
                GameRAM);

            _sceneSpriteControllers = _levelBuilder.CreateSpriteControllers(memoryBuilder);

            _levelBuilder.SetupVRAMPatternTable(
                _masterPatternTable,
                GameSystem.CoreGraphicsModule.PatternTable,
                GameSystem.Memory);

            PatternTableExporter.ExportPatternTable(
                GameSystem.GraphicsDevice, 
                GameSystem.CoreGraphicsModule.PatternTable);

            //todo, define level palettes elsewhere

            var bgPalette = GameSystem.CoreGraphicsModule.GetBackgroundPalette(0);
            bgPalette.SetColor(0, ChompGameSpecs.Gray3);
            bgPalette.SetColor(1, ChompGameSpecs.Green1);
            bgPalette.SetColor(2, ChompGameSpecs.Green2);
            bgPalette.SetColor(3, ChompGameSpecs.Green3);

            var bgPalette2 = GameSystem.CoreGraphicsModule.GetBackgroundPalette(1);
            bgPalette2.SetColor(0, ChompGameSpecs.Orange);
            bgPalette2.SetColor(1, ChompGameSpecs.Gray1);
            bgPalette2.SetColor(2, ChompGameSpecs.Gray2);
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

            _gameState.Value = GameState.PlayScene;
            _statusBar.AddToScore(123456789);
            _statusBar.SetLives(3);
            _statusBar.Health = 8;

           
            var levelMap =_levelBuilder.BuildNameTable(memoryBuilder, (int)_currentLevel.Value);
            var levelAttributeTable = _levelBuilder.BuildAttributeTable(memoryBuilder, levelMap.Bytes);
            
            _levelBuilder.ApplyLevelAlterations(levelMap);

            _levelBuilder.SetProperTiles(levelMap);

            _levelBuilder.BuildBackgroundNametable(levelMap);

            _sceneSpriteControllers.Initialize(scene, levelMap, levelAttributeTable);
           
            _worldScroller.UpdateVram();

            _collisionDetector.Initialize(scene, levelMap);
            _rasterInterrupts.SetScene(scene);
        }

        public void PlayScene()
        {
            _sceneSpriteControllers.Update();


            if(_worldScroller.Update())
            {
                _sceneSpriteControllers.OnWorldScrollerUpdate();
            }

            _rasterInterrupts.Update();

            _sceneSpriteControllers.CheckCollissions();

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
            _rasterInterrupts.OnHBlank();
            _tileModule.OnHBlank();
            _spritesModule.OnHBlank();
        }
    }
}
