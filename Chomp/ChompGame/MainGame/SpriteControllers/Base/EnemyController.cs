﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    abstract class EnemyController : ActorController, ICollidesWithPlayer, ICollidesWithBomb, ICollidableSpriteController
    {
        protected LowNibble _stateTimer;
        protected readonly ChompAudioService _audioService;
        private ScenePartsDestroyed _scenePartsDestroyed;
        private StatusBar _statusBar;

        protected MaskedByte _hitPoints;
        protected ActorMotionController _motionController;
        protected AcceleratedMotion _motion;
        public override IMotion Motion => _motion;

        protected EnemyController(SpriteType spriteType,
            SpriteTileIndex index,
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder) : base(spriteType, gameModule, memoryBuilder,index)
        {
            _audioService = gameModule.AudioService;
            _scenePartsDestroyed = gameModule.ScenePartsDestroyed;
            _statusBar = gameModule.StatusBar;
            _stateTimer = new LowNibble(memoryBuilder);
            memoryBuilder.AddByte();

            //todo, unused bits
            _hitPoints = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right3, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            _motionController = new ActorMotionController(gameModule, memoryBuilder, spriteType, WorldSprite);
            _motion = _motionController.Motion;
        }

        protected override void UpdateDying()
        {
            if (_levelTimer.Value.IsMod(4))
                _stateTimer.Value--;

            if (_stateTimer.Value == 0)
            {
                if (_hitPoints.Value == 0)
                {
                    _statusBar.AddToScore(100); //todo - score per enemy type                    
                    WorldSprite.Destroy();
                    _scenePartsDestroyed.SetDestroyed(DestructionBitOffset);
                }
                else
                {
                    WorldSprite.Status = WorldSpriteStatus.Active;
                }
            }
        }

        public CollisionResult HandlePlayerCollision(WorldSprite player)
        {
            return CollisionResult.HarmPlayer;
        }

        protected override void HandleFall()
        {
            Destroy();
        }

        public bool HandleBombCollision(WorldSprite player)
        {
            if (WorldSprite.Status != WorldSpriteStatus.Active || _hitPoints.Value == 0)
                return false;

            _hitPoints.Value--;
            Motion.Stop();
            GetSprite().Palette = 3;
            WorldSprite.Status = WorldSpriteStatus.Dying;
            _stateTimer.Value = 8;
            _audioService.PlaySound(ChompAudioService.Sound.Break);

            return true;
        }
    }
}
