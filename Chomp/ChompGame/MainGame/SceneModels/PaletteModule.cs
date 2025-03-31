using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using System.Data;

namespace ChompGame.MainGame.SceneModels
{
    public enum SpritePalette :byte
    {
        Fire=0,
        Player=1,
        Enemy1=2,
        Enemy2=3,
        Platform=3
    }

    public enum BgPalette : byte
    {
        Background=0,
        Foreground=1,
        Coin=2,
        DynamicBlock=3
    }

    public enum PaletteChange : byte
    {
        None,
        Bg1,
        Bg2,
        UpdateColor0
    }

    public enum PaletteKey : byte
    {
        Test,
        PlainsGround,
        PlainsNight,
        PlainsFarMountains,
        PlainsCloseMountains,
        PlainsEveningFarMountains,
        PlainsEveningCloseMountains,
        OceanSky,
        Gray,
        BombLight,
        Player,
        GreenEnemy,
        GreenEnemy2,
        GreenEnemy3,
        BlueEnemy,
        BlueEnemy2,
        LightBlueEnemy,
        BlueGrayEnemy,
        ChompBoss,
        Bullet,
        StatusBar,
        Coins,
        DynamicBlocks,
        Water,
        Sand,
        CitySky,
        CityBg,
        CityFg,
        CityInterior,
        CitySkyEvening,
        CityBgEvening,
        CityFgEvening,
        PurpleSky,
        BrownBrick,
        PyramidBg,
        PyramidTorches,
        DesertNight,
        DesertRain,
        ForestGround,
        ForestTreesNear,
        ForestTreesFar,
        NightMist,
        TechBaseBg,
        TechBaseFg,
        TechBaseBg2,
        TechBaseFg2,
        Max = TechBaseFg2
    }

    class PaletteModule : Module, IHBlankHandler
    {
        private int _paletteAddress;
        private readonly CoreGraphicsModule _graphicsModule;
        private GameByte _timer;
        private SceneDefinition _currentScene;
        private TileModule _tileModule;
    
        public Palette BgPalette1 { get; private set; }
        public Palette BgPalette2 { get; private set; }

        private GameByte _bgColor;
        private DenseTwoBitArray _paletteChangeTable;

        public byte BgColor
        {
            get => _bgColor.Value;
            set
            {
                _bgColor.Value = value; 
                var foregroundPalette = _graphicsModule.GetBackgroundPalette(BgPalette.Foreground);
                var coinPalette = _graphicsModule.GetBackgroundPalette(BgPalette.Coin);
                var dynamicBlockPalette = _graphicsModule.GetBackgroundPalette(BgPalette.DynamicBlock);
                dynamicBlockPalette.SetColor(0, _bgColor.Value);
                coinPalette.SetColor(0, _bgColor.Value);
                foregroundPalette.SetColor(0, _bgColor.Value);
            }
        }

        public PaletteModule(MainSystem mainSystem,
            CoreGraphicsModule coreGraphicsModule,
            TileModule tileModule,
            GameRAM ram) 
            : base(mainSystem)
        {
            _tileModule = tileModule;
            _graphicsModule = coreGraphicsModule;
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

            _paletteChangeTable = new DenseTwoBitArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddByte();
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

        public bool Darken(Palette palette, int index, int limit=0)
        {
            ColorIndex color = new ColorIndex(palette.GetColorIndex(index));
            if (color.ColorColumn <= limit)
                return true;
            palette.SetColor(index, color.Darker().Value);
            return false;
        }

        public bool Lighten(Palette palette, int index)
        {
            ColorIndex color = new ColorIndex(palette.GetColorIndex(index));
            if (color.ColorColumn == 7)
                return true;

            color = color.Lighter();
            palette.SetColor(index, color.Value);
            return false;
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


            DefinePalette(PaletteKey.OceanSky,
             ColorIndex.Blue6,
             ColorIndex.Green1,
             ColorIndex.BlueGray3,
             ColorIndex.BlueGray4);
            
            DefinePalette(PaletteKey.Gray,
                ColorIndex.Black,
                ColorIndex.Gray1,
                ColorIndex.Yellow5);

            DefinePalette(PaletteKey.BombLight,
              ColorIndex.Gray1,
              ColorIndex.Gray2,
              ColorIndex.Yellow5);

            DefinePalette(PaletteKey.Player,
                ColorIndex.Orange,
                ColorIndex.DarkBrown,
                ColorIndex.LightTan);

            DefinePalette(PaletteKey.GreenEnemy,
                ColorIndex.Green1,
                ColorIndex.Green3,
                ColorIndex.Red3);

            DefinePalette(PaletteKey.BlueEnemy,
                ColorIndex.Blue4,
                ColorIndex.Blue5,
                ColorIndex.Yellow5);

            DefinePalette(PaletteKey.BlueEnemy2,
               ColorIndex.Black,
               ColorIndex.Blue1, //fill
               ColorIndex.Blue3, //outline
               ColorIndex.Green1);

            DefinePalette(PaletteKey.LightBlueEnemy,
                ColorIndex.Black,
                ColorIndex.Blue4, //fill
                ColorIndex.Orange, 
                ColorIndex.Yellow5); //eye


            DefinePalette(PaletteKey.BlueGrayEnemy,
                ColorIndex.BlueGray2,
                ColorIndex.BlueGray3,
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
                ColorIndex.Yellow5);

            DefinePalette(PaletteKey.StatusBar,
                ColorIndex.Black,
                ColorIndex.Blue1,
                ColorIndex.White,
                ColorIndex.Green3);

            DefinePalette(PaletteKey.Coins,
              ColorIndex.Gold,
              ColorIndex.DarkBrown,
              ColorIndex.Yellow5);

            DefinePalette(PaletteKey.DynamicBlocks,
             ColorIndex.Black,
             ColorIndex.Gray2,
             ColorIndex.Gray1);

            DefinePalette(PaletteKey.Water,
               ColorIndex.Blue2,
               ColorIndex.Blue3,
               ColorIndex.Blue4,
               ColorIndex.Blue1);

            DefinePalette(PaletteKey.Sand,
                ColorIndex.Red1,
                ColorIndex.Brown5,
                ColorIndex.Brown6,
                ColorIndex.Brown7);


            DefinePalette(PaletteKey.CitySky,
                ColorIndex.Orange,
                ColorIndex.Brown5,
                ColorIndex.Brown6,
                ColorIndex.Brown7);

            DefinePalette(PaletteKey.CityBg,
              ColorIndex.White,
              ColorIndex.Brown5,
              ColorIndex.Brown2,
              ColorIndex.Brown2);

            DefinePalette(PaletteKey.CityFg,
             ColorIndex.White,
             ColorIndex.BlueGray1,
             ColorIndex.BlueGray2,
             ColorIndex.BlueGray3);

            DefinePalette(PaletteKey.CityInterior,
            ColorIndex.DarkGray2,
            ColorIndex.Red2,
            ColorIndex.Green2,
            ColorIndex.DarkGray1);

            DefinePalette(PaletteKey.CitySkyEvening,
               ColorIndex.Purple1,
               ColorIndex.Purple2,
               ColorIndex.Brown6,
               ColorIndex.Brown7);

            DefinePalette(PaletteKey.CityBgEvening,
              ColorIndex.White,
              ColorIndex.Purple2,
              ColorIndex.Black,
              ColorIndex.Black);

            DefinePalette(PaletteKey.CityFgEvening,
             ColorIndex.White,
             ColorIndex.DarkGray(4).Value,
             ColorIndex.DarkGray(5).Value,
             ColorIndex.DarkGray(7).Value);

            DefinePalette(PaletteKey.PurpleSky,
                ColorIndex.Purple1,
                ColorIndex.Brown5,
                ColorIndex.Brown6,
                ColorIndex.Red1);

            DefinePalette(PaletteKey.BrownBrick,
              ColorIndex.Brown1,
              ColorIndex.Brown2,
              ColorIndex.Brown3,
              ColorIndex.Brown4);

            DefinePalette(PaletteKey.PyramidBg,
                ColorIndex.Red1,
                ColorIndex.Red1,
                ColorIndex.Brown1,
                ColorIndex.Brown2);

            DefinePalette(PaletteKey.DesertNight,
               ColorIndex.Black,
               ColorIndex.Purple1,
               ColorIndex.Purple2,
               ColorIndex.Purple3);


            DefinePalette(PaletteKey.PyramidTorches,
                ColorIndex.Red1,
                ColorIndex.DarkGray1,
                ColorIndex.Yellow1,
                ColorIndex.Yellow4);

            DefinePalette(PaletteKey.DesertRain,
               ColorIndex.Black,
               ColorIndex.Black,
               ColorIndex.Blue1,
               ColorIndex.Blue3);

            DefinePalette(PaletteKey.ForestGround,
                ColorIndex.Red1,
                ColorIndex.Brown1,
                ColorIndex.Brown2,
                ColorIndex.Brown3);


            DefinePalette(PaletteKey.ForestTreesNear,
                ColorIndex.Green2,
                ColorIndex.Green1, //outline
                ColorIndex.Green3, //fill
                ColorIndex.Green2);

            DefinePalette(PaletteKey.ForestTreesFar,
               ColorIndex.Black,
               ColorIndex.Green1,//outline
               ColorIndex.Green2, //fill
               ColorIndex.Green3);

            DefinePalette(PaletteKey.NightMist,
              ColorIndex.Black,
              ColorIndex.Gray1,//outline
              ColorIndex.Gray2, //fill
              ColorIndex.Gray3);

            DefinePalette(PaletteKey.TechBaseBg,
            ColorIndex.Black,
            ColorIndex.Purple1,
            ColorIndex.Blue1, 
            ColorIndex.Blue2);

            DefinePalette(PaletteKey.TechBaseFg,
             ColorIndex.Black,
             ColorIndex.BlueGray3,
             ColorIndex.BlueGray1, 
             ColorIndex.Gray2);

            DefinePalette(PaletteKey.TechBaseBg2,
           ColorIndex.Black,
           ColorIndex.Purple1,
           ColorIndex.Brown4,
           ColorIndex.Brown5);

            DefinePalette(PaletteKey.TechBaseFg2,
             ColorIndex.Black,
             ColorIndex.Red1,
             ColorIndex.Brown1,
             ColorIndex.Red2);

        }
        public void SetScene(SceneDefinition sceneDefinition, Level level, SystemMemory memory)
        {
            _currentScene = sceneDefinition;
            if (_currentScene == null)
                return;

            Theme levelTheme = new Theme(memory, sceneDefinition.Theme);
            var backgroundPalette = _graphicsModule.GetBackgroundPalette(BgPalette.Background);
            var foregroundPalette = _graphicsModule.GetBackgroundPalette(BgPalette.Foreground);
            var coinPalette = _graphicsModule.GetBackgroundPalette(BgPalette.Coin);
            var dynamicBlockPalette = _graphicsModule.GetBackgroundPalette(BgPalette.DynamicBlock);

            var bombPalette = _graphicsModule.GetSpritePalette(SpritePalette.Fire);
            var playerPalette = _graphicsModule.GetSpritePalette(SpritePalette.Player);
            var enemy1Pallete = _graphicsModule.GetSpritePalette(SpritePalette.Enemy1);
            var enemy2Pallete = _graphicsModule.GetSpritePalette(SpritePalette.Enemy2);

            LoadPalette(levelTheme.Background2, backgroundPalette);

            if (_currentScene.Theme == ThemeType.DesertInterior || _currentScene.Theme == ThemeType.Forest || _currentScene.Theme == ThemeType.Mist)
                _bgColor.Value = (byte)backgroundPalette.GetColorIndex(0);
            else if (_currentScene.Theme == ThemeType.TechBase || _currentScene.Theme == ThemeType.TechBase2)
            {
                if(_currentScene.ScrollStyle == ScrollStyle.NameTable || _currentScene.ScrollStyle == ScrollStyle.Vertical)
                    _bgColor.Value = (byte)backgroundPalette.GetColorIndex(0);
                else
                    _bgColor.Value = (byte)backgroundPalette.GetColorIndex(2);
            }
            else
                _bgColor.Value = (byte)backgroundPalette.GetColorIndex(3);

            LoadPalette(levelTheme.Background1, backgroundPalette);
            LoadPalette(levelTheme.Foreground1, foregroundPalette);
            LoadPalette(PaletteKey.Coins, coinPalette);
            LoadPalette(levelTheme.Foreground2, dynamicBlockPalette);

            LoadPalette(levelTheme.Bomb, bombPalette);
            LoadPalette(PaletteKey.Player, playerPalette);
            LoadPalette(levelTheme.Enemy1, enemy1Pallete);
            LoadPalette(levelTheme.Enemy2, enemy2Pallete);

            LoadPalette(levelTheme.Background1, BgPalette1);
            LoadPalette(levelTheme.Background2, BgPalette2);

            dynamicBlockPalette.SetColor(0, _bgColor.Value);
            coinPalette.SetColor(0, _bgColor.Value);
            foregroundPalette.SetColor(0, _bgColor.Value);

            FillPaletteChangeTable();
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

        public void FillPaletteChangeTable()
        {
            if(_currentScene.IsLevelBossScene)
            {
                _paletteChangeTable[(byte)BackgroundPart.Top] = (byte)PaletteChange.None;
                _paletteChangeTable[(byte)BackgroundPart.Upper] = (byte)PaletteChange.None;
                _paletteChangeTable[(byte)BackgroundPart.Middle] = (byte)PaletteChange.None;
                _paletteChangeTable[(byte)BackgroundPart.Lower] = (byte)PaletteChange.None;
            }
            else if(_currentScene.Theme == ThemeType.Desert || _currentScene.Theme == ThemeType.GlitchCore)
            {
                _paletteChangeTable[(byte)BackgroundPart.Top] = (byte)PaletteChange.Bg2;
                _paletteChangeTable[(byte)BackgroundPart.Upper] = (byte)PaletteChange.Bg2;
                _paletteChangeTable[(byte)BackgroundPart.Middle] = (byte)PaletteChange.Bg2;
                _paletteChangeTable[(byte)BackgroundPart.Lower] = (byte)PaletteChange.None;
            }
            else if (_currentScene.Theme == ThemeType.Forest)
            {
                _paletteChangeTable[(byte)BackgroundPart.Top] = (byte)PaletteChange.Bg1;
                _paletteChangeTable[(byte)BackgroundPart.Upper] = (byte)PaletteChange.Bg2;
                _paletteChangeTable[(byte)BackgroundPart.Middle] = (byte)PaletteChange.None;
                _paletteChangeTable[(byte)BackgroundPart.Lower] = (byte)PaletteChange.None;
            }
            else if (_currentScene.Theme == ThemeType.DesertNight)
            {
                _paletteChangeTable[(byte)BackgroundPart.Top] = (byte)PaletteChange.Bg1;
                _paletteChangeTable[(byte)BackgroundPart.Upper] = (byte)PaletteChange.Bg1;
                _paletteChangeTable[(byte)BackgroundPart.Middle] = (byte)PaletteChange.Bg1;
                _paletteChangeTable[(byte)BackgroundPart.Lower] = (byte)PaletteChange.Bg2;
            }
            else if (_currentScene.Theme == ThemeType.DesertRain)
            {
                _paletteChangeTable[(byte)BackgroundPart.Top] = (byte)PaletteChange.Bg1;
                _paletteChangeTable[(byte)BackgroundPart.Upper] = (byte)PaletteChange.Bg1;
                _paletteChangeTable[(byte)BackgroundPart.Middle] = (byte)PaletteChange.Bg1;
                _paletteChangeTable[(byte)BackgroundPart.Lower] = (byte)PaletteChange.Bg2;
            }
            else if (_currentScene.Theme == ThemeType.TechBase || _currentScene.Theme == ThemeType.TechBase2)
            {
                if (_currentScene.ScrollStyle == ScrollStyle.Vertical || _currentScene.ScrollStyle == ScrollStyle.NameTable)
                {
                    _paletteChangeTable[(byte)BackgroundPart.Top] = (byte)PaletteChange.Bg2;
                    _paletteChangeTable[(byte)BackgroundPart.Upper] = (byte)PaletteChange.Bg2;
                    _paletteChangeTable[(byte)BackgroundPart.Middle] = (byte)PaletteChange.Bg2;
                    _paletteChangeTable[(byte)BackgroundPart.Lower] = (byte)PaletteChange.Bg2;
                }
                else
                {
                    _paletteChangeTable[(byte)BackgroundPart.Top] = (byte)PaletteChange.Bg2;
                    _paletteChangeTable[(byte)BackgroundPart.Upper] = (byte)PaletteChange.Bg2;
                    _paletteChangeTable[(byte)BackgroundPart.Middle] = (byte)PaletteChange.Bg1;
                    _paletteChangeTable[(byte)BackgroundPart.Lower] = (byte)PaletteChange.Bg2;
                }
            }
            else if (_currentScene.ScrollStyle == ScrollStyle.Horizontal &&
                _currentScene.HorizontalScrollStyle == HorizontalScrollStyle.Interior)
            {
                _paletteChangeTable[(byte)BackgroundPart.Top] = (byte)PaletteChange.Bg1;
                _paletteChangeTable[(byte)BackgroundPart.Upper] = (byte)PaletteChange.Bg2;
                _paletteChangeTable[(byte)BackgroundPart.Middle] = (byte)PaletteChange.Bg1;
                _paletteChangeTable[(byte)BackgroundPart.Lower] = (byte)PaletteChange.Bg1;
            }
            else if(_currentScene.Theme == ThemeType.DesertInterior)
            {
                _paletteChangeTable[(byte)BackgroundPart.Top] = (byte)PaletteChange.Bg2;
                _paletteChangeTable[(byte)BackgroundPart.Upper] = (byte)PaletteChange.Bg2;
                _paletteChangeTable[(byte)BackgroundPart.Middle] = (byte)PaletteChange.Bg2;
                _paletteChangeTable[(byte)BackgroundPart.Lower] = (byte)PaletteChange.None;
            }            
            else
            {
                _paletteChangeTable[(byte)BackgroundPart.Top] = (byte)PaletteChange.Bg1;
                _paletteChangeTable[(byte)BackgroundPart.Upper] = (byte)PaletteChange.None;
                _paletteChangeTable[(byte)BackgroundPart.Middle] = (byte)PaletteChange.Bg2;
                _paletteChangeTable[(byte)BackgroundPart.Lower] = (byte)PaletteChange.UpdateColor0;
            }
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
            else if(_currentScene.IsAutoScroll)
            {
                OnHBlank_AutoScroll();
                return;
            }
            else if(_currentScene.Theme == ThemeType.GlitchCore)
            {
                OnHBlank_GlitchCore();
                return;
            }

            if (_graphicsModule.ScreenPoint.Y == 0)
            {
                LoadPalette(PaletteKey.StatusBar, _graphicsModule.GetBackgroundPalette(0));
            }
            else if (_graphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                int back2Pixel = _currentScene.GetBackgroundLayerPixel(BackgroundPart.Middle, includeStatusBar: true) - _tileModule.Scroll.Y;
                if (_graphicsModule.ScreenPoint.Y >= back2Pixel)
                    ApplyPaletteChange((PaletteChange)_paletteChangeTable[(byte)BackgroundPart.Middle]);
                else
                    ApplyPaletteChange((PaletteChange)_paletteChangeTable[(byte)BackgroundPart.Top]);
            }
            else
            {
                CheckPaletteChange(0);
                CheckPaletteChange(1);
                CheckPaletteChange(2);
                CheckPaletteChange(3);
            }
        }

        private void OnHBlank_GlitchCore()
        {
            if (_graphicsModule.ScreenPoint.Y == 0)
            {
                LoadPalette(PaletteKey.StatusBar, _graphicsModule.GetBackgroundPalette(0));
            }
            else if (_graphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                ApplyPaletteChange((PaletteChange)_paletteChangeTable[(byte)BackgroundPart.Top]);
            }
            else if (_graphicsModule.ScreenPoint.Y > Constants.StatusBarHeight)
            {
                int startY = (_timer.Value % 64) / 4;
                int relY = _graphicsModule.ScreenPoint.Y - startY;

                if (relY.IsMod(4))
                {
                    var p = _graphicsModule.GetBackgroundPalette(BgPalette.Foreground);
                    if (relY.IsMod(24))
                    {
                        LoadPalette(BgPalette1.Address, _graphicsModule.GetBackgroundPalette(BgPalette.Foreground));
                    }
                    else
                    {
                        Darken(p, 0);
                        Darken(p, 1);
                        Darken(p, 2);
                        Darken(p, 3);
                    }
                }
            }
        }

        private void CheckPaletteChange(int index)
        {
            var pixelY = _currentScene.GetBackgroundLayerPixel((BackgroundPart)index, true) - _tileModule.Scroll.Y;
            if (_graphicsModule.ScreenPoint.Y != pixelY)
                return;

            ApplyPaletteChange((PaletteChange)_paletteChangeTable[index]);           
        }

        private void ApplyPaletteChange(PaletteChange p)
        {
            switch (p)
            {
                case PaletteChange.Bg1:
                    LoadPalette(BgPalette1.Address, _graphicsModule.GetBackgroundPalette(0));
                    return;
                case PaletteChange.Bg2:
                    LoadPalette(BgPalette2.Address, _graphicsModule.GetBackgroundPalette(0));
                    return;
                case PaletteChange.UpdateColor0:
                    _graphicsModule.GetBackgroundPalette(0).SetColor(0, _bgColor.Value);
                    return;
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

        private void OnHBlank_AutoScroll()
        {
            int back2Pixel = _currentScene.GetBackgroundLayerPixel(BackgroundPart.Middle, includeStatusBar: true);

            if (_graphicsModule.ScreenPoint.Y == 0)
            {
                LoadPalette(PaletteKey.StatusBar, _graphicsModule.GetBackgroundPalette(0));
            }
            else if (_graphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                LoadPalette(BgPalette1.Address, _graphicsModule.GetBackgroundPalette(0));
            }
            else if (_graphicsModule.ScreenPoint.Y == back2Pixel)
            {
                LoadPalette(BgPalette2.Address, _graphicsModule.GetBackgroundPalette(0));
            }
        }

        public void CyclePalette(Palette p)
        {
            var c1 = p.GetColorIndex(1);
            p.SetColor(1,(byte)p.GetColorIndex(2));
            p.SetColor(2,(byte)p.GetColorIndex(3));
            p.SetColor(3,(byte)c1);
        }

        public void CyclePalette3(Palette p)
        {
            var c3 = p.GetColorIndex(3);

            p.SetColor(3, (byte)p.GetColorIndex(2));
            p.SetColor(2, (byte)p.GetColorIndex(1));
            p.SetColor(1, (byte)c3);
        }

        public void CyclePalette2(Palette p)
        {
            byte c1 = (byte)p.GetColorIndex(1);
            p.SetColor(1, (byte)p.GetColorIndex(3));
            p.SetColor(3, c1);
        }

        public void CycleFirePalette(Palette p)
        {
            byte c1 = (byte)p.GetColorIndex(2);

            if(c1 == ColorIndex.Red1)
            {
                p.SetColor(2, ColorIndex.Yellow5);
                p.SetColor(3, ColorIndex.Red2);
            }
            else if (c1 == ColorIndex.Yellow5)
            {
                p.SetColor(2, ColorIndex.White);
                p.SetColor(3, ColorIndex.Red1);
            }
            else if (c1 == ColorIndex.White)
            {
                p.SetColor(2, ColorIndex.Red2);
                p.SetColor(3, ColorIndex.Yellow5);
            }
            else
            {
                p.SetColor(2, ColorIndex.Red1);
                p.SetColor(3, ColorIndex.White);
            }
        }

        public void Update()
        {
            if (_timer.Value % 2 == 0)
            {
                if (_currentScene.Theme == ThemeType.DesertRain)
                {
                    CyclePalette3(BgPalette1);
                }

            }

            if (_timer.Value % 8 == 0)
            {
                var bulletPallete = GameSystem.CoreGraphicsModule.GetSpritePalette(SpritePalette.Fire);
                CycleFirePalette(bulletPallete);

                var coinPallete = GameSystem.CoreGraphicsModule.GetBackgroundPalette(BgPalette.Coin);
                CyclePalette2(coinPallete);

                if (_currentScene.Theme == ThemeType.DesertInterior)
                {
                    if (Lighten(BgPalette2, 2))
                        BgPalette2.SetColor(2, ColorIndex.Yellow1);

                    if (Darken(BgPalette2, 3, 4))
                        BgPalette2.SetColor(3, ColorIndex.Yellow5);
                }
            }

            if (_timer.Value % 16 == 0)
            {
                if (_currentScene.Theme == ThemeType.Mist)
                {
                    if (_timer.Value < 128)
                    {
                        Lighten(BgPalette2, 2);
                        Darken(BgPalette1, 2, limit: 5);
                    }
                    else
                    {
                        Darken(BgPalette2, 2, limit: 3);
                        Lighten(BgPalette1, 2);
                    }

                    if (Lighten(BgPalette1, 3))
                        BgPalette1.SetColor(3, ColorIndex.Gray1);
                }
                else if (_currentScene.Theme == ThemeType.TechBase || _currentScene.Theme == ThemeType.TechBase2)
                {
                    if (_timer.Value < 128)
                        Lighten(BgPalette1, 3);
                    else
                        Darken(BgPalette1, 3);

                }
            }
        
        
        
            if(_currentScene.Theme == ThemeType.GlitchCore)
            {
                LoadPalette(BgPalette1.Address, _graphicsModule.GetBackgroundPalette(BgPalette.Foreground));
            }
        }
    }
}
