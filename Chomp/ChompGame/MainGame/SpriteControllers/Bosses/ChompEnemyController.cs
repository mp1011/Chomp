using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompEnemyController : EnemyController
    {
        private const int Speed = 20;
        private readonly PaletteModule _paletteModule;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly Specs _specs;
        private WorldSprite _player;
        private CoreGraphicsModule _graphics;
        protected override int PointsForEnemy => 1000;
        protected override bool DestroyBombOnCollision => false;
        protected override bool AlwaysActive => true;

        public ChompEnemyController(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _bullets = bullets;
            _graphics = gameModule.GameSystem.CoreGraphicsModule;
            _paletteModule = gameModule.PaletteModule;
            _player = player;
            
            _specs = gameModule.Specs;
            Palette = SpritePalette.Enemy1;

            GameDebug.Watch1 = new DebugWatch("T", () => _stateTimer.Value);
        }

        protected override void BeforeInitializeSprite()
        {
            SpriteIndex = _spritesModule.GetFreeSpriteIndex();
            _motion.TargetXSpeed = -20;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = 2;
            _motion.XAcceleration = 5;
            _motion.YAcceleration = 5;
        }

        protected override void UpdateActive()
        {
            if(_stateTimer.Value == 0 || _levelTimer.IsMod(32))
            {
                _motion.TargetXSpeed = TargetXSpeed();
                _motion.TargetYSpeed = TargetYSpeed();
            }

            _motionController.Update();

            if(_levelTimer.IsMod(8))
            {
                _stateTimer.Value++;

                int bspeed = 30;
                if(_stateTimer.Value == 2)
                {
                    FireBullet(-bspeed, -bspeed);
                    FireBullet(bspeed, -bspeed);
                    FireBullet(-bspeed, bspeed);
                    FireBullet(bspeed, bspeed);
                }
                if (_stateTimer.Value == 6)
                {
                    FireBullet(-bspeed, 0);
                    FireBullet(bspeed, 0);
                    FireBullet(0, -bspeed);
                    FireBullet(0, bspeed);
                }
            }
        }

        private int TargetXSpeed()
        {
            if (WorldSprite.X < _player.X)
                return Speed;
            else if (WorldSprite.X > _player.X + 16)
                return -Speed;
            else
                return _rng.Generate(1) == 0 ? -Speed : Speed;             
        }

        private int TargetYSpeed()
        {
            if (WorldSprite.Y < 8)
                return Speed;
            else if (WorldSprite.Y > _player.Y)
                return -Speed;
            else
                return _rng.Generate(1) == 0 ? -Speed : Speed;
        }

        private void FireBullet(int x, int y)
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnTimer = true;
            bullet.DestroyOnCollision = false;

            bullet.WorldSprite.X = WorldSprite.X + 4;
            bullet.WorldSprite.Y = WorldSprite.Y + 4;
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);


            bullet.AcceleratedMotion.YAcceleration = 2;
            bullet.AcceleratedMotion.XAcceleration = 2;
            bullet.AcceleratedMotion.TargetXSpeed = x;
            bullet.AcceleratedMotion.TargetYSpeed = y;
        }

        private void FadeIn()
        {
            var targetSpritePalette = _paletteModule.GetPalette(PaletteKey.BlueGrayEnemy);
         
            var spritePalette = _graphics.GetSpritePalette(SpritePalette.Enemy1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 2);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 3);
        }

        private void CreateExplosion()
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.EnsureInFrontOf(this);
            bullet.WorldSprite.Center = WorldSprite.Center.Add(
                _rng.RandomItem(-4, -2, 0, 2, 4),
                _rng.RandomItem(-4, 2, 0, 2, 4));

            bullet.Motion.YSpeed = 0;
            bullet.Motion.XSpeed = 0;
            bullet.Explode();
        }
    }
}
