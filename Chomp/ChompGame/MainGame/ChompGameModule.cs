using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.Helpers;
using ChompGame.MainGame.Editors;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteModels;
using ChompGame.MainGame.WorldScrollers;
using ChompGame.ROM;
using Microsoft.Xna.Framework;
using System.Linq;

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
        public enum GameState : byte
        {
            NewGame,
            RestartScene,
            LoadScene,
            PlayScene,
            Test,
            GameOver,
            TileEditor,
            LevelCard,
            Paused,
            Title
        }



        private CollisionDetector _collisionDetector;
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
        private NibbleEnum<ExitType> _lastTransitionExitType;

        private GameBit _carryingBomb;

        private GameByte _timer;
        private GameByte _longTimer;
        private GameByte _deathTimer;

        private GameBit _bossBackgroundHandling;

        private SceneSpriteControllers _sceneSpriteControllers;
        private WorldScroller _worldScroller;
        private RasterInterrupts _rasterInterrupts;
        private BossBackgroundHandler _bossBackgroundHandler;
        private SceneDefinition _currentScene;
        private ScenePartsDestroyed _scenePartsDestroyed;
        private TileEditor _tileEditor;

        private GlitchCoreBgModule _glitchCoreBgModule;
        private MistAutoscrollBgModule _mistAutoscrollBgModule;
        private FinalBossHelper _finalBossHelper;

        private TitleScreen _titleScreen;
        private LevelCard _levelCard;
        private PlayerDeathHandler _deathHandler;

        public GameBit BossBackgroundHandling => _bossBackgroundHandling;

        public BossBackgroundHandler BossBackgroundHandler => _bossBackgroundHandler;
        public ScenePartsDestroyed ScenePartsDestroyed => _scenePartsDestroyed;

        public GameRAM GameRAM => GameSystem.GameRAM;

        public RasterInterrupts RasterInterrupts => _rasterInterrupts;
        public TileModule TileModule => _tileModule;
        public SpritesModule SpritesModule => _spritesModule;
        public SpriteTileTable SpriteTileTable { get; private set; }
        public GameByte LevelTimer => _timer;
        public GameByte LevelTimerLong => _longTimer;
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

        public RandomModule RandomModule { get; }

        public FinalBossHelper FinalBossHelper => _finalBossHelper;

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
            RandomModule = mainSystem.GetModule<RandomModule>();
            _inputModule = inputModule;
            _spritesModule = spritesModule;
            _tileModule = tileModule;
            _musicModule = musicModule;
            RewardsModule = rewardsModule;

            PaletteModule = paletteModule;
            ExitsModule = new ExitsModule(this);
            _statusBar = new StatusBar(this, GameRAM);
            SpriteTileTable = new SpriteTileTable();
            _dynamicBlockController = new DynamicBlockController(this, SpriteTileTable);
            _tileEditor = new TileEditor(_tileModule);
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());

            memoryBuilder.Memory.AddLabel(AddressLabels.MainTimer, memoryBuilder.CurrentAddress);

            _timer = memoryBuilder.AddByte();
            _longTimer = memoryBuilder.AddByte();
            _deathTimer = memoryBuilder.AddByte();
            _currentLevel = new GameByteEnum<Level>(memoryBuilder.AddByte());

            //unused bits here
            _lastExitType = new NibbleEnum<ExitType>(new LowNibble(memoryBuilder));
            _carryingBomb = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);
            _bossBackgroundHandling = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit5, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            _lastTransitionExitType = new NibbleEnum<ExitType>(new LowNibble(memoryBuilder));
            // unused bits
            memoryBuilder.AddByte();
            _lastTransitionExitType.Value = ExitType.Right;

            _rasterInterrupts = new RasterInterrupts(this, GameSystem.CoreGraphicsModule);
            _bossBackgroundHandler = new BossBackgroundHandler(this, _rasterInterrupts);

            _rasterInterrupts.BuildMemory(memoryBuilder);
            _bossBackgroundHandler.BuildMemory(memoryBuilder);
            _statusBar.BuildMemory(memoryBuilder);
            RewardsModule.BuildMemory(memoryBuilder);

            _scenePartsDestroyed = new ScenePartsDestroyed(memoryBuilder);

            SpriteTileTable.BuildMemory(memoryBuilder);

            var freeRamOffset = memoryBuilder.AddShort();
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
            SceneBuilder.SetTransitionLevels(memoryBuilder);

            memoryBuilder.AddLabel(AddressLabels.SceneParts);
            SceneBuilder.AddSceneParts(memoryBuilder, Specs);

            ThemeBuilder.BuildThemes(memoryBuilder);

            _collisionDetector = new CollisionDetector(Specs, _bossBackgroundHandling);
            _levelCard = new LevelCard(this, _longTimer, _masterPatternTable);
            _titleScreen = new TitleScreen(this, _longTimer, _masterPatternTable);

            _deathHandler = new PlayerDeathHandler(this, _deathTimer, _statusBar, _gameState);
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

            if(_gameState.Value != GameState.Paused)
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
                    if (_timer.Value.IsMod(16))
                        _longTimer.Value++;

                    if (_tileEditor.CheckActivation())
                        _gameState.Value = GameState.TileEditor;
                    else
                        PlayScene();

                    if (GameDebug.LevelSkipEnabled && _inputModule.Player1.StartKey == GameKeyState.Pressed && _inputModule.Player1.UpKey == GameKeyState.Down)
                    {
                        ExitsModule.GotoNextLevel();
                    }
                    else if (CheckPause())
                        _gameState.Value = GameState.Paused;

                    break;
                case GameState.Test:
                    break;
                case GameState.GameOver:
                    GameOver();
                    break;
                case GameState.TileEditor:
                    if(!_tileEditor.Update())
                        _gameState.Value = GameState.PlayScene;
                    break;
                case GameState.LevelCard:
                    _inputModule.OnLogicUpdate();
                    if (_levelCard.Update())
                        _gameState.Value = GameState.LoadScene;
                    break;
                case GameState.Title:
                    _inputModule.OnLogicUpdate();
                    _titleScreen.Update();
                    break;
                case GameState.Paused:
                    _inputModule.OnLogicUpdate();
                    if (_inputModule.Player1.StartKey == GameKeyState.Pressed)
                    {
                        GameSystem.CoreGraphicsModule.FadeAmount = 0;
                        _audioService.PlaySound(ChompAudioService.Sound.ButtonPress);
                        _musicModule.Resume();
                        _gameState.Value = GameState.PlayScene;
                    }
                    break;

            }
        }

        private bool CheckPause()
        {
            if(_deathTimer.Value == 0 && _inputModule.Player1.StartKey == GameKeyState.Pressed)
            {
                _audioService.PlaySound(ChompAudioService.Sound.ButtonPress);
                _musicModule.Pause(); 
                GameSystem.CoreGraphicsModule.FadeAmount = 2;
                return true;
            }

            return false;
        }


        private void GameOver()
        {
            if(_deathTimer.Value == 0)
            {
                for (int i = 0; i < Specs.MaxSprites; i++)
                {
                    var sprite = SpritesModule.GetSprite(i);
                    sprite.Visible = false;
                    sprite.Tile = 0;
                }

                GameSystem.CoreGraphicsModule.BackgroundPatternTable.Reset();
                TileModule.NameTable.Reset();
                TileModule.AttributeTable.Reset();

                _rasterInterrupts.SetScene(null);
                PaletteModule.SetScene(null, Level.Level1_1_Start, GameSystem.Memory, _bossBackgroundHandling);

                var palette = GameSystem.CoreGraphicsModule.GetBackgroundPalette(0);
                palette.SetColor(0, ColorIndex.Black);
                palette.SetColor(2, ColorIndex.Red1);

                _masterPatternTable.CopyTilesTo(
                    destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                    source: new InMemoryByteRectangle(4, 3, 7, 1),
                    destinationPoint: new Point(1, 0),
                    Specs,
                    GameSystem.Memory);

                _masterPatternTable.CopyTilesTo(
                   destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                   source: new InMemoryByteRectangle(11, 3, 5, 1),
                   destinationPoint: new Point(0, 1),
                   Specs,
                   GameSystem.Memory);

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
                RestartLevel();
                _deathTimer.Value = 0;
            }
        }

        private void RestartLevel()
        {
            _currentLevel.Value = GameOverRestartLevel();
            _statusBar.Score = 0;
            _statusBar.AddToScore(0);
            _statusBar.SetLives(StatusBar.InitialLives);
            RestartScene();
        }

        private Level GameOverRestartLevel()
        {
            if (_currentLevel.Value < Level.Level2_1_Intro)
                return Level.Level1_1_Start;
            else if (_currentLevel.Value < Level.Level3_1_City)
                return Level.Level2_1_Intro;
            else if (_currentLevel.Value < Level.Level4_1_Desert)
                return Level.Level3_1_City;
            else if (_currentLevel.Value < Level.Level5_1_Mist)
                return Level.Level4_1_Desert;
            else if (_currentLevel.Value < Level.Level6_1_Techbase)
                return Level.Level5_1_Mist;
            else if (_currentLevel.Value < Level.Level7_1_GlitchCore)
                return Level.Level6_1_Techbase;
            else
                return Level.Level7_1_GlitchCore;
        }

        private void InitGame()
        {
            _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.None;
            _currentLevel.Value = Level.Level5_1_Mist;
            _lastExitType.Value = ExitType.Right;
            GameSystem.CoreGraphicsModule.FadeAmount = 0;
            _statusBar.Score = 0;
            _statusBar.SetLives(StatusBar.InitialLives);
            _statusBar.Health = StatusBar.FullHealth;
            _gameState.Value = GameState.LevelCard;
        }

        public void RestartScene()
        {
            _statusBar.Health = StatusBar.FullHealth;
            _gameState.Value = GameState.LevelCard;
            _levelCard.Reset();
            _scenePartsDestroyed.Reset(GameSystem.Memory);
            _lastExitType.Value = _lastTransitionExitType.Value;
        }

        public void LoadScene()
        {
            _bossBackgroundHandling.Value = false;
            _longTimer.Value = 0;
            _tileModule.NameTable.Reset();
            _tileModule.AttributeTable.Reset();

            ResetSprites();

            _scenePartsDestroyed.SetCurrentLevel(_currentLevel.Value, GameSystem.Memory);
            _currentScene = new SceneDefinition(_currentLevel.Value, GameSystem.Memory, Specs);
          
            _levelBuilder = new LevelBuilder(this, _currentScene);

            GameRAM.Reset();

            SystemMemoryBuilder memoryBuilder = new SystemMemoryBuilder(GameSystem.Memory, 
                Specs,
                GameRAM);

            if (_currentScene.IsAutoScroll)
            {
                _worldScroller = new AutoscrollWorldScroller(memoryBuilder, Specs, _tileModule, _spritesModule);
            }
            else if (_currentScene.Theme == ThemeType.TechBaseBoss)
                _worldScroller = new Level6BossScroller(memoryBuilder, Specs, _tileModule, _spritesModule, _timer);
            else
                _worldScroller = _currentScene.ScrollStyle switch {
                    ScrollStyle.Horizontal => new HorizontalWorldScroller(memoryBuilder, Specs, _tileModule, _spritesModule),
                    ScrollStyle.Vertical => new VerticalWorldScroller(memoryBuilder, Specs, _tileModule, _spritesModule, _statusBar),
                    ScrollStyle.NameTable => new NametableScroller(memoryBuilder, Specs, _tileModule, _spritesModule, _currentScene.IsLevelBossScene),
                    _ => new NoScroller(memoryBuilder, Specs, _tileModule, _spritesModule)
                };

            _sceneSpriteControllers = _levelBuilder.CreateSpriteControllers(memoryBuilder);

            _levelBuilder.SetupVRAMPatternTable(
                _masterPatternTable,
                GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                GameSystem.CoreGraphicsModule.SpritePatternTable,
                GameSystem.Memory);

            PatternTableExporter.ExportPatternTable(
                GameSystem.GraphicsDevice, 
                GameSystem.CoreGraphicsModule.SpritePatternTable,
                "spritePatternTable.png");

            PatternTableExporter.ExportPatternTable(
                GameSystem.GraphicsDevice,
                GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                "backgroundPatternTable.png");



            PaletteModule.SetScene(_currentScene, _currentLevel.Value, GameSystem.Memory, _bossBackgroundHandling);
            RewardsModule.SetScene(_currentScene);

            _gameState.Value = GameState.PlayScene;
                       
            var levelMap =_levelBuilder.BuildNameTable(memoryBuilder, (byte)_currentLevel.Value);

            CurrentScenePartHeader = new DynamicScenePartHeader(memoryBuilder, _currentLevel.Value);

            _levelBuilder.ApplyLevelAlterations(levelMap);

            _levelBuilder.AddExitTiles(levelMap);

            var levelAttributeTable = _levelBuilder.BuildAttributeTable(memoryBuilder, levelMap);

            _levelBuilder.SetProperTiles(levelMap);

            _levelBuilder.AddTurrets(levelMap);

            _levelBuilder.BuildBackgroundNametable(levelMap, levelAttributeTable);

            _sceneSpriteControllers.Initialize(_currentScene, levelMap, levelAttributeTable, _lastExitType.Value, _carryingBomb);

            _dynamicBlockController.InitializeDynamicBlocks(_currentScene, memoryBuilder, levelMap, levelAttributeTable, _sceneSpriteControllers.ExplosionControllers);

            _worldScroller.RefreshNametable();

            _collisionDetector.Initialize(_currentScene, levelMap, SpriteTileTable);
            _rasterInterrupts.SetScene(_currentScene);

            ExitsModule.BuildMemory(memoryBuilder, _currentScene);

            GameSystem.CoreGraphicsModule.FadeAmount = 16;

            _musicModule.PlaySongForLevel(_currentLevel.Value);

            if(_currentScene.Theme == ThemeType.GlitchCore || _currentScene.Theme == ThemeType.Final)
            {
                _glitchCoreBgModule = new GlitchCoreBgModule(memoryBuilder, this, CurrentLevel == Level.Level7_16_RunRoom);
            }
            else if(_currentScene.Theme == ThemeType.FinalBoss)
            {
                _finalBossHelper = new FinalBossHelper(
                    this,
                    memoryBuilder,
                    _masterPatternTable,
                    _sceneSpriteControllers,
                    SpriteTileTable,
                    GameSystem.CoreGraphicsModule,
                    GameSystem.Memory);
            }
            else if(_currentScene.Theme == ThemeType.MistAutoscroll)
            {
                _mistAutoscrollBgModule = new MistAutoscrollBgModule(this);
            }
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

            _sceneSpriteControllers.CheckCollisions();

            PaletteModule.Update();
            RewardsModule.Update(_statusBar, _sceneSpriteControllers);

            ExitsModule.CheckExits(_sceneSpriteControllers.Player, _currentScene);
            if(ExitsModule.ActiveExit.ExitType != ExitType.None)
            {
                GameDebug.DebugLog($"Exiting level {CurrentLevel} via {ExitsModule.ActiveExit.ExitType}", DebugLogFlags.LevelTransition);

                var newLevel = (Level)((int)CurrentLevel + ExitsModule.ActiveExit.ExitLevelOffset);
                if(newLevel > CurrentLevel && SceneBuilder.IsTransitionLevel(newLevel))
                {
                    _scenePartsDestroyed.Reset(GameSystem.Memory);
                }
                CurrentLevel = newLevel;

                _gameState.Value = IsLevelStart(newLevel) ? GameState.LevelCard : GameState.LoadScene;
                if (ExitsModule.ActiveExit.ExitLevelOffset == -1)
                    _gameState.Value = GameState.LoadScene;

                _levelCard.Reset();
                GameDebug.DebugLog($"Entering level {CurrentLevel}", DebugLogFlags.LevelTransition);


                _lastExitType.Value = ExitsModule.ActiveExit.ExitType;
                if (SceneBuilder.IsTransitionLevel(CurrentLevel))
                    _lastTransitionExitType.Value = _lastExitType.Value;

                _sceneSpriteControllers.CheckBombCarry(_carryingBomb);

                if (_carryingBomb.Value)
                    GameDebug.DebugLog("Player is carrying bomb", DebugLogFlags.LevelTransition);
                else
                    GameDebug.DebugLog("Player is NOT carrying bomb", DebugLogFlags.LevelTransition);
            }

            if(_currentScene.Theme == ThemeType.GlitchCore || _currentScene.Theme == ThemeType.Final)
            {
                _glitchCoreBgModule.Update();
            }
            else if(_currentScene.Theme == ThemeType.MistAutoscroll)
            {
                _mistAutoscrollBgModule.Update();
            }
        }

        private bool IsLevelStart(Level level)
        {
            return level == Level.Level1_1_Start
                || level == Level.Level2_1_Intro
                || level == Level.Level3_1_City
                || level == Level.Level4_1_Desert
                || level == Level.Level5_1_Mist
                || level == Level.Level6_1_Techbase
                || level == Level.Level7_1_GlitchCore;
        }
        private void HandlePlayerDeath()
        {
            _deathHandler.HandlePlayerDeath();
        }

        public void OnVBlank()
        {
            _tileModule.OnVBlank();
            _spritesModule.OnVBlank();
        }

        public void OnHBlank()
        {
            if (!_tileEditor.IsRunning)
            {
                _rasterInterrupts.OnHBlank();
                _bossBackgroundHandler.OnHBlank();
                if (_gameState.Value == GameState.LevelCard)
                    _levelCard.OnHBlank();
            }

            PaletteModule.OnHBlank();
            _tileModule.OnHBlank();
            _spritesModule.OnHBlank();

            _bossBackgroundHandler.OnHBlank();
        }
    }
}
