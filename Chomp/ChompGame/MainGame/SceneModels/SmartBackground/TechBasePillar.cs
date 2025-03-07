using ChompGame.Data;
using ChompGame.Extensions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.MainGame.SceneModels.SmartBackground
{
    class TechBasePillar : SmartBackgroundBlock
    {
        public TechBasePillar(SceneDefinition sceneDefinition) : base(sceneDefinition)
        {
        }

        protected override void AddBlock(Rectangle region, NBitPlane nameTable)
        {
            nameTable.ForEach(new Point(region.Left, region.Top), 
                new Point(region.Right, region.Bottom),
                (x, y, b) =>
                {
                    int relY = region.Bottom - y;

                    if (relY == 1)
                        nameTable[x, y] = 25;
                    else if(relY == region.Height || ((relY.IsMod(3) && (relY+1) != region.Height)))
                        nameTable[x, y] = 27;
                    else
                        nameTable[x, y] = 26;
                });
        }

        protected override IEnumerable<Rectangle> DetermineRegions(NBitPlane nameTable)
        {
            List<Rectangle> regions = new List<Rectangle>();

            nameTable.ForEach((x, y, b) =>
            {
                var pillarSize = PossiblePillarSize(nameTable, x, y);
                if(pillarSize > 0)
                {
                    regions.Add(new Rectangle(x, y - pillarSize + 1, 1, pillarSize));
                }
            });

            return regions;
        }

        private int PossiblePillarSize(NBitPlane nameTable, int x, int y)
        {
            if (!x.IsMod(4))
                return 0;

            if (x < 2 || x >= nameTable.Width - 2)
                return 0;

            if (y >= nameTable.Height - 2)
                return 0;

            if (nameTable[x, y] != 0)
                return 0;

            if (nameTable[x - 1, y] != 0)
                return 0;

            if (nameTable[x + 1, y] != 0)
                return 0;

            if (nameTable[x, y+1] == 0)
                return 0;

            int height = 0;
            while(true)
            {
                if (y - height <= 0)
                    return 0;

                if (nameTable[x, y - height] != 0)
                    break;

                height++;
            }

            return height;

        }
    }
}
