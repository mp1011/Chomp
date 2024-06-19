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
        private const int Ground1 = 1;
        private const int Ground2 = 2;



        protected override void AddBlock(Rectangle region, NBitPlane nameTable)
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
                   else if (blockX == region.Width-1 && blockY == 2)
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
                       nameTable[x, y] = (byte)((x % 2 == 0) ? Wheel : 0);
                   else if (blockY == 4)
                       nameTable[x, y] = (byte)((x % 2 == 0) ? Ground1 : Ground2);
               });
        }
        protected override IEnumerable<Rectangle> DetermineRegions(NBitPlane nameTable)
        {
            yield return new Rectangle(0, 9, 16, 5);
        }
    }
}
