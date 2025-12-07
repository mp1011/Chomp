using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteModels;
using ChompGame.MainGame.WorldScrollers;
using ChompGame.ROM;
using static ChompGame.GameSystem.MusicModule;

namespace ChompGame.MainGame
{
    public enum RomLoad
    {
        Code,
        Disk
    }

    public enum AddressLabels
    {
        MainTimer,
        SceneDefinitions,
        TransitionLevels,
        SpriteDefinitions,
        MasterPatternTable,
        SceneParts,
        FreeRAM,
        Themes,
        CartMemory,
        ROMBegin,

    }

    class ChompGameModule : Module, IMasterModule
    {
        private readonly RomLoad _romLoad;

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
            Title,
            Ending,
            Cheat
        }



        private CollisionDetector _collisionDetector;
        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private readonly ChompAudioService _audioService;
        private readonly TileModule _tileModule;
        private readonly MusicModule _musicModule;
        private readonly StatusBar _statusBar;
        private readonly DynamicBlockController _dynamicBlockController;

        private HighNibble _graceJumpCounter;

        public int GraceJumpCounter
        {
            get => _graceJumpCounter.Value;
            set => _graceJumpCounter.Value = (byte)value;
        }   

        private NBitPlane _masterPatternTable;
        private LevelBuilder _levelBuilder;

        private GameByteEnum<GameState> _gameState;
        private GameByteEnum<Level> _currentLevel;
        private TwoBit _saveSlot;
        private NibbleEnum<ExitType> _lastExitType;
        private NibbleEnum<ExitType> _lastTransitionExitType;

        public ExitType LastExitType => _lastExitType.Value;

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
       // private TileEditor _tileEditor;

        private GlitchCoreBgModule _glitchCoreBgModule;
        private MistAutoscrollBgModule _mistAutoscrollBgModule;
        private FinalBossHelper _finalBossHelper;

        private TitleScreen _titleScreen;
        private Ending _ending;
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

        public TileCopier TileCopier { get; private set; }

        public SaveManager SaveManager { get; }

        public FinalBossHelper FinalBossHelper => _finalBossHelper;
        
        public SceneSpriteControllers SceneSpriteControllers => _sceneSpriteControllers;

        public Level CurrentLevel
        {
            get => _currentLevel.Value;
            set => _currentLevel.Value = value;
        }

        public ChompGameModule(RomLoad romLoad, MainSystem mainSystem, InputModule inputModule, BankAudioModule audioModule,
           SpritesModule spritesModule, TileModule tileModule, MusicModule musicModule, RewardsModule rewardsModule,
           PaletteModule paletteModule)
           : base(mainSystem)
        {
            _romLoad = romLoad;
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
            //_tileEditor = new TileEditor(_tileModule);

            SaveManager = new SaveManager(this);
        }

        public void Cheat()
        {
            _longTimer.Value = 0;
            _gameState.Value = GameState.Cheat;
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());

            memoryBuilder.Memory.AddLabel(AddressLabels.MainTimer, memoryBuilder.CurrentAddress);

            _timer = memoryBuilder.AddByte();
            _longTimer = memoryBuilder.AddByte();
            _deathTimer = memoryBuilder.AddByte();
            _currentLevel = new GameByteEnum<Level>(memoryBuilder.AddByte());

            _lastExitType = new NibbleEnum<ExitType>(new LowNibble(memoryBuilder));
            _carryingBomb = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);
            _bossBackgroundHandling = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit5, memoryBuilder.Memory);
            _saveSlot = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);

            memoryBuilder.AddByte();

            _lastTransitionExitType = new NibbleEnum<ExitType>(new LowNibble(memoryBuilder));
            _graceJumpCounter = new HighNibble(memoryBuilder);
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


            if (_romLoad == RomLoad.Disk)
            {
                var romBytes = System.IO.File.ReadAllBytes("chomp.cart");
                memoryBuilder.AddLabel(AddressLabels.CartMemory);
                int romBegin = memoryBuilder.CurrentAddress + SaveManager.CartMemorySize;

                memoryBuilder.AddLabel(AddressLabels.ROMBegin, romBegin);
               
                foreach (var b in romBytes)
                {
                    memoryBuilder.AddByte(b);
                }

                var masterPatternTableAddress = new GameShort(romBegin, memoryBuilder.Memory);
                var sceneDefinitionsAddress = new GameShort(romBegin + 2, memoryBuilder.Memory);
                var transitionLevelsAddress = new GameShort(romBegin + 4, memoryBuilder.Memory);
                var scenePartsAddress = new GameShort(romBegin + 6, memoryBuilder.Memory);
                var themesAddress = new GameShort(romBegin + 8, memoryBuilder.Memory);

                memoryBuilder.AddLabel(AddressLabels.SpriteDefinitions, romBegin + 10);
                memoryBuilder.AddLabel(AddressLabels.MasterPatternTable, masterPatternTableAddress.Value);
                memoryBuilder.AddLabel(AddressLabels.SceneDefinitions, sceneDefinitionsAddress.Value);
                memoryBuilder.AddLabel(AddressLabels.TransitionLevels, transitionLevelsAddress.Value);
                memoryBuilder.AddLabel(AddressLabels.SceneParts, scenePartsAddress.Value);
                memoryBuilder.AddLabel(AddressLabels.Themes, themesAddress.Value);

                _masterPatternTable = NBitPlane.Create(masterPatternTableAddress.Value, memoryBuilder.Memory, Specs.PatternTablePlanes, 64, 64);
                SceneBuilder.LoadTransitionLevels(memoryBuilder.Memory);
            }
            else
            {
                memoryBuilder.AddLabel(AddressLabels.CartMemory);
                memoryBuilder.AddBytes(SaveManager.CartMemorySize);
                memoryBuilder.AddLabel(AddressLabels.ROMBegin);

                var masterPatternTableAddress = memoryBuilder.AddShort(); 
                var sceneDefinitionsAddress = memoryBuilder.AddShort();
                var transitionLevelsAddress = memoryBuilder.AddShort();
                var scenePartsAddress = memoryBuilder.AddShort();
                var themesAddress = memoryBuilder.AddShort();

                memoryBuilder.AddLabel(AddressLabels.SpriteDefinitions);
                SpriteDefinitionBuilder.BuildSpriteDefinitions(memoryBuilder);

                memoryBuilder.AddLabel(AddressLabels.MasterPatternTable);
                _masterPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, 64, 64);
               
                memoryBuilder.AddLabel(AddressLabels.SceneDefinitions);
                SceneBuilder.AddSceneHeaders(memoryBuilder, Specs);

                memoryBuilder.AddLabel(AddressLabels.TransitionLevels);
                SceneBuilder.SetTransitionLevels(memoryBuilder);

                memoryBuilder.AddLabel(AddressLabels.SceneParts);
                SceneBuilder.AddSceneParts(memoryBuilder, Specs);

                memoryBuilder.AddLabel(AddressLabels.Themes);
                ThemeBuilder.BuildThemes(memoryBuilder);

                masterPatternTableAddress.Value = (ushort)memoryBuilder.Memory.GetAddress(AddressLabels.MasterPatternTable);
                sceneDefinitionsAddress.Value = (ushort)memoryBuilder.Memory.GetAddress(AddressLabels.SceneDefinitions);
                transitionLevelsAddress.Value = (ushort)memoryBuilder.Memory.GetAddress(AddressLabels.TransitionLevels);
                scenePartsAddress.Value = (ushort)memoryBuilder.Memory.GetAddress(AddressLabels.SceneParts);
                themesAddress.Value = (ushort)memoryBuilder.Memory.GetAddress(AddressLabels.Themes);
            }

            TileCopier = new TileCopier(_masterPatternTable, this);
            _collisionDetector = new CollisionDetector(Specs, _bossBackgroundHandling);
            _levelCard = new LevelCard(this, _longTimer);
            _titleScreen = new TitleScreen(this, _longTimer, memoryBuilder);
            _ending = new Ending(this, _longTimer, memoryBuilder);

            _deathHandler = new PlayerDeathHandler(this, _deathTimer, _statusBar, _gameState);
        }

        public override void OnStartup()
        {
            if (_romLoad == RomLoad.Code)
            {
                PatternTableCreator.SetupMasterPatternTable(
                    _masterPatternTable,
                    GameSystem.GraphicsDevice,
                    Specs);

                new DiskNBitPlaneLoader()
                    .Load(new DiskFile(ContentFolder.PatternTables, "master.pt"),
                        _masterPatternTable);
            }

            _gameState.Value = GameState.NewGame;
            _audioService.OnStartup();

            if(_romLoad == RomLoad.Code)
            {
                // write cart to disk
                var cart = GameSystem.Memory.Span(GameSystem.Memory.GetAddress(AddressLabels.CartMemory), -1);
                System.IO.File.WriteAllBytes("chomp.cart", cart);
            }
        }

        public void Reset()
        {
            _gameState.Value = GameState.NewGame;
            _longTimer.Value = 0;
        }

        public void OnLogicUpdate()
        {
            _musicModule.Update();
            _audioService.Update();

            if(_gameState.Value != GameState.Paused)
                _timer.Value++;

            switch (_gameState.Value)
            {
                case GameState.Cheat:
                    CheatMenu();
                    break;
                case GameState.NewGame:
                    InitGame();
                    break;
                case GameState.RestartScene:
                    RestartScene();
                    break;
                case GameState.LoadScene:
                    if (_currentLevel.Value == Level.Ending)
                    {
                        _longTimer.Value = 0;
                        _gameState.Value = GameState.Ending;
                    }
                    else
                        LoadScene();
                    break;
                case GameState.Ending:
                    if(_ending.Update())
                    {
                        _gameState.Value = GameState.Title;
                        _longTimer.Value = 0;
                    }
                    break;
                case GameState.PlayScene:
                    if (_timer.Value.IsMod(16))
                    {
                        if(_currentScene.Theme != ThemeType.MistAutoscroll || _longTimer.Value < 255)
                            _longTimer.Value++;
                    }

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
                   // if(!_tileEditor.Update())
                        _gameState.Value = GameState.PlayScene;
                    break;
                case GameState.LevelCard:
                    _bossBackgroundHandling.Value = false;
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

        private void CheatMenu()
        {
            if (_longTimer.Value == 0)
            {
                _musicModule.CurrentSong = MusicModule.SongName.None;
                TileCopier.CopyTilesForStatusBar();

                _tileModule.NameTable.Reset();
                _tileModule.AttributeTable.Reset();
                _tileModule.Scroll.X = 0;
                _tileModule.Scroll.Y = 0;

                _tileModule.NameTable[7, 7] = 16;
                _tileModule.NameTable[7, 9] = 16;

                _longTimer.Value = 1;
                GameSystem.CoreGraphicsModule.FadeAmount = 0;

                var palette = GameSystem.CoreGraphicsModule.GetBackgroundPalette(0);
                palette.SetColor(0, ColorIndex.Black);
                palette.SetColor(1, ColorIndex.Yellow1);
                palette.SetColor(2, ColorIndex.Gray1);
                palette.SetColor(3, ColorIndex.White);
            }
            else
            {
                _inputModule.OnLogicUpdate();
                int levelNum = _tileModule.NameTable[7, 7] - 15;
                if(levelNum < 7 && _inputModule.Player1.UpKey == GameKeyState.Pressed)
                {
                    _tileModule.NameTable[7, 7] = (byte)(_tileModule.NameTable[7, 7] + 1);
                    _audioService.PlaySound(ChompAudioService.Sound.ButtonPress);
                }
                if (levelNum > 1 && _inputModule.Player1.DownKey == GameKeyState.Pressed)
                {
                    _tileModule.NameTable[7, 7] = (byte)(_tileModule.NameTable[7, 7] - 1);
                    _audioService.PlaySound(ChompAudioService.Sound.ButtonPress);
                }

                int subNum = _tileModule.NameTable[7, 9] - 15;
                if (subNum < 4 && _inputModule.Player1.RightKey == GameKeyState.Pressed)
                {
                    _tileModule.NameTable[7, 9] = (byte)(_tileModule.NameTable[7, 9] + 1);
                    _audioService.PlaySound(ChompAudioService.Sound.ButtonPress);
                }
                if (subNum > 1 && _inputModule.Player1.LeftKey == GameKeyState.Pressed)
                {
                    _tileModule.NameTable[7, 9] = (byte)(_tileModule.NameTable[7, 9] - 1);
                    _audioService.PlaySound(ChompAudioService.Sound.ButtonPress);
                }

                _musicModule.CurrentSong = CheatSong(levelNum, subNum);

                if (_inputModule.Player1.StartKey == GameKeyState.Pressed)
                {
                    StartGame();
                    _audioService.PlaySound(ChompAudioService.Sound.Break);
                    _statusBar.SetLives(9);

                    _currentLevel.Value = CheatStartLevel(levelNum, subNum);
                    _gameState.Value = GameState.LoadScene;
                }
            }
        }

        private MusicModule.SongName CheatSong(int levelNum, int subNum)
        {
            return levelNum switch {
                1 => subNum switch {
                    1 => SongName.Adventure,
                    2 => SongName.Threat,
                    3 => SongName.Adventure2,
                    _ => SongName.Nemesis
                },
                2 => subNum switch {
                    1 => SongName.Flight,
                    2 => SongName.Threat,
                    3 => SongName.SeaDreams,
                    _ => SongName.Nemesis
                },
                3 => subNum switch {
                    1 => SongName.City,
                    2 => SongName.Threat,
                    3 => SongName.Railway,
                    _ => SongName.Nemesis
                },
                4 => subNum switch {
                    1 => SongName.Dusk,
                    2 => SongName.Threat,
                    3 => SongName.Stronghold,
                    _ => SongName.Nemesis
                },
                5 => subNum switch {
                    1 => SongName.Moonstruck,
                    2 => SongName.Threat,
                    3 => SongName.Space,
                    _ => SongName.Nemesis
                },
                6 => subNum switch {
                    1 => SongName.CommandCenter,
                    2 => SongName.Threat,
                    3 => SongName.Infiltration,
                    _ => SongName.Nemesis
                },
                _ => subNum switch {
                    1 => SongName.SystemMalfunction,
                    2 => SongName.VeryDefinitelyFinalDungeon,
                    3 => SongName.FinalFight,
                    _ => SongName.Ending
                },
            };
        }

        private Level CheatStartLevel(int levelNum, int subNum)
        {
            return levelNum switch {
                1 => subNum switch {
                    1 => Level.Level1_1_Start,
                    2 => Level.Level1_11_Boss,
                    3 => Level.Level1_12_Horizontal2,
                    _ => Level.Level1_17_Boss
                },
                2 => subNum switch {
                    1 => Level.Level2_1_Intro,
                    2 => Level.Level2_2_Fly2,
                    3 => Level.Level2_3_Beach,
                    _ => Level.Level2_12_Boss
                },
                3 => subNum switch {
                    1 => Level.Level3_1_City,
                    2 => Level.Level3_20_Midboss,
                    3 => Level.Level3_21_CityAfterMidboss,
                    _ => Level.Level3_26_Boss
                },
                4 => subNum switch {
                    1 => Level.Level4_1_Desert,
                    2 => Level.Level4_31_Midboss,
                    3 => Level.Level4_32_Desert4,
                    _ => Level.Level4_40_Boss
                },
                5 => subNum switch {
                    1 => Level.Level5_1_Mist,
                    2 => Level.Level5_22_MidBoss,
                    3 => Level.Level5_23_Plane_Begin,
                    _ => Level.Level5_27_Boss
                },
                6 => subNum switch {
                    1 => Level.Level6_1_Techbase,
                    2 => Level.Level6_17_Midboss,
                    3 => Level.Level6_18_Techbase11,
                    _ => Level.Level6_37_Boss
                },
                _ => subNum switch {
                    1 => Level.Level7_1_GlitchCore,
                    2 => Level.Level7_16_RunRoom,
                    3 => Level.Level7_40_FinalBoss,
                    _ => Level.Level7_41_FinalBossEpilogue
                },
            };
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

                TileCopier.CopyTilesForGameOver();

                TileModule.Scroll.X = 0;
                TileModule.Scroll.Y = 0;

                int wordX = 4;
                int wordY = 16;

                TileModule.NameTable[wordX++, wordY] = 9;
                TileModule.NameTable[wordX++, wordY] = 6;
                TileModule.NameTable[wordX++, wordY] = 10;
                wordX++;
                TileModule.NameTable[wordX++, wordY] = 8;
                TileModule.NameTable[wordX++, wordY] = 6;
                TileModule.NameTable[wordX++, wordY] = 11;
                TileModule.NameTable[wordX++, wordY] = 3;

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
            _currentLevel.Value = Level.Level1_1_Start;
            _gameState.Value = GameState.Title;
            GameSystem.CoreGraphicsModule.FadeAmount = 0;
            _statusBar.Score = 0;
            _statusBar.SetLives(StatusBar.InitialLives);
            _statusBar.Health = StatusBar.FullHealth;
            _lastExitType.Value = ExitType.Right;
        }

        public void RestartScene()
        {
            _statusBar.Health = StatusBar.FullHealth;
            _gameState.Value = GameState.LevelCard;
            _levelCard.Reset();

            if(_currentLevel.Value != Level.Level7_40_FinalBoss)
                _scenePartsDestroyed.Reset(GameSystem.Memory);

            _lastExitType.Value = _lastTransitionExitType.Value;
        }

        public void StartGame()
        {
            _saveSlot.Value = (byte)SaveManager.FreeSlot();
            _longTimer.Value = 0;
            _timer.Value = 0;
            _gameState.Value = GameState.LevelCard;
            _currentLevel.Value = Level.Level1_1_Start;
            GameSystem.CoreGraphicsModule.FadeAmount = 0;
            _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.None;
            _lastExitType.Value = ExitType.Right;
            GameSystem.CoreGraphicsModule.FadeAmount = 0;
            _statusBar.Score = 0;
            _statusBar.SetLives(StatusBar.InitialLives);
            _statusBar.Health = StatusBar.FullHealth;
            _scenePartsDestroyed.Reset(GameSystem.Memory);
        }

        public void LoadGame(int saveSlot)
        {
            _saveSlot.Value = (byte)saveSlot;

            var saveAddress = SaveManager.SaveSlotAddress(saveSlot);

            // 0: current level
            // 1 - 4: score
            // 5: lives and carrying bomb(high bit)
            // 6: last exit-type(low) and health(high)
            // 7-22: scene parts destroyed
            _currentLevel.Value = (Level)GameSystem.Memory[saveAddress];
            GameSystem.Memory.BlockCopy(saveAddress + 1, _statusBar.ScorePtr, 4);

            var livesAndCarryingBomb = GameSystem.Memory[saveAddress + 5];
            _statusBar.SetLives((byte)(livesAndCarryingBomb & 0x0F));
            _carryingBomb.Value = (livesAndCarryingBomb & 0x80) != 0;

            var exitTypeAndHealth = GameSystem.Memory[saveAddress + 6];

            _lastExitType.Value = (ExitType)(exitTypeAndHealth & 0x0F);
            _statusBar.Health = (byte)(exitTypeAndHealth >> 4);
            _scenePartsDestroyed.ReadFromSaveBuffer(GameSystem.Memory, saveAddress + 7);

            _longTimer.Value = 0;
            _timer.Value = 0;
            _gameState.Value = GameState.LevelCard;
            _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.None;
            GameSystem.CoreGraphicsModule.FadeAmount = 0;
        }

        public void LoadScene()
        {
            GraceJumpCounter = 0;
            SaveCurrentGame();
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
                GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                GameSystem.CoreGraphicsModule.SpritePatternTable,
                GameSystem.Memory);

            //PatternTableExporter.ExportPatternTable(
            //    GameSystem.GraphicsDevice, 
            //    GameSystem.CoreGraphicsModule.SpritePatternTable,
            //    "spritePatternTable.png");

            //PatternTableExporter.ExportPatternTable(
            //    GameSystem.GraphicsDevice,
            //    GameSystem.CoreGraphicsModule.BackgroundPatternTable,
            //    "backgroundPatternTable.png");



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
            if (ExitsModule.ActiveExit.ExitType != ExitType.None && GameSystem.CoreGraphicsModule.FadeAmount >= 1)
            {
                GameSystem.CoreGraphicsModule.FadeAmount++;

                if(GameSystem.CoreGraphicsModule.FadeAmount == 15)
                    HandleLevelExit();

                return;
            }
            
            if (_deathTimer.Value == 0 && GameSystem.CoreGraphicsModule.FadeAmount > 0)
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
                GameSystem.CoreGraphicsModule.FadeAmount = 1;
                GameDebug.DebugLog($"Exiting level {CurrentLevel} via {ExitsModule.ActiveExit.ExitType}", DebugLogFlags.LevelTransition); 
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

        private void HandleLevelExit()
        {
            var newLevel = (Level)((int)CurrentLevel + ExitsModule.ActiveExit.ExitLevelOffset);

            if (newLevel > CurrentLevel && SceneBuilder.IsTransitionLevel(newLevel))
            {
                _scenePartsDestroyed.Reset(GameSystem.Memory);
                if(IsLevelStart(newLevel))
                {
                    _statusBar.Health = StatusBar.FullHealth;
                    _lastTransitionExitType.Value = ExitType.Right;
                }
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

        private void SaveCurrentGame()
        {
            SaveManager.SaveCurrentGame(_saveSlot.Value, _carryingBomb.Value);
        }

        public void OnVBlank()
        {
            _tileModule.OnVBlank();
            _spritesModule.OnVBlank();
        }

        public void OnHBlank()
        {
            _rasterInterrupts.OnHBlank();
            _bossBackgroundHandler.OnHBlank();
            if (_gameState.Value == GameState.LevelCard)
                _levelCard.OnHBlank();
            else if (_gameState.Value == GameState.Title)
                _titleScreen.OnHBlank();
            else if (_gameState.Value == GameState.Ending)
                _ending.OnHBlank();
       
            PaletteModule.OnHBlank();
            _tileModule.OnHBlank();
            _spritesModule.OnHBlank();

            _bossBackgroundHandler.OnHBlank();
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
    }
}
