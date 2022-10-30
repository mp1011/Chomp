using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.ROM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;

namespace ChompGame.MainGame
{
    class PatternTableCreator
    {
        public static void SetupMasterPatternTable(
            NBitPlane masterPatternTable,
            GraphicsDevice graphicsDevice, 
            Specs specs)
        {
            var masterPatternTableImage = new DiskBitmap(
                new DiskFile(ContentFolder.PatternTables, "master_pt.bmp"),
                graphicsDevice,
                specs);

            //player
            CopyPatternTableRegion(
                masterPatternTableImage,
                masterPatternTable,
                new Rectangle(0, 0, 2, 2),
                specs);

            //lizard enemy
            CopyPatternTableRegion(
              masterPatternTableImage,
              masterPatternTable,
              new Rectangle(2, 0, 2, 2),
              specs);

            //fireball
            CopyPatternTableRegion(
              masterPatternTableImage,
              masterPatternTable,
              new Rectangle(4, 0, 4, 1),
              specs);

            //coin
            CopyPatternTableRegion(
              masterPatternTableImage,
              masterPatternTable,
              new Rectangle(4, 1, 1, 1),
              specs);

            //text and health guage
            CopyPatternTableRegion(
               masterPatternTableImage,
               masterPatternTable,
               new Rectangle(0, 3, 14, 2),
                specs);


            CopyPatternTableRegion(
               masterPatternTableImage,
               masterPatternTable,
               new Rectangle(0, 6, 8, 1),
                specs);

            CopyPatternTableRegion(
               masterPatternTableImage,
               masterPatternTable,
               new Rectangle(0, 7, 6, 1),
                specs);

            CopyPatternTableRegion(
              masterPatternTableImage,
              masterPatternTable,
              new Rectangle(5, 7, 3, 1),
               specs);

            ExportToDisk(
                masterPatternTable,
                new DiskFile(ContentFolder.PatternTables, "master.pt"));
        }

        private static void ExportToDisk(NBitPlane table, DiskFile destination)
        {
            var file = destination.GetFile();
            if (file.Exists)
                file.Delete();

            string[] rows = new string[table.Height]; 
            char[] columnValues = new char[table.Width];

            for (int row = 0; row < table.Height; row++)
            {
                for(int col = 0; col < table.Width; col++)
                {
                    columnValues[col] = table[col, row].ToString("X")[0];
                }

                rows[row] = new string(columnValues);
            }


            File.WriteAllLines(file.FullName, rows);
                
        }

        private static void CopyPatternTableRegion(
            DiskBitmap masterImage, 
            NBitPlane masterPatternTable, 
            Rectangle tileRegion,
            Specs specs)
        {
            Color transparentColor = new Color(255, 0, 255);
            var pixelRegion = new Rectangle(
                tileRegion.X * specs.TileWidth,
                tileRegion.Y * specs.TileHeight,
                tileRegion.Width * specs.TileWidth,
                tileRegion.Height * specs.TileHeight);

            Color[] colors = new Color[2.Power(specs.BitsPerPixel)];
            int insertIndex = 1;
            int pixelIndex = 0;

            for (int y = pixelRegion.Y; y < pixelRegion.Bottom; y++)
            {
                for (int x = pixelRegion.X; x < pixelRegion.Right; x++)
                {
                    Color pixelColor = masterImage.GetPixel(x, y);

                    if (pixelColor.Equals(transparentColor))
                    {
                        colors[0] = new Color(0, 0, 0, 0);
                        pixelIndex = 0;
                    }
                    else
                    {
                        int index = Array.IndexOf(colors, pixelColor);

                        if (index == -1)
                        {
                            colors[insertIndex] = pixelColor;
                            pixelIndex = insertIndex;

                            insertIndex++;
                            if (insertIndex == colors.Length)
                                insertIndex = 0;
                        }
                        else
                            pixelIndex = index;
                    }

                    masterPatternTable[x, y] = (byte)pixelIndex;
                }
            }
        }

    }
}
