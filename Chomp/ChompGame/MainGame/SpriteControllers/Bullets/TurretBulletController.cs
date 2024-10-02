using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class TurretBulletController : ActorController, ICollidesWithPlayer, ICollidableSpriteController
    {
        private const byte STATUS_IDLE = 0;
        private const byte STATUS_FIRE = 16;
        private const byte STATUS_EXPLODE = 60;

        private MaskedByte _state;
        private GameByte _scenePartIndex;
        private ChompGameModule _gameModule;
        private IMotionController _motionController;
        private readonly CollisionDetector _collisionDetector;
        private readonly ChompAudioService _audioService;

        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

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

            _state = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left6, memoryBuilder.Memory, leftShift:2);
            memoryBuilder.AddByte();

            _scenePartIndex = memoryBuilder.AddByte();
            Palette = SpritePalette.Fire;
        }

        private Direction SetStartingPosition()
        {
            var scenePart = _gameModule
                .CurrentScenePartHeader
                .GetTurretScenePart(ScenePartIndex, _gameModule.CurrentScene, _gameModule.Specs);

            WorldSprite.X = scenePart.X * _gameModule.Specs.TileWidth;
            WorldSprite.Y = (scenePart.Y + Constants.StatusBarTiles) * _gameModule.Specs.TileWidth;

            switch(scenePart.Direction)
            {
                case Direction.Up:
                    WorldSprite.Y -= _gameModule.Specs.TileHeight;
                    break;

                case Direction.Right:
                    WorldSprite.X += _gameModule.Specs.TileWidth;
                    break;

            }
            return scenePart.Direction;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _state.Value = STATUS_IDLE;
        }

        protected override void UpdateHidden()
        {
            _state.Value = 0;
            SetStartingPosition();
            WorldSprite.Show();
        }

        protected override void UpdateActive()
        {
            if (_levelTimer.IsMod(8))
                _state.Value++;

            if(_state.Value < STATUS_FIRE)
            {
                WorldSprite.Visible = false;
                _motionController.Motion.XSpeed = 0;
                _motionController.Motion.YSpeed = 0;
            }
            else if(_state.Value == STATUS_FIRE)
            {
                FireBullet();
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

        private void FireBullet()
        {
            var sprite = GetSprite();

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
            WorldSprite.TileIndex = SpriteTileIndex.Extra1;
            WorldSprite.Visible = true;
          
            sprite.Tile = _spriteTileTable.GetTile(SpriteTileIndex.Extra1);
            var direction = SetStartingPosition();
            byte spriteTile = _spriteTileTable.GetTile(WorldSprite.TileIndex);

            switch (direction)
            {
                case Direction.Right:
                    WorldSprite.Tile = spriteTile;
                    _motionController.Motion.XSpeed = _motionController.Speed;
                    _motionController.Motion.YSpeed = 0;
                    WorldSprite.FlipX = false;
                    break;
                case Direction.Left:
                    WorldSprite.Tile = spriteTile;
                    _motionController.Motion.XSpeed = -_motionController.Speed;
                    _motionController.Motion.YSpeed = 0;
                    WorldSprite.FlipX = true;
                    break;
                case Direction.Up:
                    WorldSprite.Tile = (byte)(spriteTile + 1);
                    _motionController.Motion.XSpeed = 0;
                    _motionController.Motion.YSpeed = -_motionController.Speed;
                    WorldSprite.FlipY = true;
                    break;
                default:
                    WorldSprite.Tile = (byte)(spriteTile + 1);
                    _motionController.Motion.XSpeed = 0;
                    _motionController.Motion.YSpeed = _motionController.Speed;
                    WorldSprite.FlipY = false;
                    break;
            }


            _state.Value++;
        }

        public void Explode()
        {
            if (_state.Value >= STATUS_EXPLODE)
                return;

            Palette = SpritePalette.Fire;

            var sprite = GetSprite();
            sprite.Palette = SpritePalette.Fire;
            sprite.Tile = _spriteTileTable.GetTile(SpriteTileIndex.Explosion);
            _audioService.PlaySound(ChompAudioService.Sound.Break);
            _state.Value = STATUS_EXPLODE;
            _motionController.Motion.XSpeed = 0;
            _motionController.Motion.YSpeed = 0;
        }

        public CollisionResult HandlePlayerCollision(WorldSprite player)
        {
            if (_state.Value < STATUS_FIRE)
                return CollisionResult.None;

            Explode();
            _motionController.Motion.XSpeed = 0;
            _motionController.Motion.YSpeed = 0;

            return CollisionResult.HarmPlayer;
        }

        public BombCollisionResponse HandleBombCollision(WorldSprite player) => BombCollisionResponse.None;
        public bool CollidesWithPlayer(PlayerController player) => player.CollidesWith(WorldSprite);
        public bool CollidesWithBomb(WorldSprite bomb) => false;
    }
}
