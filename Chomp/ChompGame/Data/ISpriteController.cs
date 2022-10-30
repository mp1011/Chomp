using ChompGame.MainGame;

namespace ChompGame.Data
{
    interface ISpriteController
    {
        byte SpriteIndex { get; set; }

        Sprite GetSprite();

        WorldSprite WorldSprite { get; }
    }
}
