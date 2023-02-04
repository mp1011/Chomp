namespace ChompGame.MainGame.SpriteControllers.Base
{
    interface ICollidesWithBomb
    {
        bool HandleBombCollision(MovingWorldSprite player);
    }
}
