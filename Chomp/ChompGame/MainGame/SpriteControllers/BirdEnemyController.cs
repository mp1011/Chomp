using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BirdEnemyController : EnemyController, IAutoScrollSpriteController
    {
        private const int _hoverSpeed = 20;
        private readonly WorldSprite _player;

        private GameByte _variation;
        public byte Variation
        {
            get => _variation.Value;
            set => _variation.Value = value;
        }

        protected override bool DestroyWhenFarOutOfBounds => _variation.Value > 0;
        protected override bool DestroyWhenOutOfBounds => _variation.Value > 0;

        public BirdEnemyController(
            WorldSprite player,
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteTileIndex tileIndex) 
            : base(SpriteType.Bird,
                    tileIndex,
                    gameModule,                  
                    memoryBuilder)
        {
            _player = player;
            _variation = memoryBuilder.AddByte();
            Palette = 1;
        }

        protected override void UpdateBehavior() 
        {
            _motionController.Update();

            if (_variation.Value == 0)
                UpdateBehavior_Normal();
            else
                UpdateBehavior_AutoScroll();
        }

        private void UpdateBehavior_Normal()
        {
            if ((_levelTimer % 32) == 0)
            {
                _state.Value++;

                int hoverTarget = _player.Y - 16;

                if (_state < 8)
                {
                    if (WorldSprite.Y < hoverTarget)
                        _motion.TargetYSpeed = _hoverSpeed;
                    else
                        _motion.TargetYSpeed = -_hoverSpeed;

                    WorldSprite.FlipX = _player.X < WorldSprite.X;
                }
                else if (_state >= 8 && _state < 14)
                {
                    if (WorldSprite.Y < _player.Y)
                        _motion.TargetTowards(WorldSprite, _player, _motionController.WalkSpeed);
                    else
                        _state.Value = 0;
                }
                else if (_state >= 14)
                {
                    _motion.TargetXSpeed = 0;
                    _motion.TargetYSpeed = -_hoverSpeed;

                    if (WorldSprite.Y <= hoverTarget)
                    {
                        _state.Value = 0;
                        _motion.TargetYSpeed = 0;
                    }
                }
            }
        }

        private void UpdateBehavior_AutoScroll()
        {
            if ((_levelTimer % 32) == 0)
            {
                _state.Value++;

                if (_state < 7)
                {
                    _motion.TargetTowards(WorldSprite, _player, _motionController.WalkSpeed);
                }
            }
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _motion.TargetXSpeed = 0;
            _motion.TargetYSpeed = 0;
            _motion.XSpeed = 0;
            _motion.YSpeed = 0;           
            _motion.XAcceleration = _motionController.WalkAccel;
            _motion.YAcceleration = _motionController.WalkAccel;

            _hitPoints.Value = 0;
            _state.Value = 0;
        }

        public void AfterSpawn(ISpriteControllerPool pool) { }
    }

}
