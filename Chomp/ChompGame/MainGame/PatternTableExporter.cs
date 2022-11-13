using ChompGame.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace ChompGame.MainGame
{
    static class PatternTableExporter
    {
        /// <summary>
        /// Exports pattern table to an image in the bin folder
        /// </summary>
        /// <param name="device"></param>
        /// <param name="patternTable"></param>
        public static void ExportPatternTable(GraphicsDevice device, NBitPlane patternTable)
        {
            var bmp = new Texture2D(device, patternTable.Width, patternTable.Height);

            Color[] palette = new Color[] { Color.Black, Color.Red, Color.Green, Color.Blue };

            List<Color> colors = new List<Color>();

            for(int i = 0; i < patternTable.Width * patternTable.Height; i++)
            {
                colors.Add(palette[patternTable[i]]);
            }

            bmp.SetData(colors.ToArray());

            if (File.Exists("patterntable.png"))
                File.Delete("patterntable.png");

            using (var fs = new FileStream("patterntable.png", FileMode.Create))
            {
                bmp.SaveAsPng(fs, bmp.Width, bmp.Height);
                fs.Flush();
            }
        }
    }
}
