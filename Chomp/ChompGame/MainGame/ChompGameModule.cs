using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteModels;
using ChompGame.ROM;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame
{
    public enum AddressLabels
    {
        MainTimer,
        NameTables,
        SceneDefinitions,
        SpriteDefinitions,
        SceneParts,
        FreeRAM,
        Themes
    }

    class ChompGameModule : Module, IMasterModule
    {
        enum GameState : byte
        {
            NewGame,
            RestartScene,
            LoadScene,
            PlayScene,
            Test,
            GameOver
        }

       

        private readonly CollisionDetector _collisionDetector;
        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private readonly ChompAudioService _audioService;
        private readonly TileModule _tileModule;
        private readonly MusicModule _musicModule;
        private readonly StatusBar _statusBar;
        private readonly DynamicBlockController _dynamicBlockController;
        
        private NBitPlane _masterPatternTable;
        private LevelBuilder _levelBuilder;
       
        private GameByteEnum<GameState> _gameState;
        private GameByteEnum<Level> _currentLevel;
        private NibbleEnum<ExitType> _lastExitType;
        private GameBit _carryingBomb;

        private GameByte _timer;
        private GameByte _deathTimer;
        private SceneSpriteControllers _sceneSpriteControllers;
        private WorldScroller _worldScroller;
        private RasterInterrupts _rasterInterrupts;
        private SceneDefinition _currentScene;
        private ScenePartsDestroyed _scenePartsDestroyed;

        private GameByteGridPoint _levelBossPosition;

        public GameByteGridPoint LevelBossPosition => _levelBossPosition;

        public ScenePartsDestroyed ScenePartsDestroyed => _scenePartsDestroyed;

        public GameRAM GameRAM => GameSystem.GameRAM;

        public TileModule TileModule => _tileModule;
        public SpritesModule SpritesModule => _spritesModule;
        public GameByte LevelTimer => _timer;
        public WorldScroller WorldScroller => _worldScroller;
        public CollisionDetector CollissionDetector => _collisionDetector;
        public StatusBar StatusBar => _statusBar;
        public ChompAudioService AudioService => _audioService;
        public InputModule InputModule => _inputModule;

        public PaletteModule PaletteModule { get; }

        public ExitsModule ExitsModule { get; }

        public RewardsModule RewardsModule { get; }

        public MusicModule MusicModule => _musicModule;

        public DynamicBlockController DynamicBlocksController => _dynamicBlockController;

        public DynamicScenePartHeader CurrentScenePartHeader { get; private set; }

        public SceneDefinition CurrentScene => _currentScene;

        public Level CurrentLevel
        {
            get => _currentLevel.Value;
            set => _currentLevel.Value = value;
        }

        public ChompGameModule(MainSystem mainSystem, InputModule inputModule, BankAudioModule audioModule,
           SpritesModule spritesModule, TileModule tileModule, MusicModule musicModule, RewardsModule rewardsModule,
           PaletteModule paletteModule)
           : base(mainSystem)
        {
            _audioService = mainSystem.GetModule<ChompAudioService>();
            _inputModule = inputModule;
            _spritesModule = spritesModule;
            _tileModule = tileModule;
            _musicModule = musicModule;
            RewardsModule = rewardsModule;
            _collisionDetector = new CollisionDetector(Specs);

            PaletteModule = paletteModule;
            ExitsModule = new ExitsModule(this);
            _statusBar = new StatusBar(this, GameRAM);
            _dynamicBlockController = new DynamicBlockController(this);
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());

            memoryBuilder.Memory.AddLabel(AddressLabels.MainTimer, memoryBuilder.CurrentAddress);

            _timer = memoryBuilder.AddByte();
            _deathTimer = memoryBuilder.AddByte();
            _currentLevel = new GameByteEnum<Level>(memoryBuilder.AddByte());

            //unused bits here
            _lastExitType = new NibbleEnum<ExitType>(new LowNibble(memoryBuilder));
            _carryingBomb = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);

            memoryBuilder.AddByte();

            _worldScroller = new WorldScroller(memoryBuilder, Specs, _tileModule, _spritesModule);

            _rasterInterrupts = new RasterInterrupts(this, GameSystem.CoreGraphicsModule);

            _rasterInterrupts.BuildMemory(memoryBuilder);

            _statusBar.BuildMemory(memoryBuilder);
            RewardsModule.BuildMemory(memoryBuilder);

            _scenePartsDestroyed = new ScenePartsDestroyed(memoryBuilder);

            //note, have unused bits here
            var freeRamOffset = new ExtendedByte2(
                memoryBuilder.AddByte(),
                new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0));

            GameRAM.Initialize(freeRamOffset, memoryBuilder.Memory);

            memoryBuilder.AddByte();

            memoryBuilder.AddLabel(AddressLabels.FreeRAM);
            memoryBuilder.AddBytes(Specs.GameRAMSize);

            memoryBuilder.AddLabel(AddressLabels.SpriteDefinitions);

            SpriteDefinitionBuilder.BuildSpriteDefinitions(memoryBuilder);

            _masterPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, 64, 64);

            memoryBuilder.AddLabel(AddressLabels.NameTables);
            memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);

            memoryBuilder.AddLabel(AddressLabels.SceneDefinitions);

            SceneBuilder.AddSceneHeaders(memoryBuilder, Specs);

            memoryBuilder.AddLabel(AddressLabels.SceneParts);
            SceneBuilder.AddSceneParts(memoryBuilder, Specs);

            ThemeBuilder.BuildThemes(memoryBuilder);

            _levelBossPosition = new GameByteGridPoint(memoryBuilder.AddByte(), memoryBuilder.AddByte(), 255, 255);
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

            _gameState.Value = GameState.NewGame;
            _audioService.OnStartup();

        }

        public void OnLogicUpdate()
        {
            _musicModule.Update();
            _audioService.Update();
            _timer.Value++;
          
            switch (_gameState.Value)
            {
                case GameState.NewGame:
                    InitGame();
                    break;
                case GameState.RestartScene:
                    RestartScene();
                    break;
                case GameState.LoadScene:
                    LoadScene();
                    break;
                case GameState.PlayScene:
                    PlayScene();
                    break;
                case GameState.Test:
                    break;
                case GameState.GameOver:
                    GameOver();
                    break;
            }
        }

        private void GameOver()
        {
            if(_deathTimer.Value == 0)
            {
                GameSystem.CoreGraphicsModule.PatternTable.Reset();
                TileModule.NameTable.Reset();
                TileModule.AttributeTable.Reset();

                _rasterInterrupts.SetScene(null, _levelBossPosition);
                PaletteModule.SetScene(null, Level.Level1_1_Start, GameSystem.Memory);

                var palette = GameSystem.CoreGraphicsModule.GetBackgroundPalette(0);
                palette.SetColor(0, ColorIndex.Black);
                palette.SetColor(2, ColorIndex.Red1);

                _masterPatternTable.CopyTilesTo(
                    destination: GameSystem.CoreGraphicsModule.PatternTable,
                    source: new InMemoryByteRectangle(4, 3, 7, 1),
                    destinationPoint: new Point(1, 0),
                    Specs,
                    GameSystem.Memory);

                _masterPatternTable.CopyTilesTo(
                   destination: GameSystem.CoreGraphicsModule.PatternTable,
                   source: new InMemoryByteRectangle(11, 3, 5, 1),
                   destinationPoint: new Point(0, 1),
                   Specs,
                   GameSystem.Memory);

                TileModule.TileStartX = 0;
                TileModule.TileStartY = 0;
                TileModule.Scroll.X = 0;
                TileModule.Scroll.Y = 0;

                int wordX = 4;
                int wordY = 16;

                TileModule.NameTable[wordX++, wordY] = 8;
                TileModule.NameTable[wordX++, wordY] = 6;
                TileModule.NameTable[wordX++, wordY] = 9;
                wordX++;
                TileModule.NameTable[wordX++, wordY] = 1;
                TileModule.NameTable[wordX++, wordY] = 10;
                TileModule.NameTable[wordX++, wordY] = 11;
                TileModule.NameTable[wordX++, wordY] = 12;

                _deathTimer.Value = 1;
            }

            if (GameSystem.CoreGraphicsModule.FadeAmount > 0)
                GameSystem.CoreGraphicsModule.FadeAmount--;

            if((_timer.Value % 16) == 0)
            {
                var palette = GameSystem.CoreGraphicsModule.GetBackgroundPalette(0);

                ColorIndex textColor = new ColorIndex(palette.GetColorIndex(2));
                palette.SetColor(2, textColor.LighterCycle().Value);
            }

            if ((_timer.Value % 2) == 0 
                && TileModule.Scroll.Y < 36)
            { 
                TileModule.Scroll.Y++;
            }

            _inputModule.OnLogicUpdate();
            if (_inputModule.Player1.StartKey == GameKeyState.Pressed)
            {
                _gameState.Value = GameState.NewGame;
                _deathTimer.Value = 0;
            }
        }

        private void InitGame()
        {
            _currentLevel.Value = Level.Level1_1_Start;
            _lastExitType.Value = ExitType.Right;
            GameSystem.CoreGraphicsModule.FadeAmount = 0;
            _statusBar.Score = 0;
            _statusBar.SetLives(StatusBar.InitialLives);
            _statusBar.Health = StatusBar.FullHealth;
            _gameState.Value = GameState.LoadScene;
        }

        public void RestartScene()
        {
            GameSystem.CoreGraphicsModule.FadeAmount = 0;
            _statusBar.Health = StatusBar.FullHealth;
            _gameState.Value = GameState.LoadScene;
        }

        public void LoadScene()
        {
            ResetSprites();

            _scenePartsDestroyed.SetCurrentLevel(_currentLevel.Value, GameSystem.Memory);
            _currentScene = new SceneDefinition(_currentLevel.Value, GameSystem.Memory, Specs);
            _levelBuilder = new LevelBuilder(this, _currentScene);

            GameRAM.Reset();

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

            PaletteModule.SetScene(_currentScene, _currentLevel.Value, GameSystem.Memory);

            _gameState.Value = GameState.PlayScene;
                       
            var levelMap =_levelBuilder.BuildNameTable(memoryBuilder, (int)_currentLevel.Value);

            CurrentScenePartHeader = new DynamicScenePartHeader(memoryBuilder, _currentLevel.Value);

            _levelBuilder.ApplyLevelAlterations(levelMap);

            var levelAttributeTable = _levelBuilder.BuildAttributeTable(memoryBuilder, levelMap);

            _levelBuilder.SetProperTiles(levelMap);

            _levelBuilder.BuildBackgroundNametable(levelMap);

            _sceneSpriteControllers.Initialize(_currentScene, levelMap, levelAttributeTable, _lastExitType.Value, _carryingBomb);

            _dynamicBlockController.InitializeDynamicBlocks(_currentScene, memoryBuilder, levelMap, levelAttributeTable, _sceneSpriteControllers.ExplosionControllers);
            _worldScroller.UpdateVram();

            _collisionDetector.Initialize(_currentScene, levelMap);
            _rasterInterrupts.SetScene(_currentScene, _levelBossPosition);

            ExitsModule.BuildMemory(memoryBuilder, _currentScene);

            GameSystem.CoreGraphicsModule.FadeAmount = 16;

            _spritesModule.SpriteStartIndex = (byte)(_currentScene.IsLevelBossScene ? 31 : 39);

            _musicModule.PlaySongForLevel(_currentLevel.Value);
        }

        private void ResetSprites()
        {
            for (int i = 0; i < Specs.MaxSprites; i++)
            {
                var sprite = SpritesModule.GetSprite(i);
                sprite.Visible = false;
                sprite.Tile = 0;
            }
        }

        public void PlayScene()
        {
            if(_deathTimer.Value == 0 && GameSystem.CoreGraphicsModule.FadeAmount > 0)
            {
                GameSystem.CoreGraphicsModule.FadeAmount--;
            }

            _sceneSpriteControllers.Update();

            if(_worldScroller.Update())
            {
                _sceneSpriteControllers.OnWorldScrollerUpdate();
            }

            HandlePlayerDeath();
            _rasterInterrupts.Update();

            _sceneSpriteControllers.CheckCollissions();

            PaletteModule.Update();

            RewardsModule.GiveRewards(_statusBar.Score, _sceneSpriteControllers);

            ExitsModule.CheckExits(_sceneSpriteControllers.Player, _currentScene);
            if(ExitsModule.ActiveExit.ExitType != ExitType.None)
            {
                GameDebug.DebugLog($"Exiting level {CurrentLevel} via {ExitsModule.ActiveExit.ExitType}");

                _gameState.Value = GameState.LoadScene;
                CurrentLevel = (Level)((int)CurrentLevel + ExitsModule.ActiveExit.ExitLevelOffset);
                GameDebug.DebugLog($"Entering level {CurrentLevel}");


                _lastExitType.Value = ExitsModule.ActiveExit.ExitType;

                _sceneSpriteControllers.CheckBombCarry(_carryingBomb);

                if (_carryingBomb.Value)
                    GameDebug.DebugLog("Player is carrying bomb");
                else
                    GameDebug.DebugLog("Player is NOT carrying bomb");
            }
        }

        private void HandlePlayerDeath()
        {
            if (_deathTimer.Value == 0 && _statusBar.Health == 0)
            {
                _audioService.PlaySound(ChompAudioService.Sound.PlayerDie);
                _deathTimer.Value = 1;
            }

            if (_deathTimer.Value > 0)
            {
                _deathTimer.Value++;

                if (_deathTimer > 64 && (_deathTimer.Value % 8)==0)
                    GameSystem.CoreGraphicsModule.FadeAmount++;

                if (_deathTimer.Value == 255)
                {
                    if(_statusBar.Lives > 0)
                        _gameState.Value = GameState.RestartScene;
                    else
                        _gameState.Value = GameState.GameOver;
                    _deathTimer.Value = 0;
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
            PaletteModule.OnHBlank();
            _tileModule.OnHBlank();
            _spritesModule.OnHBlank();
        }
    }
}
