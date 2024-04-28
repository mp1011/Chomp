using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SceneModels.SceneParts;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class TurretBulletController : ActorController, ICollidesWithPlayer, ICollidableSpriteController
    {
        private const byte STATUS_IDLE = 0;
        private const byte STATUS_FIRE = 64;
        private const byte STATUS_EXPLODE = 200;

        private GameByte _state;
        private GameByte _scenePartIndex;
        private ChompGameModule _gameModule;
        private IMotionController _motionController;
        private readonly CollisionDetector _collisionDetector;
        private readonly ChompAudioService _audioService;

        public byte ScenePartIndex
        {
            get => _scenePartIndex.Value;
            set => _scenePartIndex.Value = value;
        }

        public TurretBulletController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, SpriteTileIndex tileIndex) 
            : base(SpriteType.LizardBullet, gameModule, memoryBuilder, tileIndex)
        {
            _gameModule = gameModule;
            _collisionDetector = gameModule.CollissionDetector;
            _audioService = gameModule.AudioService;
            _motionController = new SimpleMotionController(memoryBuilder, WorldSprite,
               new SpriteDefinition(SpriteType.LizardBullet, memoryBuilder.Memory));
            _state = memoryBuilder.AddByte();
            _scenePartIndex = memoryBuilder.AddByte();
            Palette = SpritePalette.Fire;
        }

        private void SetStartingPosition()
        {
            var scenePart = _gameModule
                .CurrentScenePartHeader
                .GetTurretScenePart(ScenePartIndex, _gameModule.CurrentScene, _gameModule.Specs);

            WorldSprite.X = scenePart.TileX * _gameModule.Specs.TileWidth;
            WorldSprite.Y = (scenePart.TileY+ Constants.StatusBarTiles) * _gameModule.Specs.TileWidth;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _state.Value = STATUS_IDLE;
        }

        protected override void UpdateActive()
        {
            var sprite = GetSprite();

//            if (_levelTimer.IsMod(16))
                _state.Value++;

            if(_state.Value < STATUS_FIRE)
            {
                WorldSprite.Visible = false;
                _motionController.Motion.XSpeed = 0;
                _motionController.Motion.YSpeed = 0;
            }
            else if(_state.Value == STATUS_FIRE)
            {
                _audioService.PlaySound(ChompAudioService.Sound.Fireball);
                WorldSprite.TileIndex = SpriteTileIndex.Extra1;
                WorldSprite.Visible = true;
                _motionController.Motion.XSpeed = _motionController.Speed;
                sprite.Tile = _spriteTileTable.GetTile(SpriteTileIndex.Extra1);
                SetStartingPosition();
                _state.Value++;
            }
            else if(_state.Value >= STATUS_EXPLODE)
            {
                WorldSprite.TileIndex = SpriteTileIndex.Explosion;
            }

            _motionController.Update();

            var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite, _motionController.Motion);
            if (collisionInfo.XCorrection != 0 || collisionInfo.YCorrection != 0)            
                Explode();            
        }

        public void Explode()
        {
            if (_state.Value >= STATUS_EXPLODE)
                return;

            Palette = SpritePalette.Fire;
            GetSprite().Palette = SpritePalette.Fire;

            _audioService.PlaySound(ChompAudioService.Sound.Break);
            _state.Value = STATUS_EXPLODE;
            _motionController.Motion.XSpeed = 0;
            _motionController.Motion.YSpeed = 0;
        }

        public CollisionResult HandlePlayerCollision(WorldSprite player)
        {
            if (_state.Value >= 40)
                return CollisionResult.HarmPlayer;

            WorldSprite.Status = WorldSpriteStatus.Dying;
            _state.Value = 41;
            _motionController.Motion.XSpeed = 0;
            _motionController.Motion.YSpeed = 0;

            return CollisionResult.HarmPlayer;
        }

        public BombCollisionResponse HandleBombCollision(WorldSprite player) => BombCollisionResponse.None;
        public bool CollidesWithPlayer(PlayerController player) => player.CollidesWith(WorldSprite);
        public bool CollidesWithBomb(WorldSprite bomb) => false;
    }
}
