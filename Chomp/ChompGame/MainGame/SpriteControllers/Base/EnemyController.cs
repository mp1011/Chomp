using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    abstract class EnemyController : ActorController, ICollidesWithPlayer, ICollidesWithBomb, IEnemyOrBulletSpriteController
    {
        protected readonly ChompAudioService _audioService;
        private ScenePartsDestroyed _scenePartsDestroyed;

        protected EnemyController(SpriteType spriteType, 
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder) : base(spriteType, gameModule, memoryBuilder)
        {
            _audioService = gameModule.AudioService;
            _scenePartsDestroyed = gameModule.ScenePartsDestroyed;
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
                 _state.Value++;

                if (_state.Value == (int)State.Destroyed)
                {
                    WorldSprite.Destroy();
                    _scenePartsDestroyed.SetDestroyed(DestructionBitOffset);
                }
            }
            else
                UpdateBehavior();
        }

        protected abstract void UpdateBehavior();

        public void HandlePlayerCollision(MovingWorldSprite player)
        {
        }

        public bool HandleBombCollision(MovingWorldSprite player)
        {
            if (_state.Value >= (int)State.Dying)
                return false;

            _state.Value = (int)State.Dying;

            if (WorldSprite.Status == WorldSpriteStatus.Active)
            {
                GetSprite().Palette = 3;
            }

            Motion.Stop();

            _audioService.PlaySound(ChompAudioService.Sound.Noise);
            return true;
        }
    }
}
