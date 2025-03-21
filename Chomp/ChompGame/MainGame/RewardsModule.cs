﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers;

namespace ChompGame.MainGame
{
    class RewardsModule : Module
    {
        private const int FlashDuration = 60;
        private const int CoinsUntilReward = GameDebug.QuickReward ? 1 : 20;

        private readonly Specs _specs;
        private readonly ChompAudioService _audioService;
        private readonly SpritesModule _spritesModule;
        private SceneDefinition _currentScene;
        private GameByte _nextReward;
        private GameByte _timer;
        private GameByte _rewardSpriteIndex;

        public RewardsModule(MainSystem mainSystem) : base(mainSystem)
        {
            _audioService = mainSystem.GetModule<ChompAudioService>();
            _spritesModule = mainSystem.GetModule<SpritesModule>();
            _specs = mainSystem.Specs;
        }

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
            if (_currentScene.IsAutoScroll)
                coinsAdded *= 2;

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

        private bool RewardIsBomb(StatusBar statusBar)
        {
            if (_currentScene.IsMidBossScene || _currentScene.IsLevelBossScene)
                return true;

            if (_currentScene.IsAutoScroll)
                return false;

            return statusBar.Health == StatusBar.FullHealth;
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

                    _rewardSpriteIndex.Value = prize.SpriteIndex;
                    return true;
                }
            }

            return false;
        }
    }
}
