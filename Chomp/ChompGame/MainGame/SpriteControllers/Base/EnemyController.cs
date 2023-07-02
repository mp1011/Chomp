using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.MotionControllers;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    abstract class EnemyController : ActorController, ICollidesWithPlayer, ICollidesWithBomb, IEnemyOrBulletSpriteController
    {
        protected GameByte _state;
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
            _state = memoryBuilder.AddByte();

            //todo, unused bits
            _hitPoints = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right3, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            _motionController = new ActorMotionController(gameModule, memoryBuilder, spriteType, WorldSprite);
            _motion = _motionController.Motion;
        }

        public enum State
        {
            Dying=40,
            Destroyed=63
        }

        protected override void UpdateActive()
        {      
            if (_state.Value >= (int)State.Dying)
            {
                
                if (_state.Value == (int)State.Destroyed)
                {
                    if (_hitPoints.Value == 0)
                    {
                        if (HandleDestroy())
                        {
                            Destroy();
                            _scenePartsDestroyed.SetDestroyed(DestructionBitOffset);
                        }
                    }
                    else
                    {
                        _state.Value = 0;
                        GetSprite().Palette = _palette.Value;
                    }
                }
                else
                {
                    _state.Value++;
                }
            }
            else
                UpdateBehavior();
        }

        protected virtual bool HandleDestroy() => true;

        protected abstract void UpdateBehavior();

        public void HandlePlayerCollision(WorldSprite player)
        {
        }

        protected override void HandleFall()
        {
            Destroy();
        }

        public bool HandleBombCollision(WorldSprite player)
        {
            if (_state.Value >= (int)State.Dying)
                return false;

            _state.Value = (int)State.Dying;

            if (WorldSprite.Status == WorldSpriteStatus.Active)
            {
                GetSprite().Palette = 3;
            }

            if (_hitPoints.Value == 0)
            {
                _statusBar.AddToScore(100); //todo - score per enemy type
                Motion.Stop();
                _audioService.PlaySound(ChompAudioService.Sound.Break);
            }
            else
            {
                _hitPoints.Value--;
                _audioService.PlaySound(ChompAudioService.Sound.Break);
            }

            return true;
        }
    }
}
