using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class WavingBossBulletController : ActorController, ICollidesWithPlayer, ICollidableSpriteController
    {
        private GameByte _initialX;
        private GameByte _state;
        private AcceleratedMotion _motion;
        private readonly CollisionDetector _collisionDetector;
        private readonly ChompAudioService _audioService;
        private readonly DynamicBlockController _dynamicBlockController;
        private readonly Specs _specs;

        public AcceleratedMotion AcceleratedMotion => _motion;

        public WavingBossBulletController(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            bool destroyOnCollision,
            SpriteType spriteType = SpriteType.BossBullet) : base(spriteType, gameModule, memoryBuilder, SpriteTileIndex.Extra1)
        {
            _collisionDetector = gameModule.CollissionDetector;
            _audioService = gameModule.AudioService;
            _dynamicBlockController = gameModule.DynamicBlocksController;
            _specs = gameModule.Specs;
            _state = memoryBuilder.AddByte();
            _initialX = memoryBuilder.AddByte();
            _motion = new AcceleratedMotion(gameModule.LevelTimer, memoryBuilder);

            Palette = SpritePalette.Fire;
        }

        protected override bool DestroyWhenOutOfBounds => true;

        protected override void UpdateActive() 
        {
            if(_initialX.Value == 0 || WorldSprite.X > _initialX + 8)
            {
                _motion.TargetXSpeed = -40;
                _motion.XAcceleration = 7;
            }
            else if (WorldSprite.X < _initialX - 8)
            {
                _motion.TargetXSpeed = 40;
                _motion.XAcceleration = 7;
            }

            if (_initialX.Value == 0)            
                _initialX.Value = (byte)WorldSprite.X;
            
            _motion.Apply(WorldSprite);

            if(WorldSprite.Y >= 52)
                Explode();
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            Palette = SpritePalette.Fire;
            GetSprite().Palette = SpritePalette.Fire;
        }

        protected override void UpdateDying()
        {
            if (_levelTimer.Value.IsMod(4))
                _state.Value++;

            var sprite = GetSprite();
            if (_state.Value == 50)
            {
                var spriteBounds = WorldSprite.Bounds;

                _dynamicBlockController.SpawnCoins(
                    new Rectangle(
                        spriteBounds.X,
                        spriteBounds.Y - _specs.TileHeight,
                        spriteBounds.Width,
                        spriteBounds.Height * 2));

                Destroy();
            }
            else if (_state.Value > 40)
            {
                var baseTile = _spriteTileTable.GetTile(SpriteTileIndex.Explosion);
                sprite.Tile = (byte)(baseTile + (_levelTimer.Value % 2));
            }
        }

        public void Explode()
        {
            Palette = SpritePalette.Fire;
            GetSprite().Palette = SpritePalette.Fire;

            _audioService.PlaySound(ChompAudioService.Sound.Break);

            WorldSprite.Status = WorldSpriteStatus.Dying;
            _state.Value = 41;
            _motion.XSpeed = 0;
            _motion.YSpeed = 0;
        }

        public void Smoke()
        {
            if (_state.Value >= 40)
                return;

            Palette = SpritePalette.Platform;
            GetSprite().Palette = SpritePalette.Platform;

            WorldSprite.Status = WorldSpriteStatus.Dying;
            _state.Value = 41;
            _motion.XSpeed = 0;
            _motion.YSpeed = 0;
        }

        public CollisionResult HandlePlayerCollision(WorldSprite player)
        {
            Explode();
            return CollisionResult.HarmPlayer;
        }

        public BombCollisionResponse HandleBombCollision(WorldSprite player) => BombCollisionResponse.None;
        public bool CollidesWithPlayer(PlayerController player) => player.CollidesWith(WorldSprite);
        public bool CollidesWithBomb(WorldSprite bomb) => false;
    }
}
