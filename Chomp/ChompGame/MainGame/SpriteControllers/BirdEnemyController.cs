﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class BirdEnemyController : EnemyController, IAutoScrollSpriteController
    {
        private const int _hoverSpeed = 20;
        private readonly WorldSprite _player;

        private LowNibble _variation;
        private NibbleEnum<BirdState> _state;
        protected override int PointsForEnemy => 100;
        private enum BirdState : byte
        {
            Hover,
            Attack,
            Return
        }

        public byte Variation
        {
            get => _variation.Value;
            set => _variation.Value = value;
        }

        protected override bool DestroyWhenFarOutOfBounds => _variation.Value > 0;
        protected override bool DestroyWhenOutOfBounds => false;

        protected override bool AlwaysActive => true;

        public BirdEnemyController(
            WorldSprite player,
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            SpriteTileIndex tileIndex) 
            : base(SpriteType.Bird,
                    tileIndex,
                    gameModule,                  
                    memoryBuilder)
        {
            _player = player;
            _variation = new LowNibble(memoryBuilder);
            _state = new NibbleEnum<BirdState>(new HighNibble(memoryBuilder.CurrentAddress, memoryBuilder.Memory));
            memoryBuilder.AddByte();
            Palette = gameModule.CurrentScene.Theme.IsCityTheme() ? SpritePalette.Enemy2 : SpritePalette.Player;
        }

        protected override void HandleFall() { }

        protected override void UpdateActive() 
        {
            _motionController.Update();

            if (_variation.Value == 0)
                UpdateBehavior_Normal();
            else
                UpdateBehavior_AutoScroll();
        }

        private void UpdateBehavior_Normal()
        {
            if (WorldSprite.XDistanceTo(_player) > 64)
                Hide();

            if ((_levelTimer % 32) == 0)
            {
                _stateTimer.Value++;

                int hoverTarget = _player.Y - 16;

                switch(_state.Value)
                {
                    case BirdState.Hover:
                        if (WorldSprite.Y < hoverTarget)
                            _motion.TargetYSpeed = _hoverSpeed;
                        else
                            _motion.TargetYSpeed = -_hoverSpeed;

                        WorldSprite.FlipX = _player.X < WorldSprite.X;

                        if(_stateTimer.Value == 8)
                        {
                            _stateTimer.Value = 0;
                            _state.Value = BirdState.Attack;
                        }

                        return;

                    case BirdState.Attack:
                        
                        if (_stateTimer.Value == 8 || WorldSprite.Y > _player.Y)
                        {
                            _stateTimer.Value = 0;
                            _state.Value = BirdState.Return;
                        }
                        else
                        {
                            _motion.TargetTowards(WorldSprite, _player, _motionController.WalkSpeed);
                        }

                        return;

                    case BirdState.Return:
                        _motion.TargetXSpeed = 0;
                        _motion.TargetYSpeed = -_hoverSpeed;

                        if (WorldSprite.Y <= hoverTarget)
                        {
                            _state.Value = BirdState.Hover;
                            _stateTimer.Value = 0;
                            _motion.TargetYSpeed = 0;
                        }
                        return;
                }           
            }
        }

        protected override void UpdateHidden()
        {
            if (_variation.Value > 0)
                Destroy();

            if (WorldSprite.XDistanceTo(_player) < 48)
                WorldSprite.Show();
        }

        private void UpdateBehavior_AutoScroll()
        {
            if ((_levelTimer % 32) == 0)
            {
                _stateTimer.Value++;

                if (_variation.Value == 2 && WorldSprite.X < 48)
                    return;

                if (_stateTimer.Value < 7)
                {
                    _motion.TargetTowards(WorldSprite, _player, _motionController.WalkSpeed);
                }
            }
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _motion.TargetXSpeed = 0;
            _motion.TargetYSpeed = 0;
            _motion.XSpeed = 0;
            _motion.YSpeed = 0;           
            _motion.XAcceleration = _motionController.WalkAccel;
            _motion.YAcceleration = _motionController.WalkAccel;

            _hitPoints.Value = 1;
            _state.Value = BirdState.Hover;
        }

        public void AfterSpawn(ISpriteControllerPool pool) { }
    }

}
