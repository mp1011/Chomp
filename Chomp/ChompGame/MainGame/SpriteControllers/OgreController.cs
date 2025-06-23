using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class OgreController : EnemyController
    {
        private readonly WorldSprite _player;
        private readonly CollisionDetector _collisionDetector;
        private readonly ICollidableSpriteControllerPool _bulletControllers;
        protected override int PointsForEnemy => 250;
        public OgreController(ICollidableSpriteControllerPool bulletControllers, SpriteTileIndex index, ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, WorldSprite player) 
            : base(SpriteType.Ogre, index, gameModule, memoryBuilder)
        {
            _player = player;
            _collisionDetector = gameModule.CollissionDetector;
            _bulletControllers = bulletControllers;
            Palette = SpritePalette.Enemy1;
        }
        protected override void OnSpriteCreated(Sprite sprite)
        {
            _motion.TargetXSpeed = _motionController.WalkSpeed;
            _motion.XSpeed = _motionController.WalkSpeed;
            _motion.XAcceleration = _motionController.WalkAccel;
            sprite.Tile2Offset = 1;
            _hitPoints.Value = 2;
            _stateTimer.Value = 0;
        }

        protected override void UpdateActive()
        {
            if (WorldSprite.X > _player.X)
                _motion.TargetXSpeed = -_motionController.WalkSpeed;
            else
                _motion.TargetXSpeed = _motionController.WalkSpeed;

            _motionController.Update();
            var collision = _collisionDetector.DetectCollisions(WorldSprite, _motion);
            _motionController.AfterCollision(collision);

            if((collision.HitLeftWall || collision.HitRightWall) && collision.IsOnGround)
            {
                _motion.YSpeed = -_motionController.JumpSpeed;
            }

            if (WorldSprite.XDistanceTo(_player) < 20 && _levelTimer.Value.IsMod(16))
                ThrowFireball();
        }

        private void ThrowFireball()
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
            bullet.WorldSprite.X = WorldSprite.X;
            bullet.WorldSprite.Y = WorldSprite.Y;
            bullet.WorldSprite.FlipX = WorldSprite.FlipX;
        }
    }
}
