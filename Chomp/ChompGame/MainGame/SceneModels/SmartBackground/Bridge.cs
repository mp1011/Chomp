using ChompGame.Data;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.SmartBackground
{
    class Bridge : SmartBackgroundBlock
    {
        private const int Brick = 23;
        private const int BrickLeft = 25;
        private const int BrickRight = 26;

        public Bridge(SceneDefinition sceneDefinition, ChompGameModule chompGameModule)
            : base(sceneDefinition)
        {
        }
        
        protected override IEnumerable<Rectangle> DetermineRegions(NBitPlane nameTable)
        {

            if (_sceneDefinition.ScrollStyle == ScrollStyle.NameTable)
            {
                yield return new Rectangle(4, 12,24,4);
            }
            else
            {
                bool inPit = false;
                int pitBegin = -1;
                int pitLeftHeight = 0;
                int prevGroundHeight = 0;
                int groundHeight = 0;

                for (int x = 0; x < nameTable.Width; x++)
                {
                    groundHeight = 0;
                    for (int y = nameTable.Height - 1; y > 0; y--)
                    {
                        if (nameTable[x, y] == 0)
                            break;

                        groundHeight++;
                    }

                    if (!inPit && groundHeight == 0 && prevGroundHeight != 0)
                    {
                        inPit = true;
                        pitBegin = x;
                        pitLeftHeight = prevGroundHeight;
                    }
                    else if (inPit && groundHeight != 0 && prevGroundHeight == 0)
                    {
                        inPit = false;
                        int pitHeight = pitLeftHeight > groundHeight ? groundHeight : pitLeftHeight;
                        pitHeight = (pitHeight / 2) * 2;
                        yield return new Rectangle(pitBegin, nameTable.Height - pitHeight, x - pitBegin, pitHeight);
                    }

                    prevGroundHeight = groundHeight;
                }
            }
        }

        protected override void AddBlock(Rectangle region, NBitPlane nameTable)
        {
            nameTable.ForEach(new Point(region.Left, region.Top), new Point(region.Right, region.Bottom), (x, y, b) =>
             {
                 if (y == region.Top)
                     nameTable[x, y] = Brick;

                 if (y == region.Top + 1 && x == region.Left)
                     nameTable[x, y] = BrickLeft;

                 if (y == region.Top + 1 && x == region.Right-1)
                     nameTable[x, y] = BrickRight;                 
             });
        }
    }
}
