using ChompGame.Data;
using ChompGame.MainGame.SceneModels;

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

        byte DestructionBitOffset { get; set; }

        FallCheck FallCheck { get; set; }
    }

    interface IEnemyOrBulletSpriteController : ISpriteController, ICollidesWithPlayer, ICollidesWithBomb
    {

    }
}
