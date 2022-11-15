﻿using ChompGame.Data;
using ChompGame.Extensions;
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

        private GameBit _bombPickup;

        public bool IsHoldingBomb => _bombPickup.Value;

        public PlayerController(
            SpritesModule spritesModule, 
            InputModule inputModule, 
            StatusBar statusBar,
            ChompAudioService audioService,
            CollisionDetector collisionDetector,
            GameByte levelTimer,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Player, spritesModule, memoryBuilder, levelTimer, Bit.Right4)
        {
            _statusBar = statusBar;
            _audioService = audioService;

            _inputModule = inputModule;
            _collisionDetector = collisionDetector;

            _bombPickup = new GameBit(_state.Address, Bit.Bit7, memoryBuilder.Memory);

            SpriteIndex = 0;
        }

        public void Update()
        {
            if(_state.Value > 0)
            {
                var sprite = WorldSprite.GetSprite();               
                
                if(_levelTimer.IsMod(4))
                {
                    sprite.Visible = !sprite.Visible;
                }

                if (_levelTimer.IsMod(8))
                {
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

            if (collisionInfo.IsOnGround && _inputModule.Player1.AKey == GameKeyState.Pressed)
            {
                _audioService.PlaySound(ChompAudioService.Sound.Jump);
                Motion.YSpeed = -_movingSpriteController.JumpSpeed;
            }
        }

        public void CheckBombPickup(SpriteControllerPool<BombController> bombs)
        {
            if (IsHoldingBomb)
                return;

            if (_inputModule.Player1.BKey != GameKeyState.Pressed)
                return;

            bombs.Execute(p =>
            {
                if (p.WorldSprite.Bounds.Intersects(WorldSprite.Bounds))
                {
                    _bombPickup.Value = true;
                    p.SetPickup();
                }
            });
        }

        public void CheckBombThrow(BombController bombController)
        {
            if (_inputModule.Player1.BKey != GameKeyState.Pressed)
                return;

            _bombPickup.Value = false;
            bombController.DoThrow();
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
                    _state.Value = 15;
                    p.HandlePlayerCollision(WorldSprite);

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
