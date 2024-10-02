using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.MainGame.SceneModels
{
    public enum ThemeType : byte
    {
        Plains = 0,
        PlainsEvening,
        PlainsBoss,
        Ocean,
        OceanAutoscroll,
        OceanBoss,
        Forest,
        Desert,
        DesertInterior,
        DesertNight,
        DesertRain,
        City,
        CityInterior,
        CityBoss,
        CityEvening,
        CityTrain,
        Space,
        TechBase,
        GlitchCore,
    }

    static class ThemeBuilder
    {
        public static void BuildThemes(SystemMemoryBuilder memoryBuilder)
        {
            memoryBuilder.Memory.AddLabel(AddressLabels.Themes, memoryBuilder.CurrentAddress);

            ThemeType _;

            _ = ThemeType.Plains;
            new Theme(memoryBuilder,
                bg1: PaletteKey.PlainsFarMountains,
                bg2: PaletteKey.PlainsCloseMountains, 
                fg1: PaletteKey.PlainsGround,
                fg2: PaletteKey.Gray,
                enemy1: PaletteKey.GreenEnemy,
                enemy2: PaletteKey.Gray);

            _ = ThemeType.PlainsEvening;
            new Theme(memoryBuilder,
               bg1: PaletteKey.PlainsEveningFarMountains,
               bg2: PaletteKey.PlainsEveningCloseMountains,
               fg1: PaletteKey.PlainsGround,
               fg2: PaletteKey.Gray,
               enemy1: PaletteKey.BlueGrayEnemy,
               enemy2: PaletteKey.Gray);

            _ = ThemeType.PlainsBoss;
            new Theme(memoryBuilder,
               bomb: PaletteKey.BombLight,
               bg1: PaletteKey.Test,
               bg2: PaletteKey.GreenEnemy2,
               fg1: PaletteKey.PlainsGround,
               fg2: PaletteKey.Gray,
               enemy1: PaletteKey.GreenEnemy3,
               enemy2: PaletteKey.Bullet);

            _ = ThemeType.Ocean;
            new Theme(memoryBuilder, 
               bomb: PaletteKey.Gray,
               bg1: PaletteKey.OceanSky,
               bg2: PaletteKey.Water,
               fg1: PaletteKey.Sand,
               fg2: PaletteKey.Gray,
               enemy1: PaletteKey.GreenEnemy3,
               enemy2: PaletteKey.Gray);

            _ = ThemeType.OceanAutoscroll;
            new Theme(memoryBuilder,
               bomb: PaletteKey.Gray,
               bg1: PaletteKey.PlainsFarMountains,
               bg2: PaletteKey.Water,
               fg1: PaletteKey.Sand,
               fg2: PaletteKey.Gray,
               enemy1: PaletteKey.GreenEnemy3,
               enemy2: PaletteKey.Gray);

            _ = ThemeType.OceanBoss;
            new Theme(memoryBuilder,
               bomb: PaletteKey.BombLight,
               bg1: PaletteKey.Test,
               bg2: PaletteKey.BlueEnemy2,
               fg1: PaletteKey.Sand,
               fg2: PaletteKey.Gray,
               enemy1: PaletteKey.BlueEnemy,
               enemy2: PaletteKey.Bullet);

            //Forest 
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);

            _ = ThemeType.Desert;
            new Theme(memoryBuilder,
                bg1: PaletteKey.PurpleSky,
                bg2: PaletteKey.PurpleSky,
                fg1: PaletteKey.Sand,
                fg2: PaletteKey.Gray,
                bomb: PaletteKey.Gray,
                enemy1: PaletteKey.Bullet,
                enemy2: PaletteKey.Bullet);

            _ = ThemeType.DesertInterior;
            new Theme(memoryBuilder,
                bg1: PaletteKey.PyramidBg,
                bg2: PaletteKey.PyramidTorches,
                fg1: PaletteKey.BrownBrick,
                fg2: PaletteKey.Gray,
                bomb: PaletteKey.Gray,
                enemy1: PaletteKey.Bullet,
                enemy2: PaletteKey.Bullet);

            _ = ThemeType.DesertNight;
            new Theme(memoryBuilder,
                bg1: PaletteKey.DesertNight,
                bg2: PaletteKey.DesertNight,
                fg1: PaletteKey.DesertNight,
                fg2: PaletteKey.DesertNight,
                bomb: PaletteKey.Gray,
                enemy1: PaletteKey.Bullet,
                enemy2: PaletteKey.Bullet);

            _ = ThemeType.DesertRain;
            new Theme(memoryBuilder,
                bg1: PaletteKey.DesertRain,
                bg2: PaletteKey.DesertNight,
                fg1: PaletteKey.DesertNight,
                fg2: PaletteKey.DesertNight,
                bomb: PaletteKey.Gray,
                enemy1: PaletteKey.Bullet,
                enemy2: PaletteKey.Gray);

            _ = ThemeType.City;
            new Theme(memoryBuilder,
                bg1: PaletteKey.CitySky,
                bg2: PaletteKey.CityBg,
                fg1: PaletteKey.CityFg,
                fg2: PaletteKey.Gray,
                bomb: PaletteKey.Gray,
                enemy1: PaletteKey.GreenEnemy,
                enemy2: PaletteKey.Gray);

            _ = ThemeType.CityInterior;
            new Theme(memoryBuilder,
                bg1: PaletteKey.CityInterior,
                bg2: PaletteKey.CityInterior,
                fg1: PaletteKey.CityFg,
                fg2: PaletteKey.BombLight,
                bomb: PaletteKey.BombLight,
                enemy1: PaletteKey.BlueGrayEnemy,
                enemy2: PaletteKey.BombLight);

            _ = ThemeType.CityBoss;
            new Theme(memoryBuilder,
               bomb: PaletteKey.BombLight,
               bg1: PaletteKey.Test,
               bg2: PaletteKey.GreenEnemy2,
               fg1: PaletteKey.CityFgEvening,
               fg2: PaletteKey.Gray,
               enemy1: PaletteKey.GreenEnemy3,
               enemy2: PaletteKey.Bullet);

            _ = ThemeType.CityEvening;
            new Theme(memoryBuilder,
                bg1: PaletteKey.CitySkyEvening,
                bg2: PaletteKey.CityBgEvening,
                fg1: PaletteKey.CityFgEvening,
                fg2: PaletteKey.BombLight,
                bomb: PaletteKey.Gray,
                enemy1: PaletteKey.GreenEnemy,
                enemy2: PaletteKey.Gray);

            _ = ThemeType.CityTrain;
            new Theme(memoryBuilder,
                bg1: PaletteKey.CitySkyEvening,
                bg2: PaletteKey.CityBgEvening,
                fg1: PaletteKey.CityFgEvening,
                fg2: PaletteKey.BombLight,
                bomb: PaletteKey.Gray,
                enemy1: PaletteKey.GreenEnemy,
                enemy2: PaletteKey.Gray);

            //Space
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);

            //TechBase
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);

            //GlitchCore 
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);
        }
    }

    class Theme
    {
        public const int Bytes = 7;

        private GameByteEnum<PaletteKey> _bg1Palette;
        private GameByteEnum<PaletteKey> _bg2Palette;
        private GameByteEnum<PaletteKey> _fg1Palette;
        private GameByteEnum<PaletteKey> _fg2Palette;
        private GameByteEnum<PaletteKey> _enemy1Palette;
        private GameByteEnum<PaletteKey> _enemy2Palette;
        private GameByteEnum<PaletteKey> _bombPalette;

        public PaletteKey Background1 => _bg1Palette.Value;
        public PaletteKey Background2 => _bg2Palette.Value;
        public PaletteKey Foreground1 => _fg1Palette.Value;
        public PaletteKey Foreground2 => _fg2Palette.Value;

        public PaletteKey Enemy1 => _enemy1Palette.Value;
        public PaletteKey Enemy2 => _enemy2Palette.Value;

        public PaletteKey Bomb => _bombPalette.Value;

        public Theme(SystemMemoryBuilder memoryBuilder, 
            PaletteKey bg1,
            PaletteKey bg2,
            PaletteKey fg1,
            PaletteKey fg2,
            PaletteKey enemy1, 
            PaletteKey enemy2,
            PaletteKey bomb = PaletteKey.Gray)
        {
            _bg1Palette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _bg2Palette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _fg1Palette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _fg2Palette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _enemy1Palette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _enemy2Palette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _bombPalette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());

            _bg1Palette.Value = bg1;
            _bg2Palette.Value = bg2;
            _fg1Palette.Value = fg1;
            _fg2Palette.Value = fg2;
            _enemy1Palette.Value = enemy1;
            _enemy2Palette.Value = enemy2;
            _bombPalette.Value = bomb;
        }

        public Theme(SystemMemory memory, int address)
        {
            _bg1Palette = new GameByteEnum<PaletteKey>(new GameByte(address, memory));
            _bg2Palette = new GameByteEnum<PaletteKey>(new GameByte(address+1, memory));
            _fg1Palette = new GameByteEnum<PaletteKey>(new GameByte(address+2, memory));
            _fg2Palette = new GameByteEnum<PaletteKey>(new GameByte(address + 3, memory));
            _enemy1Palette = new GameByteEnum<PaletteKey>(new GameByte(address+4, memory));
            _enemy2Palette = new GameByteEnum<PaletteKey>(new GameByte(address+5, memory));
            _bombPalette = new GameByteEnum<PaletteKey>(new GameByte(address+6, memory));
        }

        public Theme(SystemMemory memory, ThemeType type) :
            this(memory, memory.GetAddress(AddressLabels.Themes) + (int)type * Bytes) { }
    }
}
