using ChompGame.Data;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    interface ICollidesWithPlayer
    {
        CollisionResult HandlePlayerCollision(WorldSprite player);
    }
}
