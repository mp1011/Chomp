using ChompGame.Data;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    interface IAutoScrollSpriteController
    {
        byte Variation { get; set; }

        void AfterSpawn(ISpriteControllerPool pool);
    }
}
