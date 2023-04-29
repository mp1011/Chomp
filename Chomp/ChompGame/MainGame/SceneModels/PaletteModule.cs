using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.Graphics;

namespace ChompGame.MainGame.SceneModels
{
    //Max 16
    public enum PaletteKey : byte
    {
        PlainsGround,
        PlainsSky,
        PlainsSky2,
        Bomb,
        Player,
        GreenEnemy,
        Bullet,
        StatusBar,
        Coins,
        DynamicBlocks
    }

    class PaletteModule : Module, IHBlankHandler
    {
        private int _paletteAddress;
        private readonly CoreGraphicsModule _graphicsModule;
        private readonly GameRAM _ram;
        private GameByte _timer;
        private SceneDefinition _currentScene;
        private Specs Specs => _graphicsModule.Specs;

        private GameByteEnum<PaletteKey> _bgPalette1;
        private GameByteEnum<PaletteKey> _bgPalette2;
        

        public PaletteModule(MainSystem mainSystem,
            CoreGraphicsModule coreGraphicsModule,
            GameRAM ram) 
            : base(mainSystem)
        {
            _graphicsModule = coreGraphicsModule;
            _ram = ram;
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _paletteAddress = memoryBuilder.CurrentAddress;
            memoryBuilder.AddBytes(Specs.BytesPerPalette * 16); // todo - how many total palettes will we have?

            _bgPalette1 = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _bgPalette2 = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
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

        public override void OnStartup()
        {
            _timer = new GameByte(GameSystem.Memory.GetAddress(AddressLabels.MainTimer), GameSystem.Memory);

            DefinePalette(PaletteKey.PlainsGround,
                ColorIndex.LightBlue,
                ColorIndex.Green1,
                ColorIndex.Green2,
                ColorIndex.Green3);

            DefinePalette(PaletteKey.PlainsSky,
                ColorIndex.LightBlue,
                ColorIndex.Gray1,
                ColorIndex.Gray2,
                ColorIndex.Gray3);

            DefinePalette(PaletteKey.PlainsSky2,
               ColorIndex.Gray3,
               ColorIndex.Green1,
               ColorIndex.BlueGray1,
               ColorIndex.BlueGray2);

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
                ColorIndex.Green2,
                ColorIndex.Red3);

            DefinePalette(PaletteKey.Bullet,
                ColorIndex.Red2,
                ColorIndex.Red3,
                ColorIndex.LightYellow);

            DefinePalette(PaletteKey.StatusBar,
                ColorIndex.Black,
                ColorIndex.Blue1,
                ColorIndex.White,
                ColorIndex.Green2);

            DefinePalette(PaletteKey.Coins,
              ColorIndex.Gold,
              ColorIndex.DarkBrown,
              ColorIndex.LightYellow);

            DefinePalette(PaletteKey.DynamicBlocks,
             ColorIndex.Black,
             ColorIndex.Gray2,
             ColorIndex.Gray1);
        }

        public void SetScene(SceneDefinition sceneDefinition)
        {
            _currentScene = sceneDefinition;

            var backgroundPalette = _graphicsModule.GetBackgroundPalette(0);
            var foregroundPalette = _graphicsModule.GetBackgroundPalette(1);   
            var coinPalette = _graphicsModule.GetBackgroundPalette(2);
            var dynamicBlockPalette = _graphicsModule.GetBackgroundPalette(3);

            var bombPalette = _graphicsModule.GetSpritePalette(0);
            var playerPalette = _graphicsModule.GetSpritePalette(1);
            var enemyPallete = _graphicsModule.GetSpritePalette(2);
            var bulletPallete = _graphicsModule.GetSpritePalette(3);

            switch (sceneDefinition.Theme)
            {
                case Theme.Plains:

                    LoadPalette(PaletteKey.Coins, coinPalette);
                    LoadPalette(PaletteKey.DynamicBlocks, dynamicBlockPalette);

                    LoadPalette(PaletteKey.PlainsGround, foregroundPalette);                   
                    LoadPalette(PaletteKey.PlainsSky, backgroundPalette);
                    _bgPalette1.Value = PaletteKey.PlainsSky;

                    if (sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
                        foregroundPalette.SetColor(0, ColorIndex.BlueGray2);
                    else if (sceneDefinition.GetBgPosition1() == 0)
                        foregroundPalette.SetColor(0, ColorIndex.LightBlue);
                    else
                        foregroundPalette.SetColor(0, ColorIndex.Gray3);

                    dynamicBlockPalette.SetColor(0, (byte)foregroundPalette.GetColorIndex(0));
                    coinPalette.SetColor(0, (byte)foregroundPalette.GetColorIndex(0));

                    if (sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
                        _bgPalette2.Value = PaletteKey.PlainsSky2;
 
                    break;
            }

            LoadPalette(PaletteKey.Bomb, bombPalette);
            LoadPalette(PaletteKey.Player, playerPalette);
            LoadPalette(PaletteKey.Bullet, bulletPallete);
            LoadPalette(PaletteKey.GreenEnemy, enemyPallete);
            LoadPalette(PaletteKey.Bullet, bulletPallete);
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

            if(_graphicsModule.ScreenPoint.Y == 0)
            {
                LoadPalette(PaletteKey.StatusBar, _graphicsModule.GetBackgroundPalette(0));
            }
            else if (_graphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                LoadPalette(_bgPalette1.Value, _graphicsModule.GetBackgroundPalette(0));
            }
            else if(_currentScene.ScrollStyle == ScrollStyle.Horizontal 
                && _graphicsModule.ScreenPoint.Y == _currentScene.GetParallaxLayerPixel(ParallaxLayer.Back2, includeStatusBar: true))
            {
                LoadPalette(_bgPalette2.Value, _graphicsModule.GetBackgroundPalette(0));
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
