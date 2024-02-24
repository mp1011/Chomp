using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.SmartBackground
{
    abstract class SmartBackgroundBlock
    {       
        protected SceneDefinition _sceneDefinition;

        protected SmartBackgroundBlock(SceneDefinition sceneDefinition)
        {
            _sceneDefinition = sceneDefinition;
        }

        public void Apply(NBitPlane nameTable)
        {
            AddBlock(new Rectangle(10, 8, 13, 4), nameTable);
        }

        protected abstract void AddBlock(Rectangle region, NBitPlane nameTable);

        public void ApplyAttributes(NBitPlane attributeTable)
        {
            SetBlockAttr(3, new Rectangle(5, 4, 7, 2), attributeTable);
        }

        private void SetBlockAttr(byte attributeValue, Rectangle tileRegion, NBitPlane attributeTable)
        {
            attributeTable.ForEach(
                new Point(tileRegion.Left, tileRegion.Top),
                new Point(tileRegion.Right, tileRegion.Bottom),
                (x, y, b) => attributeTable[x, y] = attributeValue);
        }
    }
}
