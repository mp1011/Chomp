using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using System.Xml;

namespace ChompGame.MainGame.SceneModels
{
    class GlitchCoreBgModule
    {
        private GameByte _timer;
        private WorldScroller _scroller;
        private RandomModule _rng;

        public GlitchCoreBgModule(GameByte timer, WorldScroller scroller, RandomModule rng)
        {
            _rng = rng;
            _timer = timer;
            _scroller = scroller;
        }

        public void Update()
        {
            if (!_timer.Value.IsMod(32))
                return;

            _scroller.ModifyTiles((bg, atr) =>
            {
                bg.ForEach((x, y, b) =>
                {
                    if (b == 24 || b == 25)
                    {
                        if (_rng.Generate(1) == 0)
                            bg[x, y] = 24;
                        else
                            bg[x, y] = 25;
                    }
                });
            });
        }
    }
}
