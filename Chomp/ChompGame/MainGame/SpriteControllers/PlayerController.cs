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
    class PlayerController : ActorController
    {
        private SceneDefinition _scene;
        private readonly Specs _specs;
        private const byte _recoilSpeed = 30;
        private const byte _fallSpringSpeed = 120;

        private readonly StatusBar _statusBar;
        private readonly ChompAudioService _audioService;
        private readonly CollisionDetector _collisionDetector;
        private readonly InputModule _inputModule;
        private readonly DynamicBlockController _dynamicBlockController;

        private GameBit _bombPickup;
        private GameBit _openingDoor;
        private GameBit _onPlatform;

        private MaskedByte _afterHitInvincibility;

        public bool IsHoldingBomb => _bombPickup.Value;

        public PlayerController(
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Player, gameModule, memoryBuilder)
        {
            _scene = gameModule.CurrentScene;
            _specs = gameModule.Specs;
            _statusBar = gameModule.StatusBar;
            _audioService = gameModule.AudioService;
            _dynamicBlockController = gameModule.DynamicBlocksController;

            _inputModule = gameModule.InputModule;
            _collisionDetector = gameModule.CollissionDetector;

            _onPlatform = new GameBit(_state.Address, Bit.Bit5, memoryBuilder.Memory);
            _openingDoor = new GameBit(_state.Address, Bit.Bit6, memoryBuilder.Memory);
            _bombPickup = new GameBit(_state.Address, Bit.Bit7, memoryBuilder.Memory);

            _afterHitInvincibility = new MaskedByte(_state.Address, Bit.Right4, memoryBuilder.Memory);
            SpriteIndex = 0;
        }

        public void SetInitialPosition(NBitPlane levelMap, ExitType lastExitType,
            SceneSpriteControllers sceneSpriteControllers)
        {
            switch(lastExitType)
            {
                case ExitType.Right:
                    SetInitialPosition_Horizontal(levelMap, new Point(0, levelMap.Height - 1),  1);
                    break;
                case ExitType.Left:
                    SetInitialPosition_Horizontal(levelMap, new Point(levelMap.Width-1, levelMap.Height - 1), -1);
                    break;
                case ExitType.Bottom:
                    SetInitialPosition_Vertical(levelMap, new Point(levelMap.Width/2, 1), 1);
                    break;
                case ExitType.DoorForward:
                    SetInitialPosition_Door(levelMap, sceneSpriteControllers, ExitType.DoorBack);
                    break;
                case ExitType.DoorBack:
                    SetInitialPosition_Door(levelMap, sceneSpriteControllers, ExitType.DoorForward);
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }


        private void SetInitialPosition_Door(NBitPlane levelMap, SceneSpriteControllers sceneSpriteControllers, ExitType enterDoor)
        {
            var door = sceneSpriteControllers.GetDoorPosition(enterDoor);
            WorldSprite.X = door.X;
            WorldSprite.Y = door.Y;
        }

        private void SetInitialPosition_Horizontal(NBitPlane levelMap, Point startPoint, int xOffset)
        {
            Point pt = startPoint;

            while(levelMap[pt.X,pt.Y] >= _scene.CollidableTileBeginIndex)
            {
                pt = new Point(pt.X, pt.Y - 1);
                if(pt.Y == 0)
                {
                    pt = new Point(pt.X + xOffset, startPoint.Y);
                }               
            }

            WorldSprite.X = (pt.X * _specs.TileWidth) + xOffset;
            WorldSprite.Y = (pt.Y + 1) * _specs.TileHeight;
        }

        private void SetInitialPosition_Vertical(NBitPlane levelMap, Point startPoint, int yOffset)
        {
            Point pt = startPoint;

            while (levelMap[pt.X, pt.Y] != 0)
            {
                pt = new Point(pt.X, pt.Y + yOffset);                
            }

            WorldSprite.X = pt.X * _specs.TileWidth;
            WorldSprite.Y = pt.Y * _specs.TileHeight;
        }

        public void OnOpenDoor()
        {
            _openingDoor.Value = true;
            var sprite = WorldSprite.GetSprite();
            sprite.Visible = false;
        }

        protected override void HandleFall()
        {
            if (_statusBar.Health == 0)
                return;

            Motion.YSpeed = -_fallSpringSpeed;
            WorldSprite.Y = _specs.ScreenHeight;
            HarmPlayer(2);
        }

        protected override void UpdateActive()
        {
            if(_statusBar.Health == 0)
            {
                GetSprite().FlipY = true;
                Motion.TargetYSpeed = _movingSpriteController.FallSpeed;
                Motion.XSpeed = 0;
                Motion.TargetXSpeed = 0;
                _movingSpriteController.Update();
                return;
            }

            if (_openingDoor.Value)
            {
                _inputModule.OnLogicUpdate();
                return;
            }

            if(_afterHitInvincibility.Value > 0)
            {
                var sprite = WorldSprite.GetSprite();               
                
                if(_levelTimer.IsMod(4))
                {
                    sprite.Visible = !sprite.Visible;
                }

                if (_levelTimer.IsMod(8))
                {
                    _afterHitInvincibility.Value--;
                }

                if (_afterHitInvincibility.Value == 0)
                {
                    sprite.Visible = true;
                }
            }

            _movingSpriteController.Update();
            var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite);
            _movingSpriteController.AfterCollision(collisionInfo);

            if (collisionInfo.DynamicBlockCollision)
            {
                int coinsCollected = _dynamicBlockController.HandleCoinCollision(collisionInfo, WorldSprite);
                _statusBar.AddToScore((uint)(coinsCollected * 25));
            }
            
            _inputModule.OnLogicUpdate();

            
            if(collisionInfo.XCorrection < 0 && Motion.XSpeed > 0)
            {
                Motion.XSpeed = 0;
            }

            if (collisionInfo.XCorrection > 0 && Motion.XSpeed < 0)
            {
                Motion.XSpeed = 0;
            }

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

            if ((collisionInfo.IsOnGround || _onPlatform.Value) 
                && _inputModule.Player1.AKey == GameKeyState.Pressed)
            {
                _audioService.PlaySound(ChompAudioService.Sound.Jump);
                Motion.YSpeed = -_movingSpriteController.JumpSpeed;
            }

            _onPlatform.Value = false;
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

        public void CheckEnemyOrBulletCollisions(IEnemyOrBulletSpriteControllerPool sprites)
        {
            if (sprites == null)
                return;

            if (_afterHitInvincibility.Value > 0)
                return;

            sprites.Execute(p =>
            {
                if(p.WorldSprite.Bounds.Intersects(WorldSprite.Bounds))
                {
                    p.HandlePlayerCollision(WorldSprite);

                    if(WorldSprite.FlipX)
                        Motion.XSpeed = _recoilSpeed;
                    else
                        Motion.XSpeed = -_recoilSpeed;

                    Motion.YSpeed = -_recoilSpeed;

                    HarmPlayer(1);
                }
            });
        }

        public void HarmPlayer(byte damage)
        {
            if (_statusBar.Health == 0)
                return;

            _afterHitInvincibility.Value = 15;
           
            if (damage >= _statusBar.Health)
            {
                Motion.YSpeed = -_movingSpriteController.JumpSpeed;
                
                if(_statusBar.Health > 0)
                {
                    _statusBar.Health = 0;
                    _statusBar.Lives--;
                }
            }
            else
            {
                _audioService.PlaySound(ChompAudioService.Sound.PlayerHit);
                _statusBar.Health -= damage;
            }
        }

        public void OnPlatformCollision(CollisionInfo c)
        {
            _movingSpriteController.AfterCollision(c);
            _onPlatform.Value = c.IsOnGround;
        }
    }
}
