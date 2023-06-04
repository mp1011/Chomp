using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.Graphics;

namespace ChompGame.MainGame.SceneModels
{
    public enum PaletteKey : byte
    {
        Test,
        PlainsGround,
        PlainsNight,
        PlainsFarMountains,
        PlainsCloseMountains,
        PlainsEveningFarMountains,
        PlainsEveningCloseMountains,
        Bomb,
        Player,
        GreenEnemy,
        GreenEnemy2,
        GreenEnemy3,
        ChompBoss,
        Bullet,
        StatusBar,
        Coins,
        DynamicBlocks,
        Max=DynamicBlocks
    }

    class PaletteModule : Module, IHBlankHandler
    {
        private int _paletteAddress;
        private readonly CoreGraphicsModule _graphicsModule;
        private readonly GameRAM _ram;
        private GameByte _timer;
        private SceneDefinition _currentScene;
        private TileModule _tileModule;
    
        public Palette BgPalette1 { get; private set; }
        public Palette BgPalette2 { get; private set; }

        private GameByte _bgColor;

        public byte BgColor
        {
            get => _bgColor.Value;
            set => _bgColor.Value = value;
        }

        public PaletteModule(MainSystem mainSystem,
            CoreGraphicsModule coreGraphicsModule,
            TileModule tileModule,
            GameRAM ram) 
            : base(mainSystem)
        {
            _tileModule = tileModule;
            _graphicsModule = coreGraphicsModule;
            _ram = ram;
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _paletteAddress = memoryBuilder.CurrentAddress;
            memoryBuilder.AddBytes(Specs.BytesPerPalette * ((int)PaletteKey.Max+1));

            BgPalette1 = new Palette(_graphicsModule.Specs, memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(_graphicsModule.Specs.BytesPerPalette);

            BgPalette2 = new Palette(_graphicsModule.Specs, memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(_graphicsModule.Specs.BytesPerPalette);

            _bgColor = memoryBuilder.AddByte();
        }

        public int GetPaletteAddress(PaletteKey key) => _paletteAddress + (int)key * Specs.BytesPerPalette;

        public Palette GetPalette(PaletteKey key)
        {
            return new Palette(Specs, GetPaletteAddress(key), GameSystem.Memory);
        }

        public void LoadPalette(PaletteKey key, Palette destination)
        {
            GameSystem.Memory.BlockCopy(GetPaletteAddress(key), destination.Address, Specs.BytesPerPalette);
        }

        public void LoadPalette(int sourceAddress, Palette destination)
        {
            GameSystem.Memory.BlockCopy(sourceAddress, destination.Address, Specs.BytesPerPalette);
        }

        public void FadeColor(Palette palette, Palette match, int index)
        {
            ColorIndex color = new ColorIndex(palette.GetColorIndex(index));
            ColorIndex target = new ColorIndex(match.GetColorIndex(index));

            if (color.Color != target.Color)
                palette.SetColor(index, new ColorIndex(target.Color, 0).Value);
            else if (color.ColorColumn < target.ColorColumn)
                palette.SetColor(index, color.Lighter().Value);
            else if(color.ColorColumn > target.ColorColumn)
                palette.SetColor(index, color.Darker().Value);
        }

        public override void OnStartup()
        {
            _timer = new GameByte(GameSystem.Memory.GetAddress(AddressLabels.MainTimer), GameSystem.Memory);

            DefinePalette(PaletteKey.Test,
                ColorIndex.Black,
                ColorIndex.Red1,
                ColorIndex.Green4,
                ColorIndex.Blue1);

            DefinePalette(PaletteKey.PlainsGround,
                ColorIndex.LightBlue,
                ColorIndex.Green1,
                ColorIndex.Green3,
                ColorIndex.Green4);

            DefinePalette(PaletteKey.PlainsNight,
               ColorIndex.Black,
               ColorIndex.Green1,
               ColorIndex.Green3,
               ColorIndex.Green4);

            DefinePalette(PaletteKey.PlainsFarMountains,
                ColorIndex.LightBlue,
                ColorIndex.Gray1,
                ColorIndex.Gray2,
                ColorIndex.Gray3);

            DefinePalette(PaletteKey.PlainsCloseMountains,
               ColorIndex.Gray3,
               ColorIndex.Green1,
               ColorIndex.BlueGray1,
               ColorIndex.BlueGray2);

            DefinePalette(PaletteKey.PlainsEveningFarMountains,
               ColorIndex.Blue1,
               ColorIndex.Green3,
               ColorIndex.Gray3,
               ColorIndex.Black);

            DefinePalette(PaletteKey.PlainsEveningCloseMountains,
             ColorIndex.Black,
             ColorIndex.Green1,
             ColorIndex.BlueGray2,
             ColorIndex.BlueGray1);

            DefinePalette(PaletteKey.Bomb,
                ColorIndex.Black,
                ColorIndex.Gray1,
                ColorIndex.Gray2);

            DefinePalette(PaletteKey.Player,
                ColorIndex.Orange,
                ColorIndex.DarkBrown,
                ColorIndex.LightTan);

            DefinePalette(PaletteKey.GreenEnemy,
                ColorIndex.Green1,
                ColorIndex.Green3,
                ColorIndex.Red3);

            DefinePalette(PaletteKey.GreenEnemy2,
              ColorIndex.Green3, // body
              ColorIndex.Green1, //outline
              ColorIndex.White); //teeth

            DefinePalette(PaletteKey.GreenEnemy3,
               ColorIndex.Green2,
               ColorIndex.Green1,
               ColorIndex.Red3);

            DefinePalette(PaletteKey.ChompBoss,
                ColorIndex.Red1,
                ColorIndex.Red3,
                ColorIndex.White);

            DefinePalette(PaletteKey.Bullet,
                ColorIndex.Red2,
                ColorIndex.Red3,
                ColorIndex.LightYellow);

            DefinePalette(PaletteKey.StatusBar,
                ColorIndex.Black,
                ColorIndex.Blue1,
                ColorIndex.White,
                ColorIndex.Green3);

            DefinePalette(PaletteKey.Coins,
              ColorIndex.Gold,
              ColorIndex.DarkBrown,
              ColorIndex.LightYellow);

            DefinePalette(PaletteKey.DynamicBlocks,
             ColorIndex.Black,
             ColorIndex.Gray2,
             ColorIndex.Gray1);
        }
        public void SetScene(SceneDefinition sceneDefinition, Level level, SystemMemory memory)
        {
            _currentScene = sceneDefinition;
            if (_currentScene == null)
                return;

            Theme levelTheme = new Theme(memory, sceneDefinition.Theme);
            var backgroundPalette = _graphicsModule.GetBackgroundPalette(0);
            var foregroundPalette = _graphicsModule.GetBackgroundPalette(1);
            var coinPalette = _graphicsModule.GetBackgroundPalette(2);
            var dynamicBlockPalette = _graphicsModule.GetBackgroundPalette(3);

            var bombPalette = _graphicsModule.GetSpritePalette(0);
            var playerPalette = _graphicsModule.GetSpritePalette(1);
            var enemy1Pallete = _graphicsModule.GetSpritePalette(2);
            var enemy2Pallete = _graphicsModule.GetSpritePalette(3);

            LoadPalette(levelTheme.Background2, backgroundPalette);
            _bgColor.Value = (byte)backgroundPalette.GetColorIndex(3);

            LoadPalette(levelTheme.Background1, backgroundPalette);
            LoadPalette(levelTheme.Foreground, foregroundPalette);
            LoadPalette(PaletteKey.Coins, coinPalette);
            LoadPalette(PaletteKey.DynamicBlocks, dynamicBlockPalette);

            LoadPalette(PaletteKey.Bomb, bombPalette);
            LoadPalette(PaletteKey.Player, playerPalette);
            LoadPalette(levelTheme.Enemy1, enemy1Pallete);
            LoadPalette(levelTheme.Enemy2, enemy2Pallete);

            LoadPalette(levelTheme.Background1, BgPalette1);
            LoadPalette(levelTheme.Background2, BgPalette2);

            dynamicBlockPalette.SetColor(0, _bgColor.Value);
            coinPalette.SetColor(0, _bgColor.Value);
            foregroundPalette.SetColor(0, _bgColor.Value);
        }

        //this needs a lot of work
        public void SetSceneX(SceneDefinition sceneDefinition, Level level)
        {
            //BG-TODO
            //_currentScene = sceneDefinition;
            //if (_currentScene == null)
            //    return;

            //var backgroundPalette = _graphicsModule.GetBackgroundPalette(0);
            //var foregroundPalette = _graphicsModule.GetBackgroundPalette(1);   
            //var coinPalette = _graphicsModule.GetBackgroundPalette(2);
            //var dynamicBlockPalette = _graphicsModule.GetBackgroundPalette(3);

            //var bombPalette = _graphicsModule.GetSpritePalette(0);
            //var playerPalette = _graphicsModule.GetSpritePalette(1);
            //var enemyPallete = _graphicsModule.GetSpritePalette(2);
            //var bulletPallete = _graphicsModule.GetSpritePalette(3);

            //LoadPalette(PaletteKey.Coins, coinPalette);
            //LoadPalette(PaletteKey.DynamicBlocks, dynamicBlockPalette);

            //bool customPalette = false;

            //switch (level)
            //{
            //    case Level.Level1_17_Boss:

            //        LoadPalette(PaletteKey.PlainsGround, foregroundPalette);
            //        LoadPalette(PaletteKey.PlainsSky, backgroundPalette);
            //        _bgPalette1.Value = PaletteKey.PlainsNight;
            //        _bgPalette2.Value = PaletteKey.PlainsNight;


            //        customPalette = true;
            //        break;
            //}

            //if (!customPalette)
            //{
            //    switch (sceneDefinition.Theme)
            //    {
            //        case ThemeType.Plains:

            //            LoadPalette(PaletteKey.PlainsGround, foregroundPalette);
            //            LoadPalette(PaletteKey.PlainsSky, backgroundPalette);
            //            _bgPalette1.Value = PaletteKey.PlainsSky;

            //            if (sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
            //                foregroundPalette.SetColor(0, ColorIndex.BlueGray2);
            //            else if (sceneDefinition.GetBgPosition1() == 0)
            //                foregroundPalette.SetColor(0, ColorIndex.LightBlue);
            //            else
            //                foregroundPalette.SetColor(0, ColorIndex.Gray3);

            //            dynamicBlockPalette.SetColor(0, (byte)foregroundPalette.GetColorIndex(0));
            //            coinPalette.SetColor(0, (byte)foregroundPalette.GetColorIndex(0));

            //            if (sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
            //                _bgPalette2.Value = PaletteKey.PlainsSky2;

            //            break;

            //        case ThemeType.PlainsEvening:

            //            LoadPalette(PaletteKey.PlainsGround, foregroundPalette);
            //            LoadPalette(PaletteKey.PlainsSky, backgroundPalette);
            //            _bgPalette1.Value = PaletteKey.PlainsSky3;

            //            if (sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
            //                foregroundPalette.SetColor(0, ColorIndex.BlueGray1);
            //            else if (sceneDefinition.GetBgPosition1() == 0)
            //                foregroundPalette.SetColor(0, ColorIndex.Green1);
            //            else
            //                foregroundPalette.SetColor(0, ColorIndex.Blue4);

            //            dynamicBlockPalette.SetColor(0, (byte)foregroundPalette.GetColorIndex(0));
            //            coinPalette.SetColor(0, (byte)foregroundPalette.GetColorIndex(0));

            //            if (sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
            //                _bgPalette2.Value = PaletteKey.PlainsSky4;

            //            break;

            //    }               
            //}

            //LoadPalette(PaletteKey.Bomb, bombPalette);
            //LoadPalette(PaletteKey.Player, playerPalette);
            //LoadPalette(PaletteKey.Bullet, bulletPallete);

            ////todo, need a better way to define enemy palettes
            //if (_currentScene.HasSprite(SpriteLoadFlags.Boss))
            //    LoadPalette(PaletteKey.ChompBoss, enemyPallete);
            //else
            //    LoadPalette(PaletteKey.GreenEnemy, enemyPallete);

            //LoadPalette(PaletteKey.Bullet, bulletPallete);
        }

        private void DefinePalette(PaletteKey key, byte color1, byte color2, byte color3, byte color4)
        {
            var palette = new Palette(Specs, _paletteAddress + ((int)key * Specs.BytesPerPalette), GameSystem.Memory);
            palette.SetColor(0, color1);
            palette.SetColor(1, color2);
            palette.SetColor(2, color3);
            palette.SetColor(3, color4);
        }
        private void DefinePalette(PaletteKey key, byte color2, byte color3, byte color4)
        {
            DefinePalette(key,0, color2, color3, color4);
        }

        public void OnHBlank()
        {
            if (_currentScene == null)
                return;

            if(_currentScene.IsLevelBossScene)
            {
                OnHBlank_LevelBoss();
                return;
            }

            int back2Pixel = _currentScene.GetBackgroundLayerPixel(BackgroundLayer.Back2, includeStatusBar: true) - _tileModule.Scroll.Y;

            if (_graphicsModule.ScreenPoint.Y == 0)
            {
                LoadPalette(PaletteKey.StatusBar, _graphicsModule.GetBackgroundPalette(0));
            }
            else if (_graphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                if(_graphicsModule.ScreenPoint.Y >= back2Pixel)
                    LoadPalette(BgPalette2.Address, _graphicsModule.GetBackgroundPalette(0));
                else 
                    LoadPalette(BgPalette1.Address, _graphicsModule.GetBackgroundPalette(0));
            }
            else if(_graphicsModule.ScreenPoint.Y == back2Pixel)
            {
                LoadPalette(BgPalette2.Address, _graphicsModule.GetBackgroundPalette(0));
            }
            else if (_graphicsModule.ScreenPoint.Y == _currentScene.GetBackgroundLayerPixel(BackgroundLayer.ForegroundStart, includeStatusBar: true))
            {
                _graphicsModule.GetBackgroundPalette(0).SetColor(0, _bgColor.Value);
            }
        }

        private void OnHBlank_LevelBoss()
        {
            if (_graphicsModule.ScreenPoint.Y == 0)
            {
                LoadPalette(PaletteKey.StatusBar, _graphicsModule.GetBackgroundPalette(0));
            }
            else if (_graphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                LoadPalette(BgPalette2.Address, _graphicsModule.GetBackgroundPalette(0));
                _graphicsModule.GetBackgroundPalette(0).SetColor(0, _bgColor.Value);
            }
        }

        public void CyclePalette(Palette p)
        {
            var c1 = p.GetColorIndex(1);
            p.SetColor(1,(byte)p.GetColorIndex(2));
            p.SetColor(2,(byte)p.GetColorIndex(3));
            p.SetColor(3,(byte)c1);
        }

        public void CyclePalette2(Palette p)
        {
            byte c1 = (byte)p.GetColorIndex(1);
            p.SetColor(1, (byte)p.GetColorIndex(3));
            p.SetColor(3, c1);
        }

        public void Update()
        {
            if (_timer.Value % 8 == 0)
            {
                var bulletPallete = GameSystem.CoreGraphicsModule.GetSpritePalette(3);
                CyclePalette(bulletPallete);

                var coinPallete = GameSystem.CoreGraphicsModule.GetBackgroundPalette(2);
                CyclePalette2(coinPallete);
            }
        }
    }
}
