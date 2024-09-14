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
        private ChompGameModule _gameModule;
        private WorldScroller _worldScroller;
        private RandomModule _rng;
        private readonly WorldSprite _player;
        private readonly CollisionDetector _collisionDetector;
        private readonly EnemyOrBulletSpriteControllerPool<MageBulletController> _bulletControllers;

        private NibbleEnum<Phase> _phase;

        private enum Phase : byte
        {
            Hidden,
            Appear,
            Attack,
            Disappear,
            Wait
        }

        public MageController(EnemyOrBulletSpriteControllerPool<MageBulletController> bulletControllers, SpriteTileIndex index, ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, WorldSprite player) 
            : base(SpriteType.Mage, index, gameModule, memoryBuilder)
        {
            _gameModule = gameModule;
            _player = player;
            _worldScroller = gameModule.WorldScroller;
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
            WorldSprite.Y = _player.Y - (4 + (_rng.Generate(3) * 2));
            WorldSprite.X = _player.X - 30 + (_rng.Generate(4) * 4);

            if (WorldSprite.Y < _worldScroller.ViewPane.Top)
                WorldSprite.Y = _worldScroller.ViewPane.Top + 4;
        }

        private bool PlayerIsClose()
        {
            if (_gameModule.CurrentScene.ScrollStyle == ScrollStyle.Vertical)
                return Math.Abs(_player.Y - WorldSprite.Y) < 18;
            else
                return Math.Abs(_player.X - WorldSprite.X) < 18;
        }

        protected override void UpdateActive()
        {
            if (_phase.Value == Phase.Hidden)
            {
                WorldSprite.Visible = false;
                CollisionEnabled = false;

                if (PlayerIsClose())
                {
                    _stateTimer.Value = 0;
                    _phase.Value = Phase.Appear;
                    PositionNearPlayer();
                }
            }
            else if (_phase.Value == Phase.Appear)
            {
                _motion.Stop();
                WorldSprite.Visible = _levelTimer.Value.IsMod(2);

                if (_levelTimer.Value.IsMod(8))
                    _stateTimer.Value++;

                if (_stateTimer.Value == 10)
                {
                    _stateTimer.Value = 0;
                    _phase.Value = Phase.Attack;
                    _motion.TargetYSpeed = -20;
                }
            }
            else if (_phase.Value == Phase.Attack)
            {
                _motion.YAcceleration = 5;
                WorldSprite.Visible = true;
                CollisionEnabled = true;


                if (_levelTimer.IsMod(8))
                {
                    _stateTimer.Value++;
                    if(_stateTimer.Value <= 3)
                        FireBullet();
                }

                if(_levelTimer.IsMod(32))
                {
                    _motion.TargetYSpeed *= -1;
                }
                    

                if (_stateTimer.Value == 10)
                {
                    _stateTimer.Value = 0;
                    _phase.Value = Phase.Disappear;
                }

                _motion.Apply(WorldSprite);
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

        private void FireBullet()
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.WorldSprite.Center = WorldSprite.Center;

            bullet.Motion.XSpeed = WorldSprite.FlipX ? 20 : -20;
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);                         
        }
    }
}
