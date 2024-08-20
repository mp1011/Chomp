using ChompGame.Data;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.SmartBackground
{
    class Pyramid : SmartBackgroundBlock
    {
        private const int PyramidLeft = 24;
        private const int PyramidMid = 25;
        private const int PyramidRight = 26;
        private const int BackPyramidLeft = 27;
        private const int BackPyramidRight = 28;
        private const int BackPyramidMid = 29;
        private const int OverlapPyramidLeft = 30;
        private const int OverlapPyramidRight = 31;

        public Pyramid(SceneDefinition sceneDefinition) : base(sceneDefinition)
        {
        }

        protected override void AddBlock(Rectangle region, NBitPlane nameTable)
        {
            var cursor = new Point(region.Left, region.Top);

            bool isBackPyramid = nameTable[cursor.X, cursor.Y] != 0;

            int width = region.Width;
            while (width >= 0)
            {
                if (isBackPyramid)
                    AddBackPyramidLayer(nameTable, cursor, width);
                else
                    AddPyramidLayer(nameTable, cursor, width);

                width -= 2;
                cursor.X++;
                cursor.Y--;
            }
        }

        protected override IEnumerable<Rectangle> DetermineRegions(NBitPlane nameTable)
        {
            var cursor = new Point(0, nameTable.Height - 1);
            while (nameTable[cursor.X, cursor.Y] != 0)
                cursor.Y--;

            yield return new Rectangle(cursor.X, cursor.Y, 9, 1);
            yield return new Rectangle(cursor.X+8, cursor.Y, 5, 1);
        }

        private void AddPyramidLayer(NBitPlane nameTable, Point cursor, int width)
        {
            nameTable[cursor.X, cursor.Y] = PyramidLeft;
            for (int x = 1; x < width; x++)
                nameTable[cursor.X + x, cursor.Y] = PyramidMid;

            nameTable[cursor.X + width, cursor.Y] = PyramidRight;
        }
        private void AddBackPyramidLayer(NBitPlane nameTable, Point cursor, int width)
        {
            if (nameTable[cursor.X, cursor.Y] == 0)
                nameTable[cursor.X, cursor.Y] = BackPyramidLeft;

            for (int x = 1; x < width; x++)
            {
                if(nameTable[cursor.X + x, cursor.Y] == 0)
                    nameTable[cursor.X + x, cursor.Y] = BackPyramidMid;
                else if (nameTable[cursor.X + x, cursor.Y] == PyramidLeft)
                    nameTable[cursor.X + x, cursor.Y] = OverlapPyramidLeft;
                else if (nameTable[cursor.X + x, cursor.Y] == PyramidRight)
                    nameTable[cursor.X + x, cursor.Y] = OverlapPyramidRight;

            }

            if (nameTable[cursor.X + width, cursor.Y] == 0)
                nameTable[cursor.X + width, cursor.Y] = BackPyramidRight;
        }


    }
}
