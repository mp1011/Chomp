using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class RocketEnemyController : EnemyController
    {
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bulletControllers;
        private const int Speed = 40;
        private const int VSpeed = 20;
        private const int Brake = 2;
        private const int BulletSpeed = 32;
        private WorldSprite _player;

        private GameBit _thrust;
        private LowNibble _thrustCount;

        public RocketEnemyController(SpriteTileIndex index, ChompGameModule gameModule, WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Rocket, index, gameModule, memoryBuilder)
        {
            _player = player;
            _bulletControllers = bulletControllers;

            _thrust = new GameBit(_state.Address, Bit.Bit4, memoryBuilder.Memory);
            _thrustCount = new LowNibble(_state.Address, memoryBuilder.Memory);
            Palette = 0;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _state.Value = 0;
            Palette = 0;
        }

        protected override void UpdateBehavior()
        {
            if (!_thrust.Value)
            {
                if (WorldSprite.X > _player.X)
                {
                    _motion.XSpeed = -Speed;
                    GetSprite().FlipX = true;
                }
                else
                {
                    _motion.XSpeed = Speed;
                    GetSprite().FlipX = false;
                }

                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 0;
                _motion.XAcceleration = Brake;
                _motion.YAcceleration = Brake;

                if (WorldSprite.Y < _player.Y)
                    _motion.YSpeed = VSpeed;
                else if (WorldSprite.Y > _player.Y + 8)
                    _motion.YSpeed = -VSpeed;

                _thrust.Value = true;
            }
            else if (_thrustCount.Value <= 3)
            {
                if (_motion.XSpeed == 0)
                {
                    _thrustCount.Value++;
                    if(_thrustCount.Value < 3)
                        _thrust.Value = false;
                }
                else if (_levelTimer.IsMod(16))
                {
                    CreateSmoke();
                }
            }
            else
            {
                Palette = 3;
                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 0;
                if (_levelTimer.IsMod(16))
                    _thrustCount.Value++;

                if(_thrustCount.Value == 8)
                    Explode();
            }

            _motionController.Update();
        }

        private void Explode()
        {
            Destroy();
            _audioService.PlaySound(ChompAudioService.Sound.Lightning);

            ShootBullet(-BulletSpeed, -BulletSpeed);
            ShootBullet(BulletSpeed, -BulletSpeed);
            ShootBullet(-BulletSpeed, BulletSpeed);
            ShootBullet(BulletSpeed, BulletSpeed);
        }

        private void ShootBullet(int x, int y)
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.Palette = 3;
            bullet.Motion.XSpeed = x;
            bullet.Motion.YSpeed = y;
            bullet.WorldSprite.X = WorldSprite.X;
            bullet.WorldSprite.Y = WorldSprite.Y;
        }

        private void CreateSmoke()
        {
            var smoke = _bulletControllers.TryAddNew();
            if (smoke == null)
                return;

            if(GetSprite().FlipX)
                smoke.WorldSprite.X = WorldSprite.X + 8;
            else
                smoke.WorldSprite.X = WorldSprite.X - 4;

            smoke.WorldSprite.Y = WorldSprite.Y;
            smoke.Smoke();
        }
    }
}
