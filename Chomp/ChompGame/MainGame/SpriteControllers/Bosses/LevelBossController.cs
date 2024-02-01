using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.Bosses;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    abstract class LevelBossController : EnemyController
    {
        protected abstract int BossHP { get; }

        public override IMotion Motion => _motion;

        protected readonly ChompGameModule _gameModule;
        protected DynamicBlockController _dynamicBlockController;
        protected CoreGraphicsModule _graphicsModule;
        protected EnemyOrBulletSpriteControllerPool<BossBulletController> _bulletControllers;
        protected TileModule _tileModule;
        protected PaletteModule _paletteModule;
        protected GameByteGridPoint _position;
       
        protected WorldSprite _player;
        protected GameByte _internalTimer;
        protected MusicModule _musicModule;
        protected WorldScroller _worldScroller;
        protected BossBackgroundHandler _bossBackgroundHandler;

        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        protected override bool AlwaysActive => true;

        protected abstract string BossTiles { get; }
        protected abstract string BlankBossTiles { get; }

        private GameByte _levelBossBackgroundEnd;

        public LevelBossController(ChompGameModule gameModule,
            WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.LevelBoss, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _player = player;
            _gameModule = gameModule;
            _worldScroller = gameModule.WorldScroller;
            _dynamicBlockController = gameModule.DynamicBlocksController;
            _bulletControllers = bulletControllers;
            _musicModule = gameModule.MusicModule;
            _graphicsModule = gameModule.GameSystem.CoreGraphicsModule;
            _tileModule = gameModule.TileModule;
            _paletteModule = gameModule.PaletteModule;
            _position = gameModule.BossBackgroundHandler.BossPosition;
            _levelBossBackgroundEnd = gameModule.BossBackgroundHandler.BossBackgroundEnd;
            _bossBackgroundHandler = gameModule.BossBackgroundHandler;
                     
            var motionController = new ActorMotionController(gameModule, memoryBuilder, SpriteType.LevelBoss, WorldSprite);
            _motion = motionController.Motion;

            _motionController = motionController;

            Palette = 2;
        }

        protected void SetBossBackgroundEnd(int tilesAboveBottom)
        {
            _levelBossBackgroundEnd.Value = (byte)(_spritesModule.Specs.ScreenHeight - (_spritesModule.Specs.TileHeight * tilesAboveBottom));
        }

        protected override void BeforeInitializeSprite()
        {
            _position.X = 200;
            _position.Y = 16;

            WorldSprite.Y = 80;
            WorldSprite.X = 16;

            _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.None;
            _bossBackgroundHandler.BossBgEffectValue = 0;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = (byte)BossHP;
            _stateTimer.Value = 0;
        }

    
        protected void SetBossTiles()
        {
            var tileStart = 47;
            //_tileModule.NameTable.SetFromString(0, 15, tileStart,
            //    BossTiles);

            _worldScroller.ModifyTiles((nt, _) =>
            {
                nt.SetFromString(0, 13, tileStart,
                BossTiles);
            });
        }

        protected void EraseBossTiles()
        {
            _tileModule.NameTable.SetFromString(0, 15, 0,
                   BlankBossTiles);
        }

        
        protected void HideBoss()
        {
            var spritePalette = _graphicsModule.GetSpritePalette(2);
            spritePalette.SetColor(1, 0);
            spritePalette.SetColor(2, 0);
            spritePalette.SetColor(3, 0);

            var bossPalette = _paletteModule.BgPalette2;
            bossPalette.SetColor(1, 0);
            bossPalette.SetColor(2, 0);
            bossPalette.SetColor(3, 0);
        }

        protected BossPart CreatePart(SystemMemoryBuilder memoryBuilder, SpriteDefinition spriteDefinition)
        {
            return new BossPart(_gameModule, memoryBuilder, spriteDefinition);
        }

        protected abstract void UpdatePartPositions();

        protected void CreateExplosion()
        {
            CreateExplosion(
                WorldSprite.X + _rng.RandomItem(-8, -4, 0, 4, 8),
                WorldSprite.Y + 4 + _rng.RandomItem(-8, -4, 0, 4, 8));            
        }

        protected void CreateExplosion(int x, int y)
        {
            var explosion = _bulletControllers.TryAddNew();
            if (explosion != null)
            {
                explosion.Explode();
                explosion.WorldSprite.X = x;
                explosion.WorldSprite.Y = y;
            }
        }
    }
}
