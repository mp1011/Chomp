using ChompGame.Data;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.SmartBackground
{
    class Pyramid : SmartBackgroundBlock
    {
        private readonly RandomModule _rng;
        private const int PyramidLeft = 24;
        private const int PyramidMid = 25;
        private const int PyramidRight = 26;
        private const int BackPyramidLeft = 27;
        private const int BackPyramidRight = 28;
        private const int BackPyramidMid = 29;
        private const int OverlapPyramidLeft = 30;
        private const int OverlapPyramidRight = 31;

        public Pyramid(SceneDefinition sceneDefinition, ChompGameModule gameModule) : base(sceneDefinition)
        {
            _rng = gameModule.RandomModule;
        }

        protected override void AddBlock(Rectangle region, NBitPlane nameTable)
        {
            var cursor = new Point(region.Left, region.Top);

            bool isBackPyramid = nameTable[cursor.X, cursor.Y] != 0 || !IsDirectlyOverForeground(region, nameTable);

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

        private bool IsDirectlyOverForeground(Rectangle region, NBitPlane nameTable)
        {
            for(int x = region.Left; x < region.Right; x++)
            {
                if (nameTable[x, region.Top + 1] == 0)
                    return false;
            }

            return true;
        }

        protected override IEnumerable<Rectangle> DetermineRegions(NBitPlane nameTable)
        {
            var cursor = new Point(0, 
                _sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Foreground, false)-1);

            var randomSeed = (byte)(_sceneDefinition.Address % 256);
            bool first = true;
            while (cursor.X < _sceneDefinition.LevelTileWidth)
            {
                cursor.X += _rng.FixedRandom(++randomSeed, 2);

                if (!first && _rng.FixedRandom(++randomSeed,2) < 3)
                    cursor.X += 4 * _rng.FixedRandom(++randomSeed, 2);

                var width = 7 + (_rng.FixedRandom(++randomSeed, 2)*2);
                width = 9;
                first = false;
                if (cursor.X + width < nameTable.Width - 1)
                    yield return new Rectangle(cursor.X, cursor.Y, width, 1);

                cursor.X += width - 2;
            }
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
            else if (nameTable[cursor.X, cursor.Y] == PyramidRight)
                nameTable[cursor.X, cursor.Y] = OverlapPyramidRight;
            else if (nameTable[cursor.X, cursor.Y] == BackPyramidRight)
                nameTable[cursor.X, cursor.Y] = BackPyramidMid;

            for (int x = 1; x < width; x++)
            {
                if(nameTable[cursor.X + x, cursor.Y] == 0)
                    nameTable[cursor.X + x, cursor.Y] = BackPyramidMid;
                else if (nameTable[cursor.X + x, cursor.Y] == PyramidLeft)
                    nameTable[cursor.X + x, cursor.Y] = OverlapPyramidLeft;
                else if (nameTable[cursor.X + x, cursor.Y] == PyramidRight)
                    nameTable[cursor.X + x, cursor.Y] = OverlapPyramidRight;
                else if (nameTable[cursor.X + x, cursor.Y] == BackPyramidRight)
                    nameTable[cursor.X + x, cursor.Y] = BackPyramidMid;
            }

            if (nameTable[cursor.X + width, cursor.Y] == 0)
                nameTable[cursor.X + width, cursor.Y] = BackPyramidRight;
        }


    }
}
