using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class RocketEnemyController : EnemyController, IAutoScrollSpriteController
    {
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bulletControllers;
        private const int Speed = 40;
        private const int VSpeed = 20;
        private const int Brake = 2;
        private const int BulletSpeed = 32;
        private WorldSprite _player;

        private GameBit _thrust;
        private GameBit _variation;
        private MaskedByte _thrustCount;

        public byte Variation
        {
            get => (byte)(_variation.Value ? 1 : 0);
            set => _variation.Value = value == 1;
        }

        public void AfterSpawn(ISpriteControllerPool pool)
        {

        }

        public RocketEnemyController(SpriteTileIndex index, ChompGameModule gameModule, WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Rocket, index, gameModule, memoryBuilder)
        {
            _player = player;
            _bulletControllers = bulletControllers;

            _thrust = new GameBit(_stateTimer.Address, Bit.Bit4, memoryBuilder.Memory);
            _thrustCount = new MaskedByte(_stateTimer.Address, Bit.Right3, memoryBuilder.Memory);
            _variation = new GameBit(_stateTimer.Address, Bit.Bit5, memoryBuilder.Memory);
            Palette = 0;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            bool variation = _variation.Value;
            _stateTimer.Value = 0;
            _thrust.Value = false;
            _thrustCount.Value = 0;
            Palette = 0;
            _hitPoints.Value = 1;
            _variation.Value = variation;
        }

        protected override void UpdateActive()
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

                if (_variation.Value)
                {
                    _motion.TargetXSpeed = 0;
                    _motion.TargetYSpeed = 0;
                }
                else
                {
                    _motion.TargetXSpeed = -Speed;
                    _motion.TargetYSpeed = 0;
                }

                _motion.XAcceleration = Brake;
                _motion.YAcceleration = Brake;

                if (WorldSprite.Y < _player.Y)
                    _motion.YSpeed = VSpeed;
                else if (WorldSprite.Y > _player.Y + 8)
                    _motion.YSpeed = -VSpeed;

                _thrust.Value = true;
            }
            else if (_thrustCount.Value <= 2)
            {
                if (_variation.Value && _motion.XSpeed == 0)
                {
                    _thrustCount.Value++;
                    if(_thrustCount.Value < 3)
                        _thrust.Value = false;
                }
                else if(!_variation.Value && WorldSprite.X < 16)
                {
                    _thrustCount.Value = 4;
                }
                else if (_levelTimer.IsMod(16))
                {
                    CreateSmoke();
                }
            }
            else
            {
                GetSprite().Palette = 3;
                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 0;
                _motion.XAcceleration = Brake * 2;
                if (_levelTimer.IsMod(16))
                    _thrustCount.Value++;

                if(_thrustCount.Value == 7)
                    Explode();
            }

            _motionController.Update();
        }

        private void Explode()
        {
            Destroy();
            _audioService.PlaySound(ChompAudioService.Sound.Lightning);

            if (_variation.Value)
            {
                ShootBullet(-BulletSpeed, -BulletSpeed);
                ShootBullet(BulletSpeed, -BulletSpeed);
                ShootBullet(-BulletSpeed, BulletSpeed);
                ShootBullet(BulletSpeed, BulletSpeed);
            }
            else
            {
                ShootBullet(BulletSpeed, -BulletSpeed);
                ShootBullet(BulletSpeed, 0);
                ShootBullet(BulletSpeed, BulletSpeed);
            }
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
