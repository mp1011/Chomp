namespace ChompGame.MainGame.SpriteControllers.Base
{
    enum BombCollisionResponse
    {
        None,
        Destroy,
        Bounce
    }

    interface ICollidesWithBomb
    {
        BombCollisionResponse HandleBombCollision(WorldSprite player);
    }
}
