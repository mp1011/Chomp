namespace ChompGame.Data
{
    interface ISpriteController
    {
        byte SpriteIndex { get; set; }

        Sprite GetSprite();
    }
}
