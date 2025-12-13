using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers;

namespace ChompGame.MainGame
{
    class RewardsModule : Module
    {
        private int[] _extraLifeScores = new int[] { 2000, 5000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000 };
        private const int FlashDuration = 60;
        private const byte CoinsUntilRewardForLevel = GameDebug.QuickReward ? 1 : 20;
        private const byte CoinsUntilRewardForBoss = GameDebug.QuickReward ? 1 : 10;

        private readonly Specs _specs;
        private readonly ChompAudioService _audioService;
        private readonly SpritesModule _spritesModule;
        private readonly RandomModule _randomModule;
        private SceneDefinition _currentScene;
        private GameByte _nextReward;
        private GameByte _timer;
        private GameByte _rewardSpriteIndex;

        public RewardsModule(MainSystem mainSystem) : base(mainSystem)
        {
            _audioService = mainSystem.GetModule<ChompAudioService>();
            _spritesModule = mainSystem.GetModule<SpritesModule>();
            _randomModule = mainSystem.GetModule<RandomModule>();
            _specs = mainSystem.Specs;
        }

        public byte CoinsUntilReward => (_currentScene != null && (_currentScene.IsLevelBossScene || _currentScene.IsMidBossScene)) ? CoinsUntilRewardForBoss : CoinsUntilRewardForLevel;
       
        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _nextReward = memoryBuilder.AddByte();
            _timer = memoryBuilder.AddByte();
            _rewardSpriteIndex = memoryBuilder.AddByte();
        }

        public void SetScene(SceneDefinition scene)
        {
            _currentScene = scene;
        }

        public override void OnStartup()
        {
            _nextReward.Value = CoinsUntilReward;
        }

        public void Update(StatusBar statusBar, SceneSpriteControllers sceneSpriteControllers)
        {
            if (_timer.Value > 0)
            {
                if(_timer.Value == FlashDuration 
                    && !AddReward(statusBar, sceneSpriteControllers))
                {
                    return;
                }

                var sprite = _spritesModule.GetSprite(_rewardSpriteIndex.Value);
                sprite.Palette = (SpritePalette)((byte)sprite.Palette + 1).NModByte(_specs.NumSpritePalettes);

                _timer.Value--;
                if (_timer.Value == 0)
                    sprite.Palette = 0;
            }
        }

        public void CheckRewards(int coinsAdded)
        {
            if (_timer.Value != 0)
                return;

            if(coinsAdded >= _nextReward.Value)
            {
                _audioService.PlaySound(ChompAudioService.Sound.Reward);
                _timer.Value = FlashDuration;
                _nextReward.Value = CoinsUntilReward;
            }
            else
            {
                _nextReward.Value -= (byte)coinsAdded;
            }
        }

        public void GiveHealth(SceneSpriteControllers sceneSpriteControllers)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Reward);

            var prize = sceneSpriteControllers.PrizeControllers.TryAddNew();
            if (prize != null)
            {
                prize.WorldSprite.X = sceneSpriteControllers.Player.WorldSprite.X + 8;
                prize.WorldSprite.Y = sceneSpriteControllers.Player.WorldSprite.Y - 8;
                prize.WorldSprite.UpdateSprite();
                _rewardSpriteIndex.Value = prize.SpriteIndex;
            }
        }

        public bool CheckExtraLife(uint scoreBefore, uint scoreAfter)
        {
            for (int i = 0; i < _extraLifeScores.Length; i++)
            {
                if (scoreBefore < _extraLifeScores[i] && scoreAfter >= _extraLifeScores[i])
                {
                    _audioService.PlaySound(ChompAudioService.Sound.Reward);
                    return true;
                }
            }

            return false;
        }

        private bool RewardIsBomb(StatusBar statusBar)
        {
            if (_currentScene.IsMidBossScene || _currentScene.IsLevelBossScene)
                return true;

            if (_currentScene.IsAutoScroll)
                return false;

            if (statusBar.Health == StatusBar.FullHealth)
                return true;
            else if (statusBar.Health <= 2)
                return false;


            var pct = (float)statusBar.Health / StatusBar.FullHealth;
            return pct <= _randomModule.Generate(8) / 256.0;
        }

        private bool AddReward(StatusBar statusBar, SceneSpriteControllers sceneSpriteControllers)
        {
            if (RewardIsBomb(statusBar))
            {
                var bomb = sceneSpriteControllers.BombControllers.TryAddNew();
                if (bomb != null)
                {                   
                    if (_currentScene.IsAutoScroll)
                    {
                        bomb.WorldSprite.X = 64;
                        bomb.WorldSprite.Y = sceneSpriteControllers.Player.WorldSprite.Y;

                        bomb.AcceleratedMotion.SetXSpeed(0);
                        bomb.AcceleratedMotion.SetYSpeed(0);
                    }
                    else
                    {
                        bomb.WorldSprite.X = sceneSpriteControllers.Player.WorldSprite.X;
                        bomb.WorldSprite.Y = sceneSpriteControllers.Player.WorldSprite.Y - 8;

                        bomb.AcceleratedMotion.SetYSpeed(-80);
                        bomb.FallCheck = _currentScene.SpriteFallCheck;
                    }
                    bomb.WorldSprite.UpdateSprite();
                    _rewardSpriteIndex.Value = bomb.SpriteIndex;
                    return true;
                }
            }
            else
            {
                var prize = sceneSpriteControllers.PrizeControllers.TryAddNew();
                if (prize != null)
                {
                    if (_currentScene.IsAutoScroll)
                    {
                        prize.WorldSprite.X = 64;
                        prize.WorldSprite.Y = sceneSpriteControllers.Player.WorldSprite.Y;
                        prize.Variation = 0;
                    }
                    else
                    {
                        prize.WorldSprite.X = sceneSpriteControllers.Player.WorldSprite.X + 16;
                        prize.WorldSprite.Y = sceneSpriteControllers.Player.WorldSprite.Y - 8;
                    }
                    prize.WorldSprite.UpdateSprite();

                    _rewardSpriteIndex.Value = prize.SpriteIndex;
                    return true;
                }
            }

            return false;
        }
    }
}
