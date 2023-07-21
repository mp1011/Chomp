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
        Forest,
        Desert,
        City,
        Space,
        TechBase,
        GlitchCore,
        Max = 15
    }

    static class ThemeBuilder
    {
        public static void BuildThemes(SystemMemoryBuilder memoryBuilder)
        {
            memoryBuilder.Memory.AddLabel(AddressLabels.Themes, memoryBuilder.CurrentAddress);

            //Plains
            new Theme(memoryBuilder,
                bg1: PaletteKey.PlainsFarMountains,
                bg2: PaletteKey.PlainsCloseMountains, 
                fg: PaletteKey.PlainsGround,
                enemy1: PaletteKey.GreenEnemy,
                enemy2: PaletteKey.Bullet);

            //PlainsEvening
            new Theme(memoryBuilder,
               bg1: PaletteKey.PlainsEveningFarMountains,
               bg2: PaletteKey.PlainsEveningCloseMountains,
               fg: PaletteKey.PlainsGround,
               enemy1: PaletteKey.BlueGrayEnemy,
               enemy2: PaletteKey.Bullet);

            //PlainsBoss
            new Theme(memoryBuilder,
               bomb: PaletteKey.BombLight,
               bg1: PaletteKey.Test,
               bg2: PaletteKey.GreenEnemy2,
               fg: PaletteKey.PlainsGround,
               enemy1: PaletteKey.GreenEnemy3,
               enemy2: PaletteKey.Bullet);

            //Ocean
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);

            //Forest 
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);

            //Desert
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);

            //City 
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);

            //Space
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);

            //TechBase
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);

            //GlitchCore 
            new Theme(memoryBuilder, PaletteKey.Test, PaletteKey.PlainsGround, PaletteKey.Test, PaletteKey.GreenEnemy, PaletteKey.Bullet);
        }
    }

    class Theme
    {
        public const int Bytes = 6;

        private GameByteEnum<PaletteKey> _bg1Palette;
        private GameByteEnum<PaletteKey> _bg2Palette;
        private GameByteEnum<PaletteKey> _fgPalette;
        private GameByteEnum<PaletteKey> _enemy1Palette;
        private GameByteEnum<PaletteKey> _enemy2Palette;
        private GameByteEnum<PaletteKey> _bombPalette;

        public PaletteKey Background1 => _bg1Palette.Value;
        public PaletteKey Background2 => _bg2Palette.Value;

        public PaletteKey Foreground => _fgPalette.Value;
        public PaletteKey Enemy1 => _enemy1Palette.Value;
        public PaletteKey Enemy2 => _enemy2Palette.Value;

        public PaletteKey Bomb => _bombPalette.Value;

        public Theme(SystemMemoryBuilder memoryBuilder, 
            PaletteKey bg1,
            PaletteKey bg2,
            PaletteKey fg, 
            PaletteKey enemy1, 
            PaletteKey enemy2,
            PaletteKey bomb = PaletteKey.Bomb)
        {
            _bg1Palette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _bg2Palette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _fgPalette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _enemy1Palette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _enemy2Palette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());
            _bombPalette = new GameByteEnum<PaletteKey>(memoryBuilder.AddByte());

            _bg1Palette.Value = bg1;
            _bg2Palette.Value = bg2;
            _fgPalette.Value = fg;
            _enemy1Palette.Value = enemy1;
            _enemy2Palette.Value = enemy2;
            _bombPalette.Value = bomb;
        }

        public Theme(SystemMemory memory, int address)
        {
            _bg1Palette = new GameByteEnum<PaletteKey>(new GameByte(address, memory));
            _bg2Palette = new GameByteEnum<PaletteKey>(new GameByte(address+1, memory));
            _fgPalette = new GameByteEnum<PaletteKey>(new GameByte(address+2, memory));
            _enemy1Palette = new GameByteEnum<PaletteKey>(new GameByte(address+3, memory));
            _enemy2Palette = new GameByteEnum<PaletteKey>(new GameByte(address+4, memory));
            _bombPalette = new GameByteEnum<PaletteKey>(new GameByte(address+5, memory));
        }

        public Theme(SystemMemory memory, ThemeType type) :
            this(memory, memory.GetAddress(AddressLabels.Themes) + (int)type * Bytes) { }
    }
}
