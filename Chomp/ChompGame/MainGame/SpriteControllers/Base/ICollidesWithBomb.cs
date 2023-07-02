namespace ChompGame.MainGame.SpriteControllers.Base
{
    interface ICollidesWithBomb
    {
        bool HandleBombCollision(WorldSprite player);
    }
}
