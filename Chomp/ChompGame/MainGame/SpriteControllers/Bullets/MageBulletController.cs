using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class MageBulletController : ActorController, ICollidesWithPlayer, ICollidableSpriteController
    {
        private AcceleratedMotion _motion;
       
        public MageBulletController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, SpriteTileIndex tileIndex)
            : base(SpriteType.OgreBullet, gameModule, memoryBuilder, tileIndex)
        {
            _motion = new AcceleratedMotion(gameModule.LevelTimer, memoryBuilder);
            Palette = SpritePalette.Fire;
        }

        protected override bool DestroyWhenFarOutOfBounds => true;
        protected override bool DestroyWhenOutOfBounds => true;

        public bool CollidesWithBomb(WorldSprite bomb) => false;
        public bool CollidesWithPlayer(PlayerController player) => player.CollidesWith(WorldSprite);
        public BombCollisionResponse HandleBombCollision(WorldSprite player) => BombCollisionResponse.None;
        public CollisionResult HandlePlayerCollision(WorldSprite player)
        {
            return CollisionResult.HarmPlayer;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
        }

        protected override void UpdateActive()
        {          
        }
    }
}
