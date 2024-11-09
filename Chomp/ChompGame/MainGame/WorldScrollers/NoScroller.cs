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

        public override bool Update() => false;
    }
}
