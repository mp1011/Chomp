using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.ROM;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame
{
    class ChompGameModule : Module, IMasterModule
    {
        enum GameState : byte
        {
            LoadScene,
            PlayScene,
            Test
        }

        class RomAddresses
        {
            public int NameTables { get; set; }
            public int SceneDefinitions { get; set; }
        }

        private readonly CollisionDetector _collisionDetector;
        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;
        private readonly BankAudioModule _audioModule;
        private readonly TileModule _tileModule;
        private readonly StatusBarModule _statusBarModule;
        private readonly MusicModule _musicModule;

        private RomAddresses _romAddresses = new RomAddresses();
        private NBitPlane _masterPatternTable;
       
        private GameByteEnum<GameState> _gameState;
        private GameByte _timer;
        private MaskedByte _paletteCycleIndex;

        private PlayerController _playerController;
        private WorldScroller _worldScroller;
        public ChompGameModule(MainSystem mainSystem, InputModule inputModule, BankAudioModule audioModule,
           SpritesModule spritesModule, TileModule tileModule, StatusBarModule statusBarModule, MusicModule musicModule)
           : base(mainSystem)
        {
            _audioModule = audioModule;
            _inputModule = inputModule;
            _spritesModule = spritesModule;
            _tileModule = tileModule;
            _statusBarModule = statusBarModule;
            _musicModule = musicModule;
            _collisionDetector = new CollisionDetector(tileModule, Specs);
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _gameState = new GameByteEnum<GameState>(memoryBuilder.AddByte());
            _timer = memoryBuilder.AddByte();
            _paletteCycleIndex = new MaskedByte(memoryBuilder.CurrentAddress, (Bit)3, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            _playerController = new PlayerController(_spritesModule, _inputModule, _collisionDetector, _timer, memoryBuilder);

            _worldScroller = new WorldScroller(Specs, _tileModule, _spritesModule, _playerController.WorldSprite);

            memoryBuilder.BeginROM();
            _masterPatternTable = memoryBuilder.AddNBitPlane(Specs.PatternTablePlanes, 64, 64);

            _romAddresses.NameTables = memoryBuilder.CurrentAddress;                  
            memoryBuilder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);

            _romAddresses.SceneDefinitions = memoryBuilder.CurrentAddress;
            new SceneInfo(0, 4, memoryBuilder);            
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
            SceneInfo testScene = new SceneInfo(_romAddresses.SceneDefinitions, GameSystem.Memory);
            testScene.DefineRegion(
             index: 0,
             region: new InMemoryByteRectangle(0, 0, 2, 2),
             destination: new Point(0, 2),
             systemMemory: GameSystem.Memory);

            testScene.DefineRegion(
               index: 1,
               region: new InMemoryByteRectangle(0, 6, 8, 1),
               destination: new Point(0, 0),
               systemMemory: GameSystem.Memory);

            testScene.DefineRegion(
                index: 2,
                region: new InMemoryByteRectangle(0, 7, 6, 1),
                destination: new Point(0, 1),
                systemMemory: GameSystem.Memory);

            testScene.DefineRegion(
                index: 3,
                region: new InMemoryByteRectangle(5, 7, 3, 1),
                destination: new Point(5, 1),
                systemMemory: GameSystem.Memory);

            _tileModule.BackgroundPaletteIndex.Value = 0;

            return testScene;
        }

        public void OnLogicUpdate()
        {
            _musicModule.Update();

            _timer.Value++;
            if ((_timer.Value % 4) == 0)
            {
                _paletteCycleIndex.Value++;
            }

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


            var playerSprite = _spritesModule.GetSprite(0);
            playerSprite.X = 16;
            playerSprite.Y = 16;
            playerSprite.Tile = 1;
            playerSprite.Orientation = Orientation.Vertical;
            playerSprite.Tile2Offset = 1;
            playerSprite.Palette = 1;

            _gameState.Value = GameState.PlayScene;

            _playerController.Motion.XSpeed = 0;
            _playerController.Motion.YSpeed = 0;

            GameDebug.Watch1 = new DebugWatch(
                "Player X",
                () =>_playerController.WorldSprite.X);

            GameDebug.Watch2 = new DebugWatch(
                name: "Player Sprite X",
                () => playerSprite.X);

            GameDebug.Watch3 = new DebugWatch(
               name: "Player Sprite Y",
               () => playerSprite.Y);
        }

        public void PlayScene()
        {
            _playerController.Update();
            _worldScroller.Update();
        }

        public void OnVBlank()
        {
            _tileModule.OnVBlank();
            _spritesModule.OnVBlank();
        }

        public void OnHBlank()
        {
            if(GameSystem.CoreGraphicsModule.ScreenPoint.Y == 0)
            {
                var bgPalette = GameSystem.CoreGraphicsModule.GetPalette(0);
                bgPalette.SetColor(0, ChompGameSpecs.Black);
                bgPalette.SetColor(1, ChompGameSpecs.LightBlue);
                bgPalette.SetColor(2, ChompGameSpecs.LightYellow);
                bgPalette.SetColor(3, ChompGameSpecs.White);
            }
            else if (GameSystem.CoreGraphicsModule.ScreenPoint.Y == 40)
            {
                var bgPalette = GameSystem.CoreGraphicsModule.GetPalette(0);
               
                switch (_paletteCycleIndex.Value)
                {
                    case 0:
                        bgPalette.SetColor(0, ChompGameSpecs.Blue4);
                        bgPalette.SetColor(1, ChompGameSpecs.Blue1);
                        bgPalette.SetColor(2, ChompGameSpecs.Blue2);
                        bgPalette.SetColor(3, ChompGameSpecs.Blue3);
                        break;
                    case 1:
                        bgPalette.SetColor(0, ChompGameSpecs.Blue1);
                        bgPalette.SetColor(1, ChompGameSpecs.Blue2);
                        bgPalette.SetColor(2, ChompGameSpecs.Blue3);
                        bgPalette.SetColor(3, ChompGameSpecs.Blue4);
                        break;
                    case 2:
                        bgPalette.SetColor(0, ChompGameSpecs.Blue2);
                        bgPalette.SetColor(1, ChompGameSpecs.Blue3);
                        bgPalette.SetColor(2, ChompGameSpecs.Blue4);
                        bgPalette.SetColor(3, ChompGameSpecs.Blue1);
                        break;
                    case 3:
                        bgPalette.SetColor(0, ChompGameSpecs.Blue3);
                        bgPalette.SetColor(1, ChompGameSpecs.Blue4);
                        bgPalette.SetColor(2, ChompGameSpecs.Blue1);
                        bgPalette.SetColor(3, ChompGameSpecs.Blue2);
                        break;
                }
            }
            else if (GameSystem.CoreGraphicsModule.ScreenPoint.Y == 48)
            {
                var bgPalette = GameSystem.CoreGraphicsModule.GetPalette(0);
                bgPalette.SetColor(0, ChompGameSpecs.Green3);
                bgPalette.SetColor(1, ChompGameSpecs.Blue1);
                bgPalette.SetColor(2, ChompGameSpecs.Green1);
                bgPalette.SetColor(3, ChompGameSpecs.Green2);
                  
            }
            _tileModule.OnHBlank();
            _spritesModule.OnHBlank();
        }
    }
}
