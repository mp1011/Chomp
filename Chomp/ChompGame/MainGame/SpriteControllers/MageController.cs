using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class MageController : EnemyController
    {
        private RandomModule _rng;
        private readonly WorldSprite _player;
        private readonly CollisionDetector _collisionDetector;
        private readonly ICollidableSpriteControllerPool _bulletControllers;

        private NibbleEnum<Phase> _phase;

        private enum Phase : byte
        {
            Hidden,
            Appear,
            Attack,
            Disappear,
            Wait
        }

        public MageController(ICollidableSpriteControllerPool bulletControllers, SpriteTileIndex index, ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, WorldSprite player) 
            : base(SpriteType.Mage, index, gameModule, memoryBuilder)
        {
            _player = player;
            _collisionDetector = gameModule.CollissionDetector;
            _bulletControllers = bulletControllers;
            Palette = SpritePalette.Enemy1;
            _rng = gameModule.RandomModule;

            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));
            memoryBuilder.AddByte();

        }
        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = 2;
            _stateTimer.Value = 0;
            _phase.Value = Phase.Hidden;
        }

        private void PositionNearPlayer()
        {
            WorldSprite.Y = _player.Y - (16 + (_rng.Generate(3) * 2));
            WorldSprite.X = _player.X - 30 + (_rng.Generate(4) * 4);
        }

        protected override void UpdateActive()
        {
            if (_phase.Value == Phase.Hidden)
            {
                WorldSprite.Visible = false;
                CollisionEnabled = false;

                if (Math.Abs(_player.X - WorldSprite.X) < 18)
                {
                    _stateTimer.Value = 0;
                    _phase.Value = Phase.Appear;
                    PositionNearPlayer();
                }
            }
            else if (_phase.Value == Phase.Appear)
            {
                WorldSprite.Visible = _levelTimer.Value.IsMod(2);

                if (_levelTimer.Value.IsMod(8))
                    _stateTimer.Value++;

                if (_stateTimer.Value == 10)
                {
                    _stateTimer.Value = 0;
                    _phase.Value = Phase.Attack;
                }
            }
            else if (_phase.Value == Phase.Attack)
            {
                WorldSprite.Visible = true;
                CollisionEnabled = true;

                if (_levelTimer.Value.IsMod(16))
                    _stateTimer.Value++;

                if (_stateTimer.Value == 10)
                {
                    _stateTimer.Value = 0;
                    _phase.Value = Phase.Disappear;
                }
            }
            else if (_phase.Value == Phase.Disappear)
            {
                WorldSprite.Visible = _levelTimer.Value.IsMod(2);
                CollisionEnabled = false;

                if (_levelTimer.Value.IsMod(8))
                    _stateTimer.Value++;

                if (_stateTimer.Value == 10)
                {
                    _stateTimer.Value = 0;
                    _phase.Value = Phase.Wait;
                }
            }
            else if (_phase.Value == Phase.Wait)
            {
                WorldSprite.Visible = false;
                CollisionEnabled = false;

                if (_levelTimer.Value.IsMod(16))
                    _stateTimer.Value++;

                if (_stateTimer.Value == 10)
                {
                    _stateTimer.Value = 0;
                    _phase.Value = Phase.Appear;
                    PositionNearPlayer();
                }
            }

            WorldSprite.FlipX = _player.X > WorldSprite.X;
        }
    }
}
