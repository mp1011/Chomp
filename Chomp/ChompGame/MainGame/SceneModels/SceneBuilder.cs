using ChompGame.Data;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels
{
    class SceneBuilder
    {
        public static SceneDefinition SetupTestScene(SystemMemoryBuilder memoryBuilder, Specs specs)
        {
            SceneDefinition testScene = new SceneDefinition(
                specs: specs,
                scrollStyle: ScrollStyle.Horizontal,
                levelShape: LevelShape.MediumVariance,
                beginTiles:0,
                endTiles:2,
                spriteLoadFlags: SpriteLoadFlags.Player | SpriteLoadFlags.Bird | SpriteLoadFlags.Lizard,
                memoryBuilder: memoryBuilder,
                tileRow: 8,         
                groundFillTiles: 2,
                groundTopTiles: 1,
                sideTiles: 1,
                parallaxTileBegin:3,
                parallaxSizeA: 2,
                parallaxSizeB: 2);

            return testScene;
        }
    }
}
