﻿using ChompGame.Data;
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
    class ChompFinalBossController : EnemyController
    {
        private readonly PaletteModule _paletteModule;
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

        public ChompFinalBossController(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _graphics = gameModule.GameSystem.CoreGraphicsModule;
            _paletteModule = gameModule.PaletteModule;
            _exitModule = gameModule.ExitsModule;
            _music = gameModule.MusicModule;
            _scroller = gameModule.WorldScroller;
            _bullets = bullets;
            _player = player;

            _phase = new GameByteEnum<Phase>(new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right6, memoryBuilder.Memory));
             memoryBuilder.AddByte();

            _specs = gameModule.Specs;
            Palette = SpritePalette.Enemy1;

            GameDebug.Watch1 = new DebugWatch("T", () => _stateTimer.Value);
        }

        protected override void BeforeInitializeSprite()
        {
            SpriteIndex = _spritesModule.GetFreeSpriteIndex();
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _music.CurrentSong = MusicModule.SongName.None;
            _hitPoints.Value = 1;
            SetPhase(Phase.Init);
            sprite.Priority = true;
        }

        protected override void UpdateActive()
        {
            _music.CurrentSong = MusicModule.SongName.FinalBossPart3;
            if (_phase.Value ==  Phase.Init)
            {
            }           
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {            
            return false;
        }

        private void FireBullet()
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            bullet.WorldSprite.Center = WorldSprite.Center;

            var target = _player.Center;

            bullet.AcceleratedMotion.SetYSpeed(6);
            
        }

        private void FadeIn()
        {
            var targetSpritePalette = _paletteModule.GetPalette(PaletteKey.BlueGrayEnemy);
         
            var spritePalette = _graphics.GetSpritePalette(SpritePalette.Enemy1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 2);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 3);
        }



        private void SetPhase(Phase phase)
        {
            _phase.Value = phase;
            _stateTimer.Value = 0;

            if (phase == Phase.Init)
            {
                _music.CurrentSong = MusicModule.SongName.FinalBossPart3;
            }
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
            bullet.Explode();
        }
    }
}
