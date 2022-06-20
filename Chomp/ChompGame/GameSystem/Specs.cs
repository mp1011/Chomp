using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.GameSystem
{
    public class Specs
    {
        public int ScreenWidth => 32;
        public int ScreenHeight => 32;

        public Bit ScreenPointMask => (Bit)31;

        public int PatternTableWidth => 16;
        public int PatternTableHeight => 16;
        public int PatternTablePlanes => 2;
        public Bit PatternTablePointMask => (Bit)15;

        public int NameTableWidth => 16;
        public int NameTableHeight => 8;
        public int NameTableBitPlanes = 4;

        public int ScanlineDrawPlanes = 2;

        public Bit ScrollXMask => (Bit)63;
        public Bit ScrollYMask => (Bit)31;

        public int MaxSprites = 4;
        public int SpritesPerScanline = 3;

        public int TileWidth => 4;
        public int TileHeight => 4;
        public int PatternTableTilesAcross => PatternTableWidth / TileWidth;
        public int PatternTableTilesDown => PatternTableHeight / TileHeight;
        public int MaxDrawInstructions => 64;

        public int AudioChannels = 1;
        public Color[] SystemColors => new Color[] { Color.DarkBlue, Color.BlueViolet, Color.LightBlue, Color.Silver };
    };
}
