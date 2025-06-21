using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace ChompGame.MainGame.Editors
{
    class TileInspector
    {
        private readonly ChompGameModule _gameModule;

        private WorldScroller WorldScroller => _gameModule.WorldScroller;
        private TileModule TileModule => _gameModule.TileModule;

        public TileInspector(ChompGameModule gameModule)
        {
            _gameModule = gameModule;
        }

        public void Update()
        {
            if (!EditorInputHelper.LeftClicked)
                return;

            if (WorldScroller == null)
                return;

            var levelPoint = MousePointToLevelPoint(EditorInputHelper.MousePosition.Subtract(TileModule.Scroll));
            var tilePoint = levelPoint.Divide(_gameModule.Specs.TileWidth);
            var tile = WorldScroller.LevelNameTable[tilePoint.X, tilePoint.Y];
            //WorldScroller.RefreshNametable();
            Debug.WriteLine($"Screen point = {EditorInputHelper.MousePosition.Subtract(TileModule.Scroll) }");
            Debug.WriteLine($"Level point = {levelPoint}");
            Debug.WriteLine($"Tile point = {tilePoint}");
            Debug.WriteLine($"Tile = {tile}");

            var attrPoint = tilePoint.Divide(2);
            var attr = WorldScroller.LevelAttributeTable[attrPoint.X, attrPoint.Y];
            Debug.WriteLine($"Attr = {attr}");
        }

        private Point MousePointToLevelPoint(Point mousePoint) =>
            new Point(WorldScroller.ViewPane.X + mousePoint.X,
                WorldScroller.ViewPane.Y + mousePoint.Y);
    }
}
