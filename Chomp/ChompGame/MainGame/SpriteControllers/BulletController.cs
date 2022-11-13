using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BulletController : ActorController, ICollidesWithPlayer
    {
        public BulletController(
            SpritesModule spritesModule,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder, 
            SpriteType spriteType) : base(spriteType, spritesModule, memoryBuilder, levelTimer)
        { 
        }

        public void Update()
        {
            var sprite = GetSprite();

            if (_state.Value == 0)
            {
                _movingSpriteController.Motion.YSpeed = 0;
                _movingSpriteController.Motion.TargetYSpeed = 0;

                if (sprite.FlipX)
                {
                    _movingSpriteController.Motion.XSpeed = -_movingSpriteController.WalkSpeed;
                    _movingSpriteController.Motion.TargetXSpeed = -_movingSpriteController.WalkSpeed;
                }
                else
                {
                    _movingSpriteController.Motion.XSpeed = _movingSpriteController.WalkSpeed;
                    _movingSpriteController.Motion.TargetXSpeed = _movingSpriteController.WalkSpeed;
                }
            }

            _movingSpriteController.Update();

            if(_levelTimer.Value.IsMod(4))
                _state.Value++;
            
            if (_state.Value == 40 || _state.Value == 60)
            {
                sprite.Tile = 0;
                SpriteIndex = 255;
            }
            else  if (_state.Value > 40)
            {
                sprite.Tile = (byte)(6 + (_levelTimer.Value % 3));
            }
        }

        public void HandleCollision(WorldSprite player)
        {
            if (_state.Value >= 40)
                return;

            _state.Value = 41;
            _movingSpriteController.Motion.XSpeed = 0;
            _movingSpriteController.Motion.YSpeed = 0;
            _movingSpriteController.Motion.TargetXSpeed = 0;
            _movingSpriteController.Motion.TargetYSpeed = 0;
        }
    }
}
