using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteControllers;

namespace ChompGame.MainGame
{
    class RewardsModule : Module
    {
        private readonly ChompAudioService _audioService;
        private GameInteger _nextBomb;
        private GameInteger _nextLife;
        private GameByte _timer;

        public RewardsModule(MainSystem mainSystem) : base(mainSystem)
        {
            _audioService = mainSystem.GetModule<ChompAudioService>();
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _nextBomb = memoryBuilder.AddInteger();
            _nextLife = memoryBuilder.AddInteger();
            _timer = memoryBuilder.AddByte();
        }

        public override void OnStartup()
        {
            _nextBomb.Value = 500;
            _nextLife.Value = 2000;
        }

        public void GiveRewards(int score, SceneSpriteControllers spriteControllers)
        {
            if(_timer.Value == 0 && score >= _nextBomb.Value)
            {
                _audioService.PlaySound(ChompAudioService.Sound.Reward);
                _timer.Value = 30;
            }

            if(_timer.Value > 0)
            {
                _timer.Value--;
                if(_timer.Value == 0)
                {
                    if (_nextBomb.Value < 2000)
                        _nextBomb.Value += 500;
                    else
                        _nextBomb.Value += 1000;

                    var bomb = spriteControllers.BombControllers.TryAddNew(0);
                    if(bomb != null)
                    {
                        bomb.WorldSprite.X = spriteControllers.Player.WorldSprite.X;
                        bomb.WorldSprite.Y = spriteControllers.Player.WorldSprite.Y - 24;
                    }

                }
            }
        }
    }
}
