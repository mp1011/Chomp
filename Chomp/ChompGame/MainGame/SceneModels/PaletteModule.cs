using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels
{
    //Max 16
    public enum PaletteKey : byte
    {
        PlainsGroundSky,
        PlainsGroundMountain,
        PlainsSky,
        PlainsSky2,
        Bomb,
        Player,
        GreenEnemy,
        Bullet,
        StatusBar
    }

    class PaletteModule : Module, IHBlankHandler
    {
        private int _paletteAddress;
        private readonly CoreGraphicsModule _graphicsModule;
        private readonly GameRAM _ram;
        private GameByte _timer;
        private SceneDefinition _currentScene;

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

            DefinePalette(PaletteKey.PlainsGroundSky,
                ChompGameSpecs.LightBlue,
                ChompGameSpecs.Green1,
                ChompGameSpecs.Green2,
                ChompGameSpecs.Green2);

            DefinePalette(PaletteKey.PlainsGroundMountain,
                ChompGameSpecs.BlueGray2,
                ChompGameSpecs.Green1,
                ChompGameSpecs.Green2,
                ChompGameSpecs.Green2);

            DefinePalette(PaletteKey.PlainsSky,
                ChompGameSpecs.LightBlue,
                ChompGameSpecs.Gray1,
                ChompGameSpecs.Gray2,
                ChompGameSpecs.Gray3);

            DefinePalette(PaletteKey.PlainsSky2,
               ChompGameSpecs.Gray3,
               ChompGameSpecs.Green1,
               ChompGameSpecs.BlueGray1,
               ChompGameSpecs.BlueGray2);

            DefinePalette(PaletteKey.Bomb,
                ChompGameSpecs.Black,
                ChompGameSpecs.Gray1,
                ChompGameSpecs.Gray2);

            DefinePalette(PaletteKey.Player,
                ChompGameSpecs.Orange,
                ChompGameSpecs.DarkBrown,
                ChompGameSpecs.LightTan);

            DefinePalette(PaletteKey.GreenEnemy,
                ChompGameSpecs.Green1,
                ChompGameSpecs.Green2,
                ChompGameSpecs.Red3);

            DefinePalette(PaletteKey.Bullet,
                ChompGameSpecs.Red2,
                ChompGameSpecs.Red3,
                ChompGameSpecs.LightYellow);

            DefinePalette(PaletteKey.StatusBar,
                ChompGameSpecs.Black,
                ChompGameSpecs.Blue1,
                ChompGameSpecs.White,
                ChompGameSpecs.Green2);
        }

        public void SetScene(SceneDefinition sceneDefinition)
        {
            _currentScene = sceneDefinition;

            var backgroundPalette = _graphicsModule.GetBackgroundPalette(0);
            var foregroundPalette = _graphicsModule.GetBackgroundPalette(1);            
            var bombPalette = _graphicsModule.GetSpritePalette(0);
            var playerPalette = _graphicsModule.GetSpritePalette(1);
            var enemyPallete = _graphicsModule.GetSpritePalette(2);
            var bulletPallete = _graphicsModule.GetSpritePalette(3);

            switch (sceneDefinition.Theme)
            {
                case Theme.Plains:

                    if (sceneDefinition.GetBgPosition1() == 0)
                        LoadPalette(PaletteKey.PlainsGroundSky, foregroundPalette);
                    else
                        LoadPalette(PaletteKey.PlainsGroundMountain, foregroundPalette);
                  
                    LoadPalette(PaletteKey.PlainsSky, backgroundPalette);
                    _bgPalette1.Value = PaletteKey.PlainsSky;

                    if (sceneDefinition.ScrollStyle == ScrollStyle.Horizontal)
                        _bgPalette2.Value = PaletteKey.PlainsSky2;
 
                    break;
            }

            LoadPalette(PaletteKey.Bomb, bombPalette);
            LoadPalette(PaletteKey.Player, playerPalette);
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
            if(_graphicsModule.ScreenPoint.Y == 0)
            {
                LoadPalette(PaletteKey.StatusBar, _graphicsModule.GetBackgroundPalette(0));
            }
            else if (_graphicsModule.ScreenPoint.Y == Constants.StatusBarHeight)
            {
                LoadPalette(_bgPalette1.Value, _graphicsModule.GetBackgroundPalette(0));
            }
            else if(_graphicsModule.ScreenPoint.Y == _currentScene.GetParallaxLayerPixel(ParallaxLayer.Back2, includeStatusBar: true))
            {
                LoadPalette(_bgPalette2.Value, _graphicsModule.GetBackgroundPalette(0));
            }
        }

        public void CyclePalette3(Palette p)
        {
            var c0 = p.GetColorIndex(0);

            p.SetColor(1,(byte)p.GetColorIndex(2));
            p.SetColor(2,(byte)p.GetColorIndex(3));
            p.SetColor(3,(byte)c0);
        }

        public void Update()
        {
            if (_timer.Value % 8 == 0)
            {
                var bulletPallete = GameSystem.CoreGraphicsModule.GetSpritePalette(3);
                CyclePalette3(bulletPallete);
            }
        }
    }
}
