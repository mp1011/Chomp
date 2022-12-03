using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    abstract class EnemyController : ActorController, ICollidesWithPlayer, ICollidesWithBomb
    {
        protected readonly ChompAudioService _audioService;

        protected EnemyController(SpriteType spriteType, 
            SpritesModule spritesModule, 
            WorldScroller scroller,
            ChompAudioService audioService,
            SystemMemoryBuilder memoryBuilder, 
            GameByte levelTimer, 
            Bit stateMask = Bit.Right6) : base(spriteType, spritesModule, scroller, memoryBuilder, levelTimer, stateMask)
        {
            _audioService = audioService;
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
                    WorldSprite.Destroy();
            }
            else
                UpdateBehavior();
        }

        protected abstract void UpdateBehavior();

        public void HandlePlayerCollision(WorldSprite player)
        {
        }

        public void HandleBombCollision(WorldSprite player)
        {
            if (_state.Value >= (int)State.Dying)
                return;

            _state.Value = 40;

            if (WorldSprite.Status == WorldSpriteStatus.Active)
            {
                GetSprite().Palette = 3;
            }

            Motion.Stop();

            _audioService.PlaySound(ChompAudioService.Sound.Noise);
        }
    }
}
