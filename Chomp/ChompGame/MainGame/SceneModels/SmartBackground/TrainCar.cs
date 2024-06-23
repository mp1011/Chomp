using ChompGame.Data;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.SmartBackground
{
    class TrainCar : SmartBackgroundBlock
    {
        public TrainCar(SceneDefinition sceneDefinition) : base(sceneDefinition)
        {
        }

        private const int TopLeft = 8;
        private const int TopRight = 9;
        private const int Bottom = 10;
        private const int Top = 11;
        private const int Left = 12;
        private const int Right = 13;
        private const int Wheel = 14;
        private const int Window = 16;
        private const int BottomLeft = 17;
        private const int BottomRight = 18;
        private const int Under = 3;



        protected override void AddBlock(Rectangle region, NBitPlane nameTable)
        {
            if (region.Height == 3)
                AddFlatCar(region, nameTable);
            else
                AddCar(region, nameTable);
        }

        private void AddCar(Rectangle region, NBitPlane nameTable)
        {
            nameTable.ForEach(new Point(region.X, region.Y), new Point(region.Right, region.Bottom),
               (x, y, b) =>
               {
                   int blockX = x - region.X;
                   int blockY = y - region.Y;

                   if (blockX == 0 && blockY == 0)
                       nameTable[x, y] = TopLeft;
                   else if (blockX == region.Width - 1 && blockY == 0)
                       nameTable[x, y] = TopRight;
                   else if (blockX == 0 && blockY == 2)
                       nameTable[x, y] = BottomLeft;
                   else if (blockX == region.Width - 1 && blockY == 2)
                       nameTable[x, y] = BottomRight;
                   else if (blockX == 0 && blockY < 2)
                       nameTable[x, y] = Left;
                   else if (blockX == region.Width - 1 && blockY < 2)
                       nameTable[x, y] = Right;
                   else if (blockY == 0)
                       nameTable[x, y] = Top;
                   else if (blockY == 1)
                       nameTable[x, y] = Window;
                   else if (blockY == 2)
                       nameTable[x, y] = Bottom;
                   else if (blockY == 3)
                       nameTable[x, y] = (byte)((x % 2 == 0) ? Wheel : Under);
               });
        }

        private void AddFlatCar(Rectangle region, NBitPlane nameTable)
        {
            nameTable.ForEach(new Point(region.X, region.Y), new Point(region.Right, region.Bottom),
               (x, y, b) =>
               {
                   int blockY = y - region.Y;

                   if (blockY == 0)
                       nameTable[x, y] = Bottom;                  
                   else if (blockY == 1)
                       nameTable[x, y] = (byte)((x % 2 == 0) ? Wheel : Under);
               });
        }


        protected override IEnumerable<Rectangle> DetermineRegions(NBitPlane nameTable)
        {
            if (_sceneDefinition.BottomTiles == 0)
            {
                yield return new Rectangle(0, 8, 14, 6);
                yield return new Rectangle(14, 10, 2, 3);
                yield return new Rectangle(16, 8, 12, 6);
                yield return new Rectangle(28, 10, 2, 3);
                yield return new Rectangle(30, 8, 8, 6);
                yield return new Rectangle(38, 10, 12, 3);
                yield return new Rectangle(50, 8, 8, 6);
                yield return new Rectangle(58, 10, 6, 3);
            }
            if (_sceneDefinition.BottomTiles == 2)
            {
                yield return new Rectangle(0, 8, 12, 6);
                yield return new Rectangle(18, 8, 8, 6);
                yield return new Rectangle(26, 10, 1, 3);
                yield return new Rectangle(27, 8, 8, 6);
                yield return new Rectangle(40, 8, 12, 6);
                yield return new Rectangle(52, 10, 12, 3);
            }

            if (_sceneDefinition.BottomTiles == 4)
            {
                yield return new Rectangle(0, 8, 6, 6);
                yield return new Rectangle(14, 10, 16, 3);
                yield return new Rectangle(30, 8, 12, 6);
                yield return new Rectangle(42, 10, 2, 3);
                yield return new Rectangle(44, 8, 8, 6);
                yield return new Rectangle(58, 8, 6, 6);

            }
        }
    }
}
