namespace ChompGame.MainGame.SpriteControllers.Base
{
    interface ICollidesWithPlayer
    {
        void HandlePlayerCollision(MovingWorldSprite player);
    }
}
