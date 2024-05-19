using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss3Controller : EnemyController
    {
        public const int BossHp = 3;

        private readonly WorldSprite _player;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly MusicModule _music;
        private readonly WorldScroller _scroller;
        private readonly Specs _specs;
        enum Phase : byte 
        {
            Init=0,
        }

        private GameByteEnum<Phase> _phase;

        protected override bool DestroyBombOnCollision => true;

        public ChompBoss3Controller(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _player = player;
            _music = gameModule.MusicModule;
            _bullets = bullets;

            _phase = new GameByteEnum<Phase>(memoryBuilder.AddByte());
            _scroller = gameModule.WorldScroller;
            _specs = gameModule.Specs;
 
            Palette = SpritePalette.Enemy1;
        }

        protected override void BeforeInitializeSprite()
        {
            SpriteIndex = _spritesModule.GetFreeSpriteIndex();
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = BossHp;
            _stateTimer.Value = 0;
        }

        protected override void UpdateActive()
        {
          
        }

        private void FireBullet(int xSpeed)
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.WorldSprite.Center = WorldSprite.Center;
            bullet.Motion.YSpeed = 15;
            bullet.Motion.XSpeed= xSpeed;
        }

        protected override void UpdateDying()
        {
           
        }
    }
}
