using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class PlayerController : ActorController
    {
        private const byte _recoilSpeed = 30;
        private readonly StatusBar _statusBar;
        private readonly ChompAudioService _audioService;
        private readonly CollisionDetector _collisionDetector;
        private readonly InputModule _inputModule;

        public PlayerController(
            SpritesModule spritesModule, 
            InputModule inputModule, 
            StatusBar statusBar,
            ChompAudioService audioService,
            CollisionDetector collisionDetector,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Player, spritesModule, memoryBuilder, levelTimer)
        {
            _statusBar = statusBar;
            _audioService = audioService;

            _inputModule = inputModule;
            _collisionDetector = collisionDetector;

            SpriteIndex = 0;
        }

        public void Update()
        {
            if(_state.Value > 0)
            {
                var sprite = WorldSprite.GetSprite();               
                
                if((_levelTimer.Value % 4) == 0)
                {
                    sprite.Visible = !sprite.Visible;
                    _state.Value--;
                }

                if (_state.Value == 0)
                {
                    sprite.Visible = true;
                }
            }

            _movingSpriteController.Update();
            _inputModule.OnLogicUpdate();

            var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite, 14); //todo, hard-coding

            if (_inputModule.Player1.RightKey.IsDown())
            {
                Motion.TargetXSpeed = _movingSpriteController.WalkSpeed;
                Motion.XAcceleration = _movingSpriteController.WalkAccel;
            }
            else if (_inputModule.Player1.LeftKey.IsDown())
            {
                Motion.TargetXSpeed = -_movingSpriteController.WalkSpeed;
                Motion.XAcceleration = _movingSpriteController.WalkAccel;
            }
            else
            {
                Motion.TargetXSpeed = 0;
                Motion.XAcceleration = _movingSpriteController.BrakeAccel;
            }

            if(_inputModule.Player1.BKey == GameKeyState.Pressed)
            {
                _audioService.PlaySound(ChompAudioService.Sound.Test);
            }

            if (collisionInfo.IsOnGround && _inputModule.Player1.AKey == GameKeyState.Pressed)
            {
                _audioService.PlaySound(ChompAudioService.Sound.Jump);
                Motion.YSpeed = -_movingSpriteController.JumpSpeed;
            }
        }

        public void CheckEnemyOrBulletCollisions<T>(SpriteControllerPool<T> sprites)
            where T : class, ISpriteController, ICollidesWithPlayer
        {
            if (_state.Value > 0)
                return;

            sprites.Execute(p =>
            {
                if(p.WorldSprite.Bounds.Intersects(WorldSprite.Bounds))
                {
                    _state.Value = 60;
                    p.HandleCollision(WorldSprite);

                    if(WorldSprite.FlipX)
                        Motion.XSpeed = _recoilSpeed;
                    else
                        Motion.XSpeed = -_recoilSpeed;

                    Motion.YSpeed = -_recoilSpeed;
                    _audioService.PlaySound(ChompAudioService.Sound.PlayerHit);
                    _statusBar.Health--;
                }
            });
        }
    }
}
