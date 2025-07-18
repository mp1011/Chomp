﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class PlayerController : ActorController
    {
        private RasterInterrupts _rasterInterrupts;
        protected ActorMotionController _motionController;

        public override IMotion Motion => AcceleratedMotion;
        public AcceleratedMotion AcceleratedMotion => _motionController.Motion;

        private SceneDefinition _scene;
        protected readonly Specs _specs;
        private const byte _recoilSpeed = 30;
        private const byte _fallSpringSpeed = 120;

        protected readonly StatusBar _statusBar;
        private readonly ChompAudioService _audioService;
        private readonly CollisionDetector _collisionDetector;
        protected readonly InputModule _inputModule;
        private readonly DynamicBlockController _dynamicBlockController;

        private GameBit _bombPickup;
        private GameBit _openingDoor;
        protected GameBit _bossDead;

        private GameBit _onPlatform;
        private GameBit _onPlane;

        private MaskedByte _afterHitInvincibility;

        public bool IsHoldingBomb
        {
            get => _bombPickup.Value;
            set => _bombPickup.Value = value;
        }

        public PlayerController(
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder,
            SpriteType spriteType = SpriteType.Player,
            SpriteTileIndex spriteTileIndex = SpriteTileIndex.Player) 
            : base(spriteType, gameModule, memoryBuilder, spriteTileIndex)
        {
            _rasterInterrupts = gameModule.RasterInterrupts;
            var state = memoryBuilder.AddByte();
            _scene = gameModule.CurrentScene;
            _specs = gameModule.Specs;
            _statusBar = gameModule.StatusBar;
            _audioService = gameModule.AudioService;
            _dynamicBlockController = gameModule.DynamicBlocksController;

            _inputModule = gameModule.InputModule;
            _collisionDetector = gameModule.CollissionDetector;

            _onPlane = new GameBit(state.Address, Bit.Bit4, memoryBuilder.Memory);
            _onPlatform = new GameBit(state.Address, Bit.Bit5, memoryBuilder.Memory);

            _openingDoor = new GameBit(state.Address, Bit.Bit6, memoryBuilder.Memory);
            _bossDead = new GameBit(state.Address, Bit.Bit6, memoryBuilder.Memory);

            _bombPickup = new GameBit(state.Address, Bit.Bit7, memoryBuilder.Memory);

            _afterHitInvincibility = new MaskedByte(state.Address, Bit.Right4, memoryBuilder.Memory);

            _motionController = new ActorMotionController(
                gameModule.SpritesModule, 
                gameModule.SpriteTileTable, 
                gameModule.LevelTimer, 
                memoryBuilder, 
                new SpriteDefinition(spriteType, memoryBuilder.Memory),
                WorldSprite);
            SpriteIndex = 0;

            Palette = SpritePalette.Player;
        }

        protected override void BeforeInitializeSprite()
        {
            _rasterInterrupts.SetPlayer(WorldSprite);
            base.BeforeInitializeSprite();
        }

        public virtual void OnBossDead() { }

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
                    SetInitialPosition_Vertical(levelMap, new Point(levelMap.Width/2, 3), 1);
                    WorldSprite.Y++;
                    break;
                case ExitType.Top:
                    SetInitialPosition_Vertical(levelMap, new Point(levelMap.Width / 2, levelMap.Height - 2), -1);
                    Motion.YSpeed = -_motionController.JumpSpeed;
                    CollisionEnabled = false;
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

            while(CollisionDetector.IsTileSolid(levelMap[pt.X,pt.Y]) || CollisionDetector.IsTileSolid(levelMap[pt.X, pt.Y-2]))
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

            while (CollisionDetector.IsTileSolid(levelMap[pt.X, pt.Y]))
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
            Visible = false;
        }

        public void OnPlaneEnter()
        {
            _onPlane.Value = true;
            var sprite = WorldSprite.GetSprite();
            sprite.SizeY = 1;
            sprite.FlipX = false;
            _audioService.PlaySound(ChompAudioService.Sound.PlaneTakeoff);
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
                AcceleratedMotion.TargetYSpeed = _motionController.FallSpeed;
                Motion.XSpeed = 0;
                AcceleratedMotion.TargetXSpeed = 0;
                _motionController.Update();
                return;
            }

            if (_openingDoor.Value)
            {
                _inputModule.OnLogicUpdate();
                return;
            }

            if(_onPlane.Value)
            {
                return;
            }

            CheckAfterHitInvincability();
           
            _motionController.Update();

            if (!CollisionEnabled && Motion.YSpeed >= 0)
                CollisionEnabled = true;

            var collisionInfo = CollisionEnabled ?
                _collisionDetector.DetectCollisions(WorldSprite, Motion)
                : new CollisionInfo();

            _motionController.AfterCollision(collisionInfo);

            if (collisionInfo.DynamicBlockCollision)
            {
                int coinsCollected = _dynamicBlockController.HandleCoinCollision(collisionInfo, WorldSprite);

                if(!_scene.IsMidBossScene && !_scene.IsLevelBossScene)
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
                AcceleratedMotion.TargetXSpeed = _motionController.WalkSpeed;
                AcceleratedMotion.XAcceleration = _motionController.WalkAccel;
            }
            else if (_inputModule.Player1.LeftKey.IsDown())
            {
                AcceleratedMotion.TargetXSpeed = -_motionController.WalkSpeed;
                AcceleratedMotion.XAcceleration = _motionController.WalkAccel;
            }
            else
            {
                AcceleratedMotion.TargetXSpeed = 0;
                AcceleratedMotion.XAcceleration = _motionController.BrakeAccel;
            }

            if(GameDebug.EnableFly && _inputModule.Player1.UpKey == GameKeyState.Down)
            {
                WorldSprite.Y -= 2;
                Motion.YSpeed = 0;
            }

            if ((collisionInfo.IsOnGround || _onPlatform.Value) 
                && _inputModule.Player1.AKey == GameKeyState.Pressed)
            {
                _audioService.PlaySound(ChompAudioService.Sound.Jump);
                Motion.YSpeed = -_motionController.JumpSpeed;
            }

            _onPlatform.Value = false;
        }

        protected void CheckAfterHitInvincability()
        {
            if (_afterHitInvincibility.Value > 0)
            {
                var sprite = WorldSprite.GetSprite();

                if (_levelTimer.IsMod(4))
                {
                    Visible = !Visible;
                }

                if (_levelTimer.IsMod(8))
                {
                    _afterHitInvincibility.Value--;
                }

                if (_afterHitInvincibility.Value == 0)
                {
                    Visible = true;
                }
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

        public virtual bool CollidesWith(IWithBounds other) =>
            other.Bounds.Intersects(WorldSprite.Bounds);

        public void CheckEnemyOrBulletCollisions(ICollidableSpriteControllerPool sprites)
        {
            if (sprites == null)
                return;

            if (_afterHitInvincibility.Value > 0)
                return;

            sprites.Execute(p =>
            {
                if(p.CollisionEnabled && p.CollidesWithPlayer(this))
                {
                    if (p.HandlePlayerCollision(WorldSprite) == CollisionResult.HarmPlayer)
                    {
                        if (WorldSprite.FlipX)
                            Motion.XSpeed = _recoilSpeed;
                        else
                            Motion.XSpeed = -_recoilSpeed;

                        Motion.YSpeed = -_recoilSpeed;

                        HarmPlayer(1);
                    }
                }
            });
        }

        public void HarmPlayer(byte damage)
        {
            if (_openingDoor.Value)
                return;

            if (_statusBar.Health == 0)
                return;

            _afterHitInvincibility.Value = 15;
           
            if (damage >= _statusBar.Health)
            {
                Motion.YSpeed = -_motionController.JumpSpeed;
                _motionController.Motion.YAcceleration = _motionController.GravityAccel;

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
            _motionController.AfterCollision(c);
            _onPlatform.Value = c.IsOnGround;
        }
    }
}
