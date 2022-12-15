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

        public virtual int BitsPerPixel => 2;

        public virtual int AudioROMBytes => 64;

        public virtual int NumSounds => 8;

        public virtual int GameRAMSize => 0;
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

        public override int MaxSprites => 32;
        public override int SpritesPerScanline => 8;

        public override int TileWidth => 4;
        public override int TileHeight => 4;

        public override int AudioChannels => 2;

        public override int BytesPerPalette => 3;
        public override int NumPalettes => 6;
        public override int GameRAMSize => 1024;
        public override Color[] SystemColors { get; } = new Color[64];

        public ChompGameSpecs()
        {
            SystemColors[Black] = new Color(0, 0, 0);
            SystemColors[White] = new Color(255, 255, 255);

            SystemColors[Blue1] = new Color(0, 78, 168);
            SystemColors[Blue2] = new Color(0, 129, 192);
            SystemColors[Blue3] = new Color(0, 198, 243);
            SystemColors[Blue4] = new Color(72, 219, 255);

            SystemColors[LightBlue] = new Color(183, 255, 255);
            SystemColors[LightYellow] = new Color(255, 255, 129);
            SystemColors[LightTan] = new Color(247, 193, 155);

            SystemColors[Green1] = new Color(0, 120, 0);
            SystemColors[Green2] = new Color(120, 220, 0);
            SystemColors[Green3] = new Color(183, 240, 144);

            SystemColors[Orange] = new Color(255, 108, 15);
            SystemColors[DarkBrown] = new Color(117, 29, 15);

            SystemColors[Red1] = new Color(99, 0, 0);
            SystemColors[Red2] = new Color(207, 57, 57);
            SystemColors[Red3] = new Color(255, 57, 57);

            SystemColors[Gray1] = new Color(57, 66, 99);
            SystemColors[Gray2] = new Color(132, 141, 150);
            SystemColors[Gray3] = new Color(150, 180, 180);

            SystemColors[BlueGray1] = new Color(93, 105, 156);
            SystemColors[BlueGray2] = new Color(120, 132, 201);


        }

        public const int Black = 0;
        public const int White = 1;
        
        public const int Blue1 = 2;
        public const int Blue2 = 3;
        public const int Blue3 = 4;
        public const int Blue4 = 5;

        public const int LightBlue = 6;
        public const int LightYellow = 7;
        public const int LightTan = 8;

        public const int Green1 = 9;
        public const int Green2 = 10;
        public const int Green3 = 11;

        public const int Orange = 12;
        public const int DarkBrown = 13;

        public const int Red1 = 14;
        public const int Red2 = 15;
        public const int Red3 = 16;

        public const int Gray1 = 17;
        public const int Gray2 = 18;
        public const int Gray3 = 19;

        public const int BlueGray1 = 20;
        public const int BlueGray2 = 21;




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
