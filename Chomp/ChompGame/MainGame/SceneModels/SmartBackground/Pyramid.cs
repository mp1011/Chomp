﻿using ChompGame.Data;
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
                _sceneDefinition.GetBackgroundLayerTile(BackgroundPart.Bottom, false) - 1);

            if (_sceneDefinition.ScrollStyle == ScrollStyle.NameTable)
                cursor.Y = 17;


            var randomSeed = (byte)(_sceneDefinition.Address % 256);

            int clusterSize = _rng.FixedRandom(++randomSeed, 2) + 1;

            cursor.X = _rng.FixedRandom(++randomSeed, 3);

            if (_sceneDefinition.ScrollStyle == ScrollStyle.None &&
                (_sceneDefinition.LeftTiles > 0 || _sceneDefinition.RightTiles > 0))
            {
                cursor.X = _sceneDefinition.LevelTileWidth;
            }

            while (cursor.X < _sceneDefinition.LevelTileWidth)
            {
                var width = 7 + (_rng.FixedRandom(++randomSeed, 2) * 2);
                while (cursor.X + width >= nameTable.Width - 1)
                    width -= 2;
                        
                yield return new Rectangle(cursor.X, cursor.Y, width, 1);

                if (clusterSize == 0)
                {
                    clusterSize = _rng.FixedRandom(++randomSeed, 2);
                    cursor.X += width + 7 + _rng.FixedRandom(++randomSeed, 3);
                }
                else
                {
                    cursor.X += width - 2;
                    clusterSize--;
                }
            }
        }

        private void AddPyramidLayer(NBitPlane nameTable, Point cursor, int width)
        {
            nameTable[cursor.X, cursor.Y] = PyramidLeft;
            for (int x = 1; x < width; x++)
            {
                if (nameTable[cursor.X + x, cursor.Y] == 0)
                    nameTable[cursor.X + x, cursor.Y] = PyramidMid;
            }
            
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
