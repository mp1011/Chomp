using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss1Controller : EnemyController
    {
        public const int BossHp = 3;
        public const int NumTailSections = 4;

        private readonly WorldSprite _player;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly ChompAudioService _audioService;
        private readonly MusicModule _music;
        private readonly WorldScroller _scroller;
        private readonly Specs _specs;
        private readonly NibblePoint _motionTarget;
        private ChompTail _tail;

        private const int MaxY = 32;
        private const int MinY = 12;


        private Point Target => new Point(8 + _motionTarget.X * 4, 8 + _motionTarget.Y * 2);
     
        enum Phase : byte 
        {
            Init=0,
            BeforeTrap=1,
            TrapStart=2,
            TrapEnd=12,
            Appear=15,
            BeforeAttack=16,
            PrepareAttack=20,
            AttackStart=24,
            Attack=25,
            DestroyBegin=26,
            DestroyEnd=30
        }

        private GameByte _phaseByte;
        private GameByteEnum<Phase> _phase;
        protected override bool DestroyBombOnCollision => true;

        public ChompBoss1Controller(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _player = player;
            _audioService = gameModule.AudioService;
            _music = gameModule.MusicModule;
            _bullets = bullets;

            _phaseByte = memoryBuilder.AddByte();
            _phase = new GameByteEnum<Phase>(_phaseByte);
            _scroller = gameModule.WorldScroller;
            _specs = gameModule.Specs;

            _tail = new ChompTail(memoryBuilder, NumTailSections, gameModule);

            _motionTarget = new NibblePoint(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddByte();

            Palette = SpritePalette.Enemy1;
        }

        private void HideTail()
        {
            for (int i = 0; i < NumTailSections; i++)
            {
                var sprite = _tail.GetSprite(i);
                sprite.Y = 0;
            }
        }

        private void UpdateTail()
        {
            Point anchor = new Point(30, 16);

            int intervalX = (WorldSprite.X - anchor.X) / NumTailSections;
            int intervalY = (WorldSprite.Y - anchor.Y) / NumTailSections;

            for (int i = 0; i < NumTailSections; i++)
            {
                var sprite = _tail.GetSprite(i);
                sprite.X = (byte)(anchor.X + (intervalX * i));
                sprite.Y = (byte)(anchor.Y + (intervalY * i));
            }
        }

        protected override void BeforeInitializeSprite()
        {
            _tail.CreateTail();
            SpriteIndex = _spritesModule.GetFreeSpriteIndex();
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = BossHp;
            _stateTimer.Value = 0;
            HideTail();
        }

        protected override void UpdateActive()
        {
            _motionController.Update();
            if (_phase.Value >= Phase.Appear)
            {
                UpdateTail();
            }

            if (_phase.Value == Phase.Init)
            {
                WorldSprite.X = ((_scroller.LevelNameTable.Width / 2) - 1) * _specs.TileWidth;
                WorldSprite.Y = 0;

                _music.CurrentSong = MusicModule.SongName.None;
                _phase.Value = Phase.BeforeTrap;
            }
            else if (_phase.Value == Phase.BeforeTrap && _player.X > 20)
            {
                _phase.Value = Phase.TrapStart;
            }
            else if (_phase.Value.Between(Phase.TrapStart, Phase.TrapEnd))
            {
                if ((_levelTimer.Value % 8) == 0)
                {
                    _audioService.PlaySound(ChompAudioService.Sound.Break);
                    int y = 13 - _phaseByte.Value;

                    _scroller.ModifyTiles((tilemap, attr) =>
                    {
                        tilemap[0, y] = _spriteTileTable.DestructibleBlockTile;
                        tilemap[1, y] = _spriteTileTable.DestructibleBlockTile;

                        tilemap[tilemap.Width - 1, y] = _spriteTileTable.DestructibleBlockTile;
                        tilemap[tilemap.Width - 2, y] = _spriteTileTable.DestructibleBlockTile;

                        attr[0, y / 2] = 1;
                        attr[(tilemap.Width / 2) - 1, y / 2] = 1;

                    });

                    _phaseByte.Value++;
                }
            }
            else if (_phase.Value.Between(Phase.TrapEnd, Phase.Appear))
            {
                if ((_levelTimer.Value % 8) == 0)
                    _phaseByte.Value++;
            }
            else if (_phase.Value == Phase.Appear)
            {
                if (WorldSprite.Y < MinY)
                    WorldSprite.Y = MinY;


                _music.CurrentSong = MusicModule.SongName.Threat;

                _motion.SetYSpeed(10);

                if (WorldSprite.Y >= MaxY)
                {
                    _motion.SetYSpeed(0);
                    _phase.Value = Phase.BeforeAttack;

                    _motion.XAcceleration = _motionController.WalkAccel;
                    _motion.YAcceleration = _motionController.WalkAccel;
                    _motion.XSpeed = _motionController.WalkSpeed;
                    _motion.YSpeed = _motionController.WalkSpeed;
                }
            }
            else if (_phase.Value.Between(Phase.BeforeAttack, Phase.PrepareAttack))
            {
                if ((_levelTimer % 128) == 0
                    || WorldSprite.Bounds.Center.DistanceSquared(Target) < 8
                    || WorldSprite.Y > MaxY
                    || WorldSprite.Y < MinY)
                {
                    var rng = new Random();
                    _motionTarget.X = (byte)rng.Next(15);
                    _motionTarget.Y = (byte)rng.Next(15);
                }

                _motion.TargetTowards(WorldSprite, Target, _motionController.WalkSpeed);

                if ((_levelTimer.Value % 32) == 0)
                    _phaseByte.Value++;
            }
            else if (_phase.Value.Between(Phase.PrepareAttack, Phase.AttackStart))
            {
                _motionTarget.Y = 6;
                _motionTarget.X = 6;

                if(_motion.TargetTowardsExact(WorldSprite, Target, _motionController.WalkSpeed))
                {
                    if (_levelTimer.IsMod(32))
                        _phaseByte.Value++;
                }
            }
            else if(_phase.Value == Phase.AttackStart)
            {
                _motionTarget.Y = 10;
                _motionTarget.X = (byte)_rng.RandomItem(2, 6, 10);

                _motion.TargetTowards(WorldSprite, Target, _motionController.WalkSpeed);
                _phaseByte.Value++;
            }
            else if (_phase.Value == Phase.Attack)
            {
                if(_motion.TargetTowardsExact(WorldSprite, Target, _motionController.WalkSpeed))
                {
                    _phase.Value = Phase.BeforeAttack;

                    _audioService.PlaySound(ChompAudioService.Sound.Fireball);
                    FireBullet(-8);
                    FireBullet(0);
                    FireBullet(8);

                }
            }
        }

        private void OpenPath()
        {
            _scroller.ModifyTiles((tilemap, attr) =>
            {
                for (int y = 8; y < 12; y++)
                {
                    tilemap[tilemap.Width - 1, y] = 0;
                    tilemap[tilemap.Width - 2, y] = 0;
                    attr[(tilemap.Width / 2) - 1, y / 2] = 1;
                }

            });
        }


        private void FireBullet(int xSpeed)
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.WorldSprite.Center = WorldSprite.Center;
            bullet.Motion.YSpeed = 15;
            bullet.Motion.XSpeed= xSpeed;
        }

        protected override void UpdateDying()
        {
            if(_hitPoints.Value > 0)
            {
                base.UpdateDying();
                if (WorldSprite.Status == WorldSpriteStatus.Active)
                    GetSprite().Palette = Palette;

                return;
            }

            if (_phase.Value < Phase.DestroyBegin)
            {
                _phase.Value = Phase.DestroyBegin;
                _music.CurrentSong = MusicModule.SongName.None;
                Palette = 0;
                GetSprite().Palette = 0;
                _bullets.Execute(p => p.Destroy());
            }
            else if (_phase.Value >= Phase.DestroyEnd)
            {
                HideTail();
                OpenPath();
                _stateTimer.Value = 0;
                base.UpdateDying();
            }

            GetSprite().Visible = _levelTimer.IsMod(2);

            if (_levelTimer.Value.IsMod(64))
                _phase.Value++;

            if (_levelTimer.Value.IsMod(16))
            {
                _audioService.PlaySound(ChompAudioService.Sound.Break);

                var bullet = _bullets.TryAddNew();
                if (bullet != null)
                {
                    bullet.EnsureInFrontOf(this);
                    bullet.WorldSprite.Center = WorldSprite.Center.Add(
                        _rng.RandomItem(-4, -2, 0, 2, 4),
                        _rng.RandomItem(-4, 2, 0, 2, 4));

                    bullet.Motion.YSpeed = 0;
                    bullet.Motion.XSpeed = 0;
                    bullet.Explode();
                }
            }
        }
    }
}
