using ChompGame.Data;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.SmartBackground
{
    abstract class SmartBackgroundBlock
    {       
        protected SceneDefinition _sceneDefinition;

        protected SmartBackgroundBlock(SceneDefinition sceneDefinition)
        {
            _sceneDefinition = sceneDefinition;
        }

        protected abstract IEnumerable<Rectangle> DetermineRegions(NBitPlane nameTable);

        public void Apply(NBitPlane nameTable, NBitPlane attributeTable)
        {
            foreach (var region in DetermineRegions(nameTable))
            {
                AddBlock(region, nameTable);
                SetBlockAttr(3,
                    new Rectangle(region.X / 2, region.Y / 2, region.Width / 2, region.Height / 2),
                    attributeTable);
            }
        }

        protected abstract void AddBlock(Rectangle region, NBitPlane nameTable);

        private void SetBlockAttr(byte attributeValue, Rectangle tileRegion, NBitPlane attributeTable)
        {
            attributeTable.ForEach(
                new Point(tileRegion.Left, tileRegion.Top),
                new Point(tileRegion.Right, tileRegion.Bottom),
                (x, y, b) => attributeTable[x, y] = attributeValue);
        }
    }
}
