namespace ChompGame.GameSystem
{
    public class Specs
    {
        public int ScreenWidth => 32;
        public int ScreenHeight => 32;
        public int PatternTableWidth => 8;
        public int PatternTableHeight => 8;

        public int NameTableWidth => 8;
        public int NameTableHeight => 8;

        public int TileWidth => ScreenWidth / NameTableWidth;
        public int TileHeight => ScreenHeight / NameTableHeight;

        public int PatternTableTilesAcross => PatternTableWidth / TileWidth;
        public int PatternTableTilesDown => PatternTableHeight / TileHeight;

        public int MaxDrawInstructions => 32;
    }
}
