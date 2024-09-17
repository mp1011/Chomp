using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss4Controller : EnemyController
    {
        public const int NumTailSections = 6;
        public const int BossHp = 4;
        private const int Speed = 40;
        private readonly ChompTail _tail;

        private readonly WorldScroller _scroller;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly MusicModule _music;
        private readonly Specs _specs;
        private WorldSprite _player;
        private PrecisionMotion _firstTailSectionMotion;

        enum Phase : byte 
        {
            Init,
            ClosePath,
            Appear,
            Dive,
            Attack
        }

        private TwoBit _diveCounter;
      
        private GameByteEnum<Phase> _phase;

        protected override bool DestroyBombOnCollision => true;

        public ChompBoss4Controller(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _music = gameModule.MusicModule;
            _scroller = gameModule.WorldScroller;
            _bullets = bullets;
            _player = player;

            _phase = new GameByteEnum<Phase>(new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right6, memoryBuilder.Memory));
            _diveCounter = new TwoBit(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            memoryBuilder.AddByte();

            _specs = gameModule.Specs;
            _tail = new ChompTail(memoryBuilder, NumTailSections, gameModule);
            Palette = SpritePalette.Enemy1;

            memoryBuilder.AddByte();

            _firstTailSectionMotion = new PrecisionMotion(memoryBuilder);
            memoryBuilder.AddBytes(PrecisionMotion.Bytes * NumTailSections - 1);
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
            _phase.Value = Phase.Init;
            sprite.Priority = false;
        }

        protected override void UpdateActive()
        {
            switch (_phase.Value)
            {
                case Phase.Init:
                    _music.CurrentSong = MusicModule.SongName.None;

                    if (_player.X > 16)
                        SetPhase(Phase.ClosePath);

                    break;

                case Phase.ClosePath:                   
                    if ((_levelTimer.Value % 8) == 0)
                    {
                        _audioService.PlaySound(ChompAudioService.Sound.Break);
                        int y = 11 - _stateTimer.Value;

                        _scroller.ModifyTiles((tilemap, attr) =>
                        {
                            tilemap[0, y] = _spriteTileTable.DestructibleBlockTile;
                            tilemap[1, y] = _spriteTileTable.DestructibleBlockTile;

                            tilemap[tilemap.Width - 1, y] = _spriteTileTable.DestructibleBlockTile;
                            tilemap[tilemap.Width - 2, y] = _spriteTileTable.DestructibleBlockTile;

                            attr[0, y / 2] = 1;
                            attr[(tilemap.Width / 2) - 1, y / 2] = 1;

                        });

                        _stateTimer.Value++;

                        if (_stateTimer.Value == 4)
                        {
                            _music.CurrentSong = MusicModule.SongName.Threat;
                            SetPhase(Phase.Appear);
                        }
                    }
                    break;

                case Phase.Appear:
                    if (_levelTimer.IsMod(4))
                    {
                        CreateExplosion();
                        _stateTimer.Value++;
                        if (_stateTimer.Value == 0)
                        {
                            if (_diveCounter.Value < 3)
                                SetPhase(Phase.Dive);
                            else
                                SetPhase(Phase.Attack);
                        }
                    }
                     
                    break;
                case Phase.Dive:

                    if(_stateTimer.Value <= 4)
                        _motion.TurnTowards(WorldSprite, new Point(32,16), 8, Speed);
                    else if (_stateTimer.Value <= 8)
                        _motion.TurnTowards(WorldSprite, _player.Center, 8, Speed);

                    _motionController.Update();

                    if (_stateTimer.Value < 15 && _levelTimer.IsMod(8))
                        _stateTimer.Value++;

                    if (WorldSprite.X < 0 || WorldSprite.Y < 0
                        || WorldSprite.X > _specs.ScreenWidth
                        || WorldSprite.Y > _specs.ScreenHeight)
                    {
                        _motion.Stop();
                        if(UpdateTail())
                            SetPhase(Phase.Appear);
                    }
                    else 
                        UpdateTail();

                    break;
                case Phase.Attack:

                    if (WorldSprite.Y < 14)
                        _motion.SetYSpeed(16);
                    else
                        _motion.SetYSpeed(0);

                    _motionController.Update();


                    if (_levelTimer.IsMod(16))
                        _stateTimer.Value++;

                    if (_stateTimer.Value > 4 && _levelTimer.IsMod(8))
                        FireBullet();

                    if(_stateTimer.Value == 15)
                        SetPhase(Phase.Dive);

                    break;
            }
        }

        private void ResetTail()
        {
            for (int index = 0; index < NumTailSections; index++)
            {
                var tailSprite = _tail.GetWorldSprite(index);
                tailSprite.X = WorldSprite.X;
                tailSprite.Y = WorldSprite.Y;
                tailSprite.Sprite.Priority = false;
            }
        }

        private bool UpdateTail()
        {
            bool allReachedTarget = true;

            var target = WorldSprite.Center;

            for(int index=0; index < NumTailSections; index++)
            {
                PrecisionMotion sectionMotion = new PrecisionMotion(_spritesModule.GameSystem.Memory,
                _firstTailSectionMotion.Address + (index * PrecisionMotion.Bytes));

                var tailSprite = _tail.GetWorldSprite(index);

                var distSq = tailSprite.Center.DistanceSquared(target);

                if(distSq > 0)
                {
                    allReachedTarget = false;
                    var angle = (distSq > 4 * 4) ?
                        tailSprite.Center.GetVectorTo(target, Speed - (2 * (index + 1))) :
                        tailSprite.Center.GetVectorTo(target, 24);

                    sectionMotion.XSpeed = angle.X;
                    sectionMotion.YSpeed = angle.Y;

                    sectionMotion.Apply(tailSprite);
                }
                
                target = tailSprite.Center;
            }

            return allReachedTarget;
        }
      
        private void SetPhase(Phase phase)
        {
            _phase.Value = phase;
            _stateTimer.Value = 0;

            switch(phase)
            {
                case Phase.Init:
                    WorldSprite.Visible = false;
                    break;

                case Phase.Appear:
                    WorldSprite.Visible = false;
                    PlaceBossAtRandomSpot();
                    break;

                case Phase.Dive:
                    _diveCounter.Value++;
                    WorldSprite.Visible = true;
                    ResetTail();
                    break;

                case Phase.Attack:
                    _diveCounter.Value = 0;
                    WorldSprite.Visible = true;
                    ResetTail();
                    break;
            }
        }

        private void PlaceBossAtRandomSpot()
        {

            switch(_diveCounter.Value < 3 ? _rng.Generate(2) : 2)
            {
                case 0:
                    WorldSprite.X = 56;
                    WorldSprite.Y = 4 * _rng.Generate(4);
                    break;
                case 1:
                    WorldSprite.X = 0;
                    WorldSprite.Y = 4 * _rng.Generate(4);
                    break;
                case 2:
                    WorldSprite.X = 4 + (3 * _rng.Generate(4));
                    WorldSprite.Y = 8;
                    break;
                case 3:
                    WorldSprite.X = 4 * _rng.Generate(4);
                    WorldSprite.Y = 56;
                    break;
            }

        }

        private void FireBullet()
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            bullet.WorldSprite.Center = WorldSprite.Center;

            var target = new Point(
                _player.X + (_rng.Generate(4)*2 - 16),
                _player.Y + (_rng.Generate(4)*2 - 16));

            bullet.AcceleratedMotion.TargetTowards(bullet.WorldSprite, target, 6);
            bullet.AcceleratedMotion.XAcceleration = 1;
            bullet.AcceleratedMotion.YAcceleration = 1;
        }

        private void CreateExplosion()
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.EnsureInFrontOf(this);
            bullet.WorldSprite.Center = WorldSprite.Center.Add(
                _rng.RandomItem(-4, -2, 0, 2, 4),
                _rng.RandomItem(-4, 2, 0, 2, 4));

            bullet.Motion.YSpeed = 0;
            bullet.Motion.XSpeed = 0;
            bullet.Explode();            
        }

        protected override void UpdateDying()
        {
            if (_hitPoints.Value > 0)
            {
                base.UpdateDying();
                if (WorldSprite.Status == WorldSpriteStatus.Active)
                    GetSprite().Palette = Palette;

                return;
            }
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (base.CollidesWithPlayer(player))
                return true;

            return false;
            //var partIndex = _levelTimer.Value % Arm.NumSections;

            //var part1 = _arm1.GetWorldSprite(partIndex);
            //var part2 = _arm2.GetWorldSprite(partIndex);

            //return player.CollidesWith(part1) || player.CollidesWith(part2);
        } 
    }
}
