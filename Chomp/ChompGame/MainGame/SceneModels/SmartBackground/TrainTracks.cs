using ChompGame.Data;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.SmartBackground
{
    class TrainTracks : SmartBackgroundBlock
    {
        public TrainTracks(SceneDefinition sceneDefinition) : base(sceneDefinition)
        {
        }

        private const int Ground1 = 1;
        private const int Ground2 = 2;

        protected override void AddBlock(Rectangle region, NBitPlane nameTable)
        {
            nameTable.ForEach(new Point(region.X, region.Y), new Point(region.Right, region.Bottom),
               (x, y, b) =>
               {
                   int blockY = y - region.Y;

                   if (blockY == 3)
                       nameTable[x, y] = (byte)((x % 2 == 0) ? Ground1 : Ground2);
                   else if (blockY == 4)
                       nameTable[x, y] = (byte)((x % 2 == 0) ? Ground2 : Ground1);
                   else
                       nameTable[x, y] = 0;
               });
        }
        protected override IEnumerable<Rectangle> DetermineRegions(NBitPlane nameTable)
        {
            yield return new Rectangle(0, 9, nameTable.Width, 5);            
        }
    }
}
