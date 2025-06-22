using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class BombController : ActorController
    {
        private readonly ScenePartsDestroyed _scenePartsDestroyed;
        protected readonly CollisionDetector _collisionDetector;
        protected readonly PlayerController _playerController;
        private readonly DynamicBlockController _dynamicBlockController;
        protected readonly GameBit _isThrown;
        protected readonly GameByteEnum<BombState> _bombState;


        protected IMotionController _motionController;
        protected AcceleratedMotion _motion;

        public AcceleratedMotion AcceleratedMotion => _motion;

        protected override bool DestroyWhenFarOutOfBounds => true;
        protected override bool DestroyWhenOutOfBounds => true;

        protected enum BombState : byte
        {
            Idle=0,
            IdleMid=5,
            RiseBegin=10,
            RiseEnd=14,
            Explode=20,
            Destroy=30

        }

        public BombController(
                ChompGameModule gameModule,
                PlayerController playerController,
                SystemMemoryBuilder memoryBuilder)
            : base(SpriteType.Bomb, gameModule, memoryBuilder, SpriteTileIndex.Bomb)
        {
            _scenePartsDestroyed = gameModule.ScenePartsDestroyed;
            _collisionDetector = gameModule.CollissionDetector;
            _playerController = playerController;
            _dynamicBlockController = gameModule.DynamicBlocksController;
            var state = memoryBuilder.AddByte();
            _bombState = new GameByteEnum<BombState>(new MaskedByte(state.Address, Bit.Right5, memoryBuilder.Memory));
            _isThrown = new GameBit(state.Address, Bit.Bit5, memoryBuilder.Memory);

            var motionController = new ActorMotionController(gameModule.SpritesModule, gameModule.SpriteTileTable,
                gameModule.LevelTimer, memoryBuilder, new SpriteDefinition(SpriteType.Bomb, memoryBuilder.Memory), WorldSprite);

            _motionController = motionController;
            _motion = motionController.Motion;
        }

        public bool IsCarried => _bombState.Value == BombState.RiseEnd;
        protected override void UpdateActive()
        {
            if(_bombState.Value >= BombState.Explode)
            {
                _motion.Stop();

                if (WorldSprite.Status == WorldSpriteStatus.Active)
                {
                    var sprite = GetSprite();
                    sprite.Palette = SpritePalette.Fire;

                    var baseTile = _spriteTileTable.GetTile(SpriteTileIndex.Explosion);
                    sprite.Tile = (byte)(baseTile + (_levelTimer.Value % 2));
                }

                if (_levelTimer.IsMod(2))
                    _bombState.Value++;

                if(_bombState.Value >= BombState.Destroy)
                {
                    Destroy();
                }
            }
            else if (_bombState.Value < BombState.RiseBegin)
            {
                int ySpeed = _motion.YSpeed;
                _motionController.Update();

                var collisionInfo = _collisionDetector.DetectCollisions(WorldSprite, _motion);
                _motionController.AfterCollision(collisionInfo);

                if(collisionInfo.DynamicBlockCollision 
                    &&_dynamicBlockController.HandleBombCollision(collisionInfo))
                { 
                    _motion.XSpeed = -_motion.XSpeed/2;
                    _motion.TargetXSpeed = 0;
                    _motion.XAcceleration = 10;
                }
                
                if ((int)_bombState.Value < 2 && (collisionInfo.YCorrection < 0 || collisionInfo.IsOnGround))
                {
                    if (_isThrown.Value)
                    {
                        if(Math.Abs(_motion.CurrentMotion.XSpeed) >= 30)
                        {
                            _motion.XSpeed = _motion.CurrentMotion.XSpeed / 2;
                            _motion.XAcceleration = 1;
                            _motion.TargetXSpeed = 0;
                        }
                        else
                        {
                            _isThrown.Value = false;
                            _motion.XSpeed = 0;
                            _motion.TargetXSpeed = 0;
                        }
                    }
                    else
                    {
                        _motion.XSpeed = 0;
                        _motion.TargetXSpeed = 0;
                    }
                    _motion.YSpeed = (int)(-(ySpeed * 0.5));
                    _bombState.Value++;
                }
            }
            else if(_bombState.Value < BombState.RiseEnd)
            {
                WorldSprite.X = _playerController.WorldSprite.X;
                WorldSprite.Y = _playerController.WorldSprite.Y - ((byte)_bombState.Value - 10);
                _bombState.Value++;
            }
            else
            {
                WorldSprite.X = _playerController.WorldSprite.X;
                WorldSprite.Y = _playerController.WorldSprite.Y - 5;

                _playerController.CheckBombThrow(this);
            }
        }

        protected override void HandleFall()
        {
            Destroy();
        }

        public void SetCarried()
        {
            _bombState.Value = BombState.RiseEnd;
            WorldSprite.X = _playerController.WorldSprite.X;
            WorldSprite.Y = _playerController.WorldSprite.Y - 5;
        }

        public virtual void DoThrow()
        {
            _motion.XAcceleration = 0;
            _bombState.Value = BombState.Idle;
            _isThrown.Value = true;

            if (_playerController.Motion.YSpeed < 0)
            {
                _motion.YSpeed = -50;
                _motion.XSpeed = _playerController.WorldSprite.FlipX ? -30 : 30;
            }
            else
            {
                _motion.YSpeed = -10;
                _motion.XSpeed = _playerController.WorldSprite.FlipX ? -50 : 50;
            }
        }

        public void CheckEnemyCollisions(ICollidableSpriteControllerPool sprites)
        {
            if (sprites == null)
                return;

            if (!_isThrown)
                return;

            sprites.Execute(p =>
            {
                if (!p.CollisionEnabled || !p.CollidesWithBomb(WorldSprite))
                    return;

                switch (p.HandleBombCollision(WorldSprite))
                {
                    case BombCollisionResponse.Destroy:
                        _isThrown.Value = false;
                        _bombState.Value = BombState.Explode;
                        break;
                    case BombCollisionResponse.Bounce:
                        _isThrown.Value = false;
                        if (_motion.YSpeed > 0)
                            _motion.YSpeed = -Motion.YSpeed * 2;

                        _motion.SetXSpeed(_motion.XSpeed * -1);
                        _bombState.Value = BombState.Idle;
                        break;
                }
            });
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _motion.XSpeed = 0;
            _motion.YSpeed = 0;
            _bombState.Value = BombState.Idle;
            _isThrown.Value = false;
        }

        public void SetPickup()
        {
            _bombState.Value = BombState.RiseBegin;
            _motion.XSpeed = 0;
            _motion.YSpeed = 0;
            _motion.TargetXSpeed = 0;
            _motion.TargetYSpeed = 0;

            if(DestructionBitOffset != 255)
                _scenePartsDestroyed.SetDestroyed(DestructionBitOffset);
        }

        protected override void UpdateDying() { }
    }
}
