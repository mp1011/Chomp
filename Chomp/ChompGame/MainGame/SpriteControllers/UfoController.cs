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
    class UfoController : EnemyController, IAutoScrollSpriteController
    {
        public const int SPath = 1;
        public const int Chase = 2;
        public const int Hook = 3;

        private ChompGameModule _gameModule;
        private WorldScroller _worldScroller;
        private RandomModule _rng;
        private readonly WorldSprite _player;
        private readonly CollisionDetector _collisionDetector;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bulletControllers;
        private LowNibble _variation;
        private HighNibble _extra;
        protected override int PointsForEnemy => 250;
        protected override bool DestroyWhenFarOutOfBounds => true;

        protected override bool DestroyWhenOutOfBounds => false;

        protected override bool AlwaysActive => false;

        public byte Variation { get => _variation.Value; set => _variation.Value = value; }

        private enum Phase : byte
        {
            Hidden,
            Appear,
            Attack,
            Disappear,
            Wait
        }

        public UfoController(EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SpriteTileIndex index, ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder, WorldSprite player)
            : base(SpriteType.Ufo, index, gameModule, memoryBuilder)
        {
            _gameModule = gameModule;
            _player = player;
            _worldScroller = gameModule.WorldScroller;
            _collisionDetector = gameModule.CollissionDetector;
            _bulletControllers = bulletControllers;
            Palette = SpritePalette.Enemy1;
            _rng = gameModule.RandomModule;

            _variation = new LowNibble(memoryBuilder);
            _extra = new HighNibble(memoryBuilder);

            memoryBuilder.AddByte();

        }
        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = 1;
            _stateTimer.Value = 0;
        }


        protected override void UpdateActive()
        {
            if (_variation.Value == UfoController.SPath)
                Update_SPath();
            else if (_variation.Value == UfoController.Chase)
                Update_Chase();
            else if (_variation.Value == UfoController.Hook)
                Update_Hook();
            else
                Update_Normal();

            _motionController.Update();
        }

        private void Update_Normal()
        {
            if(_stateTimer.Value == 0)
            {
                int dx = WorldSprite.X - _player.X;
                int dy = WorldSprite.Y - _player.Y;

                if(Math.Abs(dx) > Math.Abs(dy))
                {
                    _motion.TargetYSpeed = 0;
                    if (dx > 0)
                        _motion.TargetXSpeed = -_motionController.WalkSpeed;
                    else
                        _motion.TargetXSpeed = _motionController.WalkSpeed;
                }
                else
                {
                    _motion.TargetXSpeed = 0;
                    if (dy > 0)
                        _motion.TargetYSpeed = -_motionController.WalkSpeed;
                    else
                        _motion.TargetYSpeed = _motionController.WalkSpeed;
                }

                _motion.XAcceleration = _motionController.WalkAccel;
                _motion.YAcceleration = _motionController.WalkAccel;
                _stateTimer.Value++;
            }
            else if(_levelTimer.IsMod(4))
            {
                _stateTimer.Value++;
            }

        }

        private void Update_Chase()
        {
            if (_stateTimer.Value < 15)
            {
                if (_levelTimer.IsMod(4))
                    _motion.TurnTowards(WorldSprite, _player.Center, 12, 50);

                if (_levelTimer.IsMod(16))
                {
                    _stateTimer.Value++;
                }
            }
        }

        private void Update_SPath()
        {
            if (WorldSprite.X < 0)
                Destroy();

            if (_stateTimer.Value == 0)
            {
                _stateTimer.Value = 1;
                _motion.SetYSpeed(0);
                _motion.SetXSpeed(-6);
                _extra.Value = (byte)(WorldSprite.Y / 4);

                _motion.YAcceleration = 8;
            }
            else
            {
                if(_levelTimer.IsMod(32))
                {
                    int midY = _extra.Value * 4;
                    if (WorldSprite.Y < midY)
                        _motion.TargetYSpeed = 44;
                    else
                        _motion.TargetYSpeed = -44;
                }

                if(_motion.YSpeed > 0 && _stateTimer.Value == 1)
                {
                    _stateTimer.Value = 2;
                    FireBullet(0, 60);

                }
                if (_motion.YSpeed < 0 && _stateTimer.Value == 2)
                {
                    _stateTimer.Value = 1;
                    FireBullet(0, -60);
                }
            }
        }

        private void Update_Hook()
        {
            if (_stateTimer.Value == 0)
            {
                _stateTimer.Value = 1;
                _motion.SetYSpeed(0);
                _motion.SetXSpeed(-60);
            }
            else if (_stateTimer.Value == 1)
            {
                if (WorldSprite.X < 32)
                {
                    _stateTimer.Value = 2;
                    _motion.XAcceleration = 4;
                    _motion.TargetXSpeed = 60;

                    if (WorldSprite.Y < _gameModule.Specs.ScreenHeight / 2)
                        _motion.TargetYSpeed = 10;
                    else
                        _motion.TargetYSpeed = -10;

                    _motion.YAcceleration = 2;
                }
            }
            else if(_stateTimer.Value == 2 && _motion.XSpeed >= 0)
            {
                _stateTimer.Value = 3;
                FireAimedBullet();
            }
            else if (_stateTimer.Value == 3 && WorldSprite.X > _gameModule.Specs.ScreenWidth)
                Destroy();
        }


        private void FireAimedBullet()
        {
            var dir = (_player.Center - WorldSprite.Center).Normalize() * 30;
            FireBullet((byte)dir.X, (byte)dir.Y);
        }

        private void FireBullet(int x, int y)
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.Palette = SpritePalette.Fire;
            var dir = (_player.Center - WorldSprite.Center).Normalize() * 30;

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
            bullet.Motion.XSpeed = x;
            bullet.Motion.YSpeed = y;
            bullet.WorldSprite.X = WorldSprite.X;
            bullet.WorldSprite.Y = WorldSprite.Y;
            bullet.DestroyOnTimer = true;
        }

        public void AfterSpawn(ISpriteControllerPool pool)
        {
        }
    }
}
