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

        void InitializeSprite();

        void Update();

        byte DestructionBitOffset { get; set; }

        FallCheck FallCheck { get; set; }

        byte Palette { get; }
    }

    interface ICollidableSpriteController : ISpriteController, ICollidesWithPlayer, ICollidesWithBomb
    {
        bool CollisionEnabled { get; }
    }
}
