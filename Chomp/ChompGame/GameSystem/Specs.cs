using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.GameSystem
{
    public abstract class Specs
    {
        public abstract int ScreenWidth { get; }
        public abstract int ScreenHeight { get; }

        public abstract Bit ScreenPointMask { get; }

        public abstract int PatternTableWidth { get; }
        public abstract int PatternTableHeight { get; }
        public abstract int PatternTablePlanes { get; }
        public abstract Bit PatternTablePointMask { get; }
        public abstract int NameTableWidth { get; }
        public abstract int NameTableHeight { get; }
        public abstract int NameTableBitPlanes { get; }
        public abstract int ScanlineDrawPlanes { get; }
        public abstract Bit ScrollXMask { get; }
        public abstract Bit ScrollYMask { get; }

        public abstract int MaxSprites { get; }
        public abstract int SpritesPerScanline { get; }

        public abstract int TileWidth { get; }
        public abstract int TileHeight { get; }
        public int PatternTableTilesAcross => PatternTableWidth / TileWidth;
        public int PatternTableTilesDown => PatternTableHeight / TileHeight;
        public abstract int MaxDrawInstructions { get; }
        public abstract int AudioChannels { get; }
        public abstract Color[] SystemColors { get; }
    };

    public class PongSpecs : Specs
    {
        public override int ScreenWidth => 32;
        public override int ScreenHeight => 32;

        public override Bit ScreenPointMask => (Bit)31;

        public override int PatternTableWidth => 16;
        public override int PatternTableHeight => 16;
        public override int PatternTablePlanes => 2;
        public override Bit PatternTablePointMask => (Bit)15;

        public override int NameTableWidth => 16;
        public override int NameTableHeight => 8;
        public override int NameTableBitPlanes => 4;

        public override int ScanlineDrawPlanes => 2;

        public override Bit ScrollXMask => (Bit)63;
        public override Bit ScrollYMask => (Bit)31;

        public override int MaxSprites => 4;
        public override int SpritesPerScanline => 3;

        public override int TileWidth => 4;
        public override int TileHeight => 4;
        public override int MaxDrawInstructions => 64;

        public override int AudioChannels => 1;
        public override Color[] SystemColors => new Color[] { Color.DarkBlue, Color.BlueViolet, Color.LightBlue, Color.Silver };
    };


    public class SnakeSpecs : Specs
    {
        public override int ScreenWidth => 64;
        public override int ScreenHeight => 64;

        public override Bit ScreenPointMask => (Bit)63;

        public override int PatternTableWidth => 16;
        public override int PatternTableHeight => 16;
        public override int PatternTablePlanes => 2;
        public override Bit PatternTablePointMask => (Bit)15;

        public override int NameTableWidth => 64;
        public override int NameTableHeight => 64;
        public override int NameTableBitPlanes => 4;

        public override int ScanlineDrawPlanes => 2;

        public override Bit ScrollXMask => (Bit)255;
        public override Bit ScrollYMask => (Bit)255;

        public override int MaxSprites => 4;
        public override int SpritesPerScanline => 3;

        public override int TileWidth => 4;
        public override int TileHeight => 4;
        public override int MaxDrawInstructions => 64;

        public override int AudioChannels => 1;
        public override Color[] SystemColors => new Color[] { Color.DarkBlue, Color.BlueViolet, Color.LightBlue, Color.Silver };

    }

    public class FullScreenTestSpecs : Specs
    {
        public override int ScreenWidth => 256;
        public override int ScreenHeight => 256;

        public override Bit ScreenPointMask => (Bit)255;

        public override int PatternTableWidth => 16;
        public override int PatternTableHeight => 16;
        public override int PatternTablePlanes => 2;
        public override Bit PatternTablePointMask => (Bit)15;

        public override int NameTableWidth => 64;
        public override int NameTableHeight => 64;
        public override int NameTableBitPlanes => 4;

        public override int ScanlineDrawPlanes => 2;

        public override Bit ScrollXMask => (Bit)255;
        public override Bit ScrollYMask => (Bit)255;

        public override int MaxSprites => 4;
        public override int SpritesPerScanline => 3;

        public override int TileWidth => 8;
        public override int TileHeight => 8;
        public override int MaxDrawInstructions => 512;

        public override int AudioChannels => 1;
        public override Color[] SystemColors => new Color[] { Color.DarkBlue, Color.BlueViolet, Color.LightBlue, Color.Silver };
    };

}
