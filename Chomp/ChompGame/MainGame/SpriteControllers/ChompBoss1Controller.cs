using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss1Controller : EnemyController
    {
        private readonly MovingWorldSprite _player;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly ChompAudioService _audioService;
        private readonly MusicModule _music;
        private readonly WorldScroller _scroller;
        private readonly Specs _specs;
        private readonly GameByteArray _tailSprites;
        private readonly NibblePoint _motionTarget;

        private const int MaxY = 32;
        private const int MinY = 12;

        private const int TailSections = 4;

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

        public ChompBoss1Controller(MovingWorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, gameModule, memoryBuilder)
        {
            _player = player;
            _audioService = gameModule.AudioService;
            _music = gameModule.MusicModule;
            _bullets = bullets;

            _phaseByte = memoryBuilder.AddByte();
            _phase = new GameByteEnum<Phase>(_phaseByte);
            _scroller = gameModule.WorldScroller;
            _specs = gameModule.Specs;
            _tailSprites = new GameByteArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(TailSections);

            _motionTarget = new NibblePoint(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddByte();
        }

        private void HideTail()
        {
            for (int i = 0; i < TailSections; i++)
            {
                var sprite = _spritesModule.GetSprite(_tailSprites[i]);
                sprite.Y = 0;
            }
        }

        private void UpdateTail()
        {
            Point anchor = new Point(30, 16);

            int intervalX = (WorldSprite.X - anchor.X) / TailSections;
            int intervalY = (WorldSprite.Y - anchor.Y) / TailSections;

            for (int i = 0; i < TailSections; i++)
            {
                var sprite = _spritesModule.GetSprite(_tailSprites[i]);
                sprite.X = (byte)(anchor.X + (intervalX * i));
                sprite.Y = (byte)(anchor.Y + (intervalY * i));
            }
        }

        protected override void BeforeInitializeSprite() 
        {
            for(int i=0; i< TailSections;i++)
            {
                var spriteIndex = _spritesModule.GetFreeSpriteIndex();
                _tailSprites[i] = spriteIndex;

                var sprite = _spritesModule.GetSprite(spriteIndex);
                sprite.Tile = 7;
                sprite.SizeX = 1;
                sprite.SizeY = 1;
                sprite.Palette = 2;
                sprite.Visible = true;
                sprite.X = 0;
                sprite.Y = 0;
            }

            SpriteIndex = _spritesModule.GetFreeSpriteIndex();

            GameDebug.Watch1 = new DebugWatch("Boss Phase", () => _phaseByte.Value);

        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = 2;
            HideTail();
        }

        protected override void UpdateBehavior()
        {
            _movingSpriteController.Update();
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
                        tilemap[0, y] = Constants.DestructibleBlockTile;
                        tilemap[1, y] = Constants.DestructibleBlockTile;

                        tilemap[tilemap.Width - 1, y] = Constants.DestructibleBlockTile;
                        tilemap[tilemap.Width - 2, y] = Constants.DestructibleBlockTile;

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

                Motion.SetYSpeed(10);

                if (WorldSprite.Y >= MaxY)
                {
                    Motion.SetYSpeed(0);
                    _phase.Value = Phase.BeforeAttack;

                    Motion.XAcceleration = _movingSpriteController.WalkAccel;
                    Motion.YAcceleration = _movingSpriteController.WalkAccel;
                    Motion.XSpeed = _movingSpriteController.WalkSpeed;
                    Motion.YSpeed = _movingSpriteController.WalkSpeed;
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

                Motion.TargetTowards(WorldSprite, Target, _movingSpriteController.WalkSpeed);

                if ((_levelTimer.Value % 32) == 0)
                    _phaseByte.Value++;
            }
            else if (_phase.Value.Between(Phase.PrepareAttack, Phase.AttackStart))
            {
                _motionTarget.Y = 6;
                _motionTarget.X = 6;

                if(Motion.TargetTowardsExact(WorldSprite, Target, _movingSpriteController.WalkSpeed))
                {
                    if (_levelTimer.IsMod(32))
                        _phaseByte.Value++;
                }
            }
            else if(_phase.Value == Phase.AttackStart)
            {
                var rng = new RandomHelper(_levelTimer.Value);
                _motionTarget.Y = 10;
                _motionTarget.X = (byte)rng.RandomItem(2, 6, 10);

                Motion.TargetTowards(WorldSprite, Target, _movingSpriteController.WalkSpeed);
                _phaseByte.Value++;
            }
            else if (_phase.Value == Phase.Attack)
            {
                if(Motion.TargetTowardsExact(WorldSprite, Target, _movingSpriteController.WalkSpeed))
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

        protected override bool HandleDestroy()
        {
            if (_phase.Value < Phase.DestroyBegin)
            {
                _phase.Value = Phase.DestroyBegin;
                _music.CurrentSong = MusicModule.SongName.None;
            }
            else if (_phase.Value >= Phase.DestroyEnd)
            {
                HideTail();
                OpenPath();
                return true;
            }

            GetSprite().Visible = _levelTimer.IsMod(2);

            if (_levelTimer.Value.IsMod(64))
                _phase.Value++;

            if (_levelTimer.Value.IsMod(16))
            {
                _audioService.PlaySound(ChompAudioService.Sound.Break);

                var bullet = _bullets.TryAddNew(3);
                if (bullet != null)
                {
                    var rng = new RandomHelper(_levelTimer.Value);
                    bullet.WorldSprite.Center = WorldSprite.Center.Add(
                        rng.RandomItem(-4, -2, 0, 2, 4),
                        rng.RandomItem(-4, 2, 0, 2, 4));

                    bullet.Motion.SetYSpeed(0);
                    bullet.Motion.SetXSpeed(0);
                    bullet.Explode();
                }
            }

            return false;

        }


        private void FireBullet(int xSpeed)
        {
            var bullet = _bullets.TryAddNew(3);
            if (bullet == null)
                return;

            bullet.WorldSprite.Center = WorldSprite.Center;
            bullet.Motion.SetYSpeed(15);
            bullet.Motion.SetXSpeed(xSpeed);
        }
    }
}
