using Chomp.Models;
using Chomp.Services.Interfaces;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Chomp
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var services = DI.Init(args);

            var renderService = services.GetService<IRenderService>();

            using (var game = new XNAEngine(services.GetService<SystemSpecs>()))
            {
                game.OnRender += renderService.Render;
                game.Run();
            }
        }

        static void ComputeNeededSpace()
        {
            var colorsPerPalette = FlexNumber.FromValue(4);
            var numPalettes = FlexNumber.FromBits(2);
            var systemColors = FlexNumber.FromValue(64);

            var screenWidth = FlexNumber.FromValue(256);
            var screenHeight = FlexNumber.FromValue(240);

            var patternTableWidth = FlexNumber.FromValue(128);
            var patternTableHeight = FlexNumber.FromValue(128);

            var nameTableEntries = FlexNumber.FromBits(4);

            var tileSize = FlexNumber.FromValue(8);

            var patternTableBits = FlexNumber.FromBits(patternTableWidth.Value * patternTableHeight.Value * colorsPerPalette.Bits);

            var nameTableWidth = (screenWidth.Value / tileSize.Value);
            var nameTableHeight = (screenHeight.Value / tileSize.Value);

            var patternTableTileWidth = patternTableWidth.Value / tileSize.Value;
            var patternTableTileHeight = patternTableHeight.Value / tileSize.Value;

            var maxTile = FlexNumber.FromValue(patternTableTileWidth * patternTableTileHeight);

            var nameTableBits = FlexNumber.FromBits(nameTableWidth * nameTableHeight * maxTile.Bits);


            var requiredSize = FlexNumber.FromBits(patternTableBits.Bits + nameTableBits.Bits);
        }
    }
}
