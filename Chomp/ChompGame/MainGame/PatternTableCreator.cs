using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.ROM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

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

            Color[] colors = new Color[]
            {
                new Color(254,10,255),
                new Color(0,0,0),
                new Color(57,66,99),
                new Color(255,255,255)
            };

            masterPatternTable.ForEach((x, y, b) =>
            {
                var imageColor = masterPatternTableImage.GetPixel(x, y);
                int colorIndex = Array.IndexOf(colors, imageColor);
                if (colorIndex == -1)
                    throw new Exception($"Unexpected color at {x} {y}");

                masterPatternTable[x, y] = (byte)colorIndex;
            });

            ExportToDisk(
                masterPatternTable,
                new DiskFile(ContentFolder.PatternTables, "master.pt"));

            PatternTableExporter.ExportPatternTable(graphicsDevice, masterPatternTable, "master.png");
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
    
    }
}
