using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss6Controller : EnemyController
    {       
        public const int BossHp = GameDebug.BossTest ? 1 : 3;
        private readonly WorldScroller _scroller;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly MusicModule _music;
        private readonly ExitsModule _exitModule;
        private readonly Specs _specs;
        private WorldSprite _player;
        private CoreGraphicsModule _graphics;
        
        enum Phase : byte 
        {
            Init,          
        }
      
        private GameByteEnum<Phase> _phase;

        protected override bool DestroyBombOnCollision => true;
        protected override bool AlwaysActive => true;

        public ChompBoss6Controller(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _graphics = gameModule.GameSystem.CoreGraphicsModule;
            _exitModule = gameModule.ExitsModule;
            _music = gameModule.MusicModule;
            _scroller = gameModule.WorldScroller;
            _bullets = bullets;
            _player = player;

            _phase = new GameByteEnum<Phase>(new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right6, memoryBuilder.Memory));
             memoryBuilder.AddByte();

            _specs = gameModule.Specs;
            Palette = SpritePalette.Enemy1;

            memoryBuilder.AddByte();
            GameDebug.Watch1 = new DebugWatch("T", () => _stateTimer.Value);
        }

        protected override void BeforeInitializeSprite()
        {
            SpriteIndex = _spritesModule.GetFreeSpriteIndex();
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _music.CurrentSong = MusicModule.SongName.None;
            _hitPoints.Value = BossHp;
            SetPhase(Phase.Init);
            sprite.Priority = true;
        }

        protected override void UpdateActive()
        {
            

        }

        private void SetPhase(Phase phase)
        {
            _phase.Value = phase;
            _stateTimer.Value = 0;

           
        }

        protected override void UpdateDying()
        {
            
            

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
            bullet.Explode(true);
        }
    }
}
