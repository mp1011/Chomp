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
        protected ExitsModule _exitsModule;
        protected override int PointsForEnemy => 2000;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        protected override bool AlwaysActive => true;

        protected abstract string BossTiles { get; }
        protected abstract string BlankBossTiles { get; }
        protected override bool DestroyBombOnCollision => true;
        public LevelBossController(ChompGameModule gameModule,
            WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers,
            SystemMemoryBuilder memoryBuilder)
            : base(SpriteType.LevelBoss, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _player = player;
            _gameModule = gameModule;
            _exitsModule = gameModule.ExitsModule;
            _worldScroller = gameModule.WorldScroller;
            _dynamicBlockController = gameModule.DynamicBlocksController;
            _bulletControllers = bulletControllers;
            _musicModule = gameModule.MusicModule;
            _graphicsModule = gameModule.GameSystem.CoreGraphicsModule;
            _tileModule = gameModule.TileModule;
            _paletteModule = gameModule.PaletteModule;
            _position = gameModule.BossBackgroundHandler.BossPosition;
            _bossBackgroundHandler = gameModule.BossBackgroundHandler;

            var motionController = new ActorMotionController(gameModule, memoryBuilder, SpriteType.LevelBoss, WorldSprite);
            _motion = motionController.Motion;

            _motionController = motionController;

            Palette = SpritePalette.Enemy1;

            gameModule.CollissionDetector.BossBgHandling = true;
        }

        protected override void HandleFall() { }
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
            var tileStart = 16;

            _worldScroller.ModifyTiles((nt, _) =>
            { 
                nt.SetFromString(0, 13, tileStart,
                BossTiles);
            });
        }

        protected void HideOffscreenBossTiles(int leftMin, int rightMax, int bossWidth)
        {
            SetBossTiles();

            if (WorldSprite.X < leftMin)
            {
                int adjust = _player.X > 75 ? 1 : 0;
                var leftHide = ((leftMin - WorldSprite.X) / _gameModule.Specs.TileWidth) + adjust;

                _worldScroller.ModifyTiles((nt, _) =>
                {
                    nt.ForEach(Point.Zero, new Point(leftHide, nt.Height - 4), (x, y, b) =>
                          nt[x, y] = 0);
                });
            }
            else if (WorldSprite.X > rightMax)
            {
                int adjust = _player.X < 75 ? 1 : 0;
                var rightHide = ((WorldSprite.X - rightMax) / _gameModule.Specs.TileWidth) + adjust;

                _worldScroller.ModifyTiles((nt, _) =>
                {
                    nt.ForEach(new Point(bossWidth - rightHide,0), new Point(bossWidth, nt.Height - 4), (x, y, b) =>
                          nt[x, y] = 0);
                });
            }
        }

        protected void EraseBossTiles()
        {
            _tileModule.NameTable.SetFromString(0, 15, 0,
                   BlankBossTiles);
        }

        
        protected void HideBoss()
        {
            var spritePalette = _graphicsModule.GetSpritePalette(SpritePalette.Enemy1);
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
                WorldSprite.X + 4 + _rng.RandomItem(-8, -4, 0, 4, 8),
                WorldSprite.Y + 0 + _rng.RandomItem(-8, -4, 0, 4, 8));            
        }

        protected void CreateExplosion(int x, int y, bool decorative=false)
        {
            var explosion = _bulletControllers.TryAddNew();
            if (explosion != null)
            {
                explosion.Explode();
                explosion.CollisionEnabled = !decorative;
                explosion.WorldSprite.X = x;
                explosion.WorldSprite.Y = y;
            }
        }

        protected void PositionFreeCoinBlocksNearPlayer()
        {
            PositionFreeCoinBlocks(_player.X);
        }

        protected void PositionFreeCoinBlocks(int x)
        {
            _dynamicBlockController.PositionFreeCoinBlocksNearPlayer(
                (byte)(x / _spritesModule.Specs.TileWidth),
                (byte)(_spritesModule.Specs.NameTableHeight - 6));
        }
    }
}
