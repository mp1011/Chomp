using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.WorldScrollers
{
    class NoScroller : WorldScroller
    {
        public NoScroller(SystemMemoryBuilder memoryBuilder, Specs specs, TileModule tileModule, SpritesModule spritesModule) : base(memoryBuilder, specs, tileModule, spritesModule)
        {
        }

        public override Rectangle ViewPane
        {
            get
            {
                return new Rectangle(0, 0, _specs.ScreenWidth, _specs.ScreenHeight);
            }
        }

        public override void RefreshNametable()
        {
            for(byte col = 0; col < _tilesPerScreen; col++)
            {
                CopyTileColumn(col, col);
            }
        }

        public override int DistanceFromViewpane(Rectangle r)
        {
            if (r.Intersects(ViewPane))
                return 0;

            var downDistance = ViewPane.Top - r.Bottom;
            var upDistance = r.Top - ViewPane.Bottom;
            var rightDistance = ViewPane.X - r.Right;
            var leftDistance = r.Left - ViewPane.Right;

            int hDistance = 0, vDistance = 0;

            if (rightDistance > 0)
                hDistance = rightDistance;
            else if (leftDistance > 0)
                hDistance = leftDistance;

            if (downDistance > 0)
                vDistance = downDistance;
            else if (upDistance > 0)
                vDistance = upDistance;

            if (hDistance > 0 && vDistance > 0)
            {
                if (hDistance < vDistance)
                    return hDistance;
                else
                    return vDistance;
            }
            else if (hDistance > 0)
                return hDistance;
            else
                return vDistance;
        }

        public override bool Update() => false;
    }
}
