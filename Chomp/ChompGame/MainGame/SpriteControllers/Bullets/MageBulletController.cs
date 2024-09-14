using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class MageBulletController : ActorController, ICollidesWithPlayer, ICollidableSpriteController
    {
        private AcceleratedMotion _motion;
        private readonly WorldSprite _player;
        private GameByte _state;

        public MageBulletController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, SpriteTileIndex tileIndex,
            WorldSprite player)
            : base(SpriteType.BossBullet, gameModule, memoryBuilder, tileIndex)
        {
            _player = player;
            _motion = new AcceleratedMotion(gameModule.LevelTimer, memoryBuilder);
            Palette = SpritePalette.Fire;
            _state = memoryBuilder.AddByte();
        }

        public override IMotion Motion => _motion;

        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        public bool CollidesWithBomb(WorldSprite bomb) => false;
        public bool CollidesWithPlayer(PlayerController player) => player.CollidesWith(WorldSprite);
        public BombCollisionResponse HandleBombCollision(WorldSprite player) => BombCollisionResponse.None;
        public CollisionResult HandlePlayerCollision(WorldSprite player)
        {
            return CollisionResult.HarmPlayer;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            TurnTowardPlayer();
            _motion.XAcceleration = 10;
            _motion.YAcceleration = 10;
            _state.Value = 0;
        }

        protected override void UpdateActive()
        {
            if (_levelTimer.Value.IsMod(16))
                TurnTowardPlayer();

            _state.Value++;
            _motion.Apply(WorldSprite);

            if (_state.Value == 0)
                Destroy();
        }

        private void TurnTowardPlayer()
        {
            int speed = 30 + (_state.Value);
            if (speed > 50)
                speed = 50;

            _motion.TargetTowards(WorldSprite, _player.Center, speed);
        }

    }
}
