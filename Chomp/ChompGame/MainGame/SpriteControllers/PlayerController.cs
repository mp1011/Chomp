using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class PlayerController
    {
        private const byte _recoilSpeed = 30;
        private readonly StatusBar _statusBar;
        private readonly ChompAudioService _audioService;
        private readonly CollisionDetector _collisionDetector;
        private readonly MovingSpriteController _walkingSpriteController;
        private readonly InputModule _inputModule;
        private readonly GameByte _hitTimer;
        private readonly GameByte _levelTimer;
        private readonly SpriteDefinition _spriteDefinition;

        public WorldSprite WorldSprite => _walkingSpriteController.WorldSprite;
        public AcceleratedMotion Motion => _walkingSpriteController.Motion;

        public PlayerController(
            SpritesModule spritesModule, 
            InputModule inputModule, 
            StatusBar statusBar,
            ChompAudioService audioService,
            CollisionDetector collisionDetector,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder)
        {
            _statusBar = statusBar;
            _audioService = audioService;
            _levelTimer = levelTimer;
            _spriteDefinition = new SpriteDefinition(SpriteType.Player, memoryBuilder.Memory);

            _walkingSpriteController = new MovingSpriteController(
               spritesModule,
               levelTimer,
               memoryBuilder,
               spriteIndex: 0,
               spriteDefinition: _spriteDefinition);

            _inputModule = inputModule;
            _collisionDetector = collisionDetector;
            _hitTimer = memoryBuilder.AddByte();
        }

        public void Update()
        {
            if(_hitTimer.Value > 0)
            {
                _hitTimer.Value--;

                var sprite = WorldSprite.GetSprite();
                if (_hitTimer.Value == 0)
                {
                    sprite.Palette = _spriteDefinition.Palette;
                }
                else if((_levelTimer.Value % 4) == 0)
                {
                    sprite.Palette = sprite
                        .Palette
                        .Toggle(3, _spriteDefinition.Palette);
                }
            }

            var motion = _walkingSpriteController.Motion;

            _walkingSpriteController.Update();
            _inputModule.OnLogicUpdate();

            var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite, 14); //todo, hard-coding

            if (_inputModule.Player1.RightKey.IsDown())
            {
                motion.TargetXSpeed = _walkingSpriteController.WalkSpeed;
                motion.XAcceleration = _walkingSpriteController.WalkAccel;
            }
            else if (_inputModule.Player1.LeftKey.IsDown())
            {
                motion.TargetXSpeed = -_walkingSpriteController.WalkSpeed;
                motion.XAcceleration = _walkingSpriteController.WalkAccel;
            }
            else
            {
                motion.TargetXSpeed = 0;
                motion.XAcceleration = _walkingSpriteController.BrakeAccel;
            }

            if(_inputModule.Player1.BKey == GameKeyState.Pressed)
            {
                _audioService.PlaySound(ChompAudioService.Sound.Test);
            }

            if (collisionInfo.IsOnGround && _inputModule.Player1.AKey == GameKeyState.Pressed)
            {
                _audioService.PlaySound(ChompAudioService.Sound.Jump);
                motion.YSpeed = -_walkingSpriteController.JumpSpeed;
            }
        }

        public void CheckEnemyOrBulletCollisions<T>(SpriteControllerPool<T> sprites)
            where T : class, ISpriteController, ICollidesWithPlayer
        {
            if (_hitTimer.Value > 0)
                return;

            sprites.Execute(p =>
            {
                if(p.WorldSprite.Bounds.Intersects(WorldSprite.Bounds))
                {
                    _hitTimer.Value = 100;
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
