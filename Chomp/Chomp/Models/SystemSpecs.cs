namespace Chomp.Models
{
    public record SystemSpecs(
        int ScreenWidth, 
        int ScreenHeight,
        int TileSize,
        int WorkingRAM,
        int BitsPerPixel,
        TableSpecs NameTableSpecs,
        TableSpecs PatternTableSpecs)
    {

        public static SystemSpecs Default => new SystemSpecs(
                        ScreenWidth: 256,
                        ScreenHeight: 240,
                        TileSize: 8,
                        BitsPerPixel: 2,
                        WorkingRAM: 2048,
                        NameTableSpecs: new TableSpecs(
                            Width: 512 / 8,
                            Height: 480 / 8),
                        PatternTableSpecs: new TableSpecs(
                            Width: 64,
                            Height: 64));
    }

    public record TableSpecs(int Width, int Height)
    {
        public int Cells => Width * Height;
    }
    
}
