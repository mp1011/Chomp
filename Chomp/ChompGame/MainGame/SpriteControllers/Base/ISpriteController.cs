using ChompGame.Data;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    interface ISpriteController
    {
        byte SpriteIndex { get; set; }

        WorldSpriteStatus Status { get; set; }

        Sprite GetSprite();

        WorldSprite WorldSprite { get; }

        void InitializeSprite(byte palette);

        void Update();
    }

    interface IEnemyOrBulletSpriteController : ISpriteController, ICollidesWithPlayer, ICollidesWithBomb
    {

    }
}
