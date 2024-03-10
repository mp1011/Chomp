using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class OgreBulletController : ActorController, ICollidesWithPlayer, ICollidableSpriteController
    {
        private AcceleratedMotion _motion;
        private RandomModule _rng;

        public OgreBulletController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, SpriteTileIndex tileIndex)
            : base(SpriteType.OgreBullet, gameModule, memoryBuilder, tileIndex)
        {
            _motion = new AcceleratedMotion(gameModule.LevelTimer, memoryBuilder);
            _rng = gameModule.RandomModule;
            Palette = 3;
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
            _motion.TargetYSpeed = 20;
            _motion.YSpeed = -30;
            _motion.YAcceleration = 2;
            _motion.XSpeed = 0;
        }

        protected override void UpdateActive()
        {
            if(_motion.XSpeed == 0)
            {
                _motion.XSpeed = _rng.RandomItem(2, 8, 16);
                if (WorldSprite.FlipX)
                    _motion.XSpeed *= -1;
                _motion.TargetXSpeed = _motion.XSpeed;
            }

            _motion.Apply(WorldSprite);

            if(_levelTimer.IsMod(8))
            {
                if (!WorldSprite.FlipX && !WorldSprite.FlipY)
                    WorldSprite.FlipX = true;
                else if (WorldSprite.FlipX && !WorldSprite.FlipY)
                    WorldSprite.FlipY = true;
                else if (WorldSprite.FlipX && WorldSprite.FlipY)
                    WorldSprite.FlipX = false;
                else
                    WorldSprite.FlipY = false;
            }
        }
    }
}
