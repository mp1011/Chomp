using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class RocketEnemyController2 : EnemyController
    {
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bulletControllers;
        private readonly WorldScroller _worldScroller;

        private const int Speed = 40;
        private const int Brake = 2;
        private const int BulletSpeed = 20;
        private WorldSprite _player;
        
        public void AfterSpawn(ISpriteControllerPool pool)
        {

        }

        public RocketEnemyController2(SpriteTileIndex index, ChompGameModule gameModule, WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Rocket, index, gameModule, memoryBuilder)
        {
            _player = player;
            _bulletControllers = bulletControllers;
            _worldScroller = gameModule.WorldScroller;
            Palette = SpritePalette.Platform;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {            
            _stateTimer.Value = 0;
            Palette = SpritePalette.Platform;
            _hitPoints.Value = 1;           
        }

        protected override void UpdateActive()
        {
            if(_stateTimer.Value==0)
            {
                _motion.XAcceleration = Brake;
                _motion.YAcceleration = Brake;

                _motion.TargetXSpeed = 0;

                if ((WorldSprite.Y/4).IsMod(2))
                {
                    _motion.XSpeed = -Speed;
                    WorldSprite.X = _worldScroller.ViewPane.Right + 4;
                    GetSprite().FlipX = true;
                }
                else
                {
                    _motion.XSpeed = Speed;
                    WorldSprite.X = _worldScroller.ViewPane.Left - 4;
                    GetSprite().FlipX = false;
                }

                _stateTimer.Value = 1;
            }
            else if(_stateTimer.Value == 1 && _motion.XSpeed == 0 && _levelTimer.IsMod(24))
            {
                _motion.TargetTowards(WorldSprite, _player.Center, Speed);
                _stateTimer.Value = 2;
            }
            else if(_stateTimer.Value >= 2 && _stateTimer.Value < 6)
            {
                GetSprite().FlipX = _motion.XSpeed < 0;
                if (_levelTimer.Value.IsMod(32))
                    _stateTimer.Value++;
                if (WorldSprite.Y > _player.Y)
                    _stateTimer.Value = 6;
            }
            else if(_stateTimer.Value >= 6 && _stateTimer.Value < 10)
            {
                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 0;
                GetSprite().Palette = SpritePalette.Fire;
                if (_levelTimer.Value.IsMod(16))
                    _stateTimer.Value++;
            }
            else if (_stateTimer.Value >= 10)
            {
                Explode();
            }

            if (_levelTimer.IsMod(16) && _stateTimer.Value < 10)
                CreateSmoke();

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

            bullet.Palette = SpritePalette.Fire;
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
