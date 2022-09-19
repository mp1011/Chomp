using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.ROM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.Graphics
{
    class DiskBitmap
    {
        private readonly Color[] _pixels;
        private readonly int _width;
        private readonly int _height;

        public DiskBitmap(DiskFile file, GraphicsDevice graphicsDevice, Specs specs)
        {
            using (var fs = file.GetFile().OpenRead())
            {
                using (var t = Texture2D.FromStream(graphicsDevice, fs))
                {
                    _pixels = new Color[t.Width * t.Height];
                    t.GetData(_pixels);
                    _width = t.Width;
                    _height = t.Height;
                }
            }

            Validate(specs);
        }

        public Color GetPixel(int x, int y)
        {
            return _pixels[(y * _width) + x];
        }

        private void Validate(Specs specs)
        {
            var uniqueColors = _pixels
                .Distinct()
                .ToArray();

            if(uniqueColors.Length >= specs.SystemColors.Length)
                throw new Exception("Image has too many colors");

            var tilePoint = new ByteGridPoint(_width / specs.TileWidth, _height / specs.TileHeight);
            var pixelPoint = new ByteGridPoint(_width, _height);

            while (true)
            {
                List<Color> tileColors = new List<Color>();

                pixelPoint.X = (byte)(tilePoint.X * specs.TileWidth);
                pixelPoint.Y = (byte)(tilePoint.Y * specs.TileHeight);

                for (int y = 0; y < specs.TileHeight; y++)
                {
                    for (int x = 0; x < specs.TileWidth; x++)
                    {
                        pixelPoint.X = (byte)(tilePoint.X * specs.TileWidth + x);
                        pixelPoint.Y = (byte)(tilePoint.Y * specs.TileHeight + y);
                        tileColors.Add(_pixels[pixelPoint.Index]);
                    }
                }

                tileColors = tileColors
                    .Distinct()
                    .ToList();

                if (tileColors.Count > 2.Power(specs.PatternTablePlanes))
                    throw new Exception("Too many colors in tile");
               
                if (tilePoint.Next() && tilePoint.Y == 0)
                    break;                
            }
        }
    }
}
