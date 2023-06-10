using ChompGame.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static ChompGame.MainGame.ChompAudioService;

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
        public virtual int AttributeTableBlockSize { get; } = 2;
        public virtual int AttributeTableBitsPerBlock => 1;
        public abstract Bit ScrollXMask { get; }
        public abstract Bit ScrollYMask { get; }
        public abstract int MaxSprites { get; }
        public abstract int SpritesPerScanline { get; }

        public abstract int TileWidth { get; }
        public abstract int TileHeight { get; }
        public int PatternTableTilesAcross => PatternTableWidth / TileWidth;
        public int PatternTableTilesDown => PatternTableHeight / TileHeight;
        public int NameTablePixelWidth => NameTableWidth * TileWidth;
        public int NameTablePixelHeight => NameTableHeight * TileHeight;
        public abstract int AudioChannels { get; }
        public abstract Color[] SystemColors { get; }

        public virtual int BytesPerPalette { get; } = 2;
        public virtual int NumPalettes { get; } = 1;

        public virtual int BackgroundPalettes => 1;

        public virtual int BitsPerPixel => 2;

        public virtual int AudioROMBytes => 64;

        public virtual int NumSounds => 8;

        public virtual int GameRAMSize => 0;

        public virtual void SetColors(Texture2D texture) { }

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

        public override int NameTableWidth => 16;
        public override int NameTableHeight => 16;
        public override int NameTableBitPlanes => 4;

        public override int ScanlineDrawPlanes => 2;

        public override Bit ScrollXMask => (Bit)255;
        public override Bit ScrollYMask => (Bit)255;

        public override int MaxSprites => 4;
        public override int SpritesPerScanline => 3;

        public override int TileWidth => 4;
        public override int TileHeight => 4;

        public override int AudioChannels => 1;

        public override Color[] SystemColors => new Color[] 
        { 
            Color.Black,        //0
            Color.Silver,       //1
            Color.White,        //2
            Color.DarkBlue,     //3
            Color.BlueViolet,   //4
            Color.LightBlue,    //5
            Color.DarkRed,      //6
            Color.IndianRed,    //7
            Color.LightSalmon,  //8
            Color.DarkOliveGreen,//9
            Color.GreenYellow,  //10
            Color.LightSeaGreen,//11
            Color.SaddleBrown,  //12
            Color.SandyBrown,   //13
            Color.Tan,          //14
            Color.DarkOrange    //15
        };

    }

    public class PlatformerSpecs : Specs
    {
        public override int ScreenWidth => 64;
        public override int ScreenHeight => 64;

        public override Bit ScreenPointMask => (Bit)63;

        public override int PatternTableWidth => 32;
        public override int PatternTableHeight => 32;
        public override int PatternTablePlanes => 2;
        public override Bit PatternTablePointMask => (Bit)31;

        public override int NameTableWidth => 32;
        public override int NameTableHeight => 32;
        public override int NameTableBitPlanes => 4;

        public override int ScanlineDrawPlanes => 2;

        public override Bit ScrollXMask => (Bit)255;
        public override Bit ScrollYMask => (Bit)255;

        public override int MaxSprites => 8;
        public override int SpritesPerScanline => 3;

        public override int TileWidth => 4;
        public override int TileHeight => 4;

        public override int AudioChannels => 2;

        public override int BytesPerPalette => 2;
        public override int NumPalettes => 4;

        public override Color[] SystemColors => new Color[]
        {
            Color.LightCyan,    //0
            Color.SaddleBrown,  //1
            Color.ForestGreen,  //2
            Color.OrangeRed,    //3
            Color.BlueViolet,   //4
            Color.LightBlue,    //5
            Color.DarkRed,      //6
            Color.IndianRed,    //7
            Color.LightSalmon,  //8
            Color.DarkOliveGreen,//9
            Color.GreenYellow,  //10
            Color.LightSeaGreen,//11
            Color.SaddleBrown,  //12
            Color.DarkSlateBlue, //13
            Color.Tan,          //14
            Color.LightYellow    //15
        };

    }

    public class ChompGameSpecs : Specs
    {
        public override int AttributeTableBitsPerBlock => 2;
        public override int ScreenWidth => 64;
        public override int ScreenHeight => 64;

        public override int NumSounds => (int)(Sound.Max + 1);
        public override Bit ScreenPointMask => (Bit)63;

        public override int PatternTableWidth => 32;
        public override int PatternTableHeight => 32;
        public override int PatternTablePlanes => 2;
        public override Bit PatternTablePointMask => (Bit)31;

        public override int NameTableWidth => 32;
        public override int NameTableHeight => 32;
        public override int NameTableBitPlanes => 5;

        public override int ScanlineDrawPlanes => 2;

        public override Bit ScrollXMask => (Bit)255;
        public override Bit ScrollYMask => (Bit)255;

        public override int MaxSprites => 32;
        public override int SpritesPerScanline => 8;

        public override int TileWidth => 4;
        public override int TileHeight => 4;

        public override int AudioChannels => 2;

        public override int BytesPerPalette => 3;
        public override int NumPalettes => 8;
        public override int BackgroundPalettes => 4;
        public override int GameRAMSize => 2048; //see if we can lower this
        public override Color[] SystemColors => _colors;

        private Color[] _colors = new Color[64];

        public override void SetColors(Texture2D texture)
        {
            texture.GetData(_colors);
        }

        public ChompGameSpecs()
        {
            

        }
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
        public override int AudioChannels => 1;

        public override Color[] SystemColors => new Color[]
       {
            Color.Black,        //0
            Color.Silver,       //1
            Color.White,        //2
            Color.DarkBlue,     //3
            Color.BlueViolet,   //4
            Color.LightBlue,    //5
            Color.DarkRed,      //6
            Color.IndianRed,    //7
            Color.LightSalmon,  //8
            Color.DarkOliveGreen,//9
            Color.GreenYellow,  //10
            Color.LightSeaGreen,//11
            Color.SaddleBrown,  //12
            Color.SandyBrown,   //13
            Color.Tan,          //14
            Color.DarkOrange    //15
       };
    }
}
