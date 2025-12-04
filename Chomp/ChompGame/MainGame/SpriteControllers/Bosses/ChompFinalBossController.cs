using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteControllers.Bosses;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompFinalBossController : EnemyController
    {
        private readonly PaletteModule _paletteModule;
        private readonly WorldScroller _scroller;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly MusicModule _music;
        private readonly ExitsModule _exitModule;
        private readonly Specs _specs;
        private WorldSprite _player;
        private CoreGraphicsModule _graphics;
        private BossPart _eye1, _eye2;
        private GameByte _rotation;

        private EnemyOrBulletSpriteControllerPool<GemSpriteController> _gemControllers;

        protected override int PointsForEnemy => 1000;

        enum Phase : byte 
        {
            Init,
            Attack1,
            Attack2,
            Attack3,
            Attack4,
            Attack5,
            Dying,
            Dead
        }
      
        private GameByteEnum<Phase> _phase;

        protected override bool DestroyBombOnCollision => true;
        protected override bool AlwaysActive => true;

        public ChompFinalBossController(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule,
            EnemyOrBulletSpriteControllerPool<GemSpriteController> gems,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _gemControllers = gems;
            _graphics = gameModule.GameSystem.CoreGraphicsModule;
            _paletteModule = gameModule.PaletteModule;
            _exitModule = gameModule.ExitsModule;
            _music = gameModule.MusicModule;
            _scroller = gameModule.WorldScroller;
            _bullets = bullets;
            _player = player;

            _eye1 = new BossPart(gameModule, memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye2 = new BossPart(gameModule, memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));

            _phase = new GameByteEnum<Phase>(new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right6, memoryBuilder.Memory));
             memoryBuilder.AddByte();

            _rotation = memoryBuilder.AddByte();
            _specs = gameModule.Specs;
            Palette = SpritePalette.Enemy1;
        }

        protected override void BeforeInitializeSprite()
        {
            SpriteIndex = _spritesModule.GetFreeSpriteIndex();
        }

        private void SetupBossParts()
        {
            var eye1Sprite = _eye1.PrepareSprite(SpriteTileIndex.Enemy2);
            eye1Sprite.Tile2Offset = 1;
            eye1Sprite.FlipX = true;
            eye1Sprite.FlipY = false;
            eye1Sprite.Priority = false;
            _eye1.XOffset = -8;
            _eye1.YOffset = -8;
            eye1Sprite.Visible = true;

            var eye2Sprite = _eye2.PrepareSprite(SpriteTileIndex.Enemy2);
            eye2Sprite.Tile2Offset = 1;
            eye2Sprite.FlipX = false;
            eye2Sprite.FlipY = false;
            eye2Sprite.Priority = false;
            _eye2.XOffset = 16;
            _eye2.YOffset = -8;
            eye2Sprite.Visible = true;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _music.CurrentSong = MusicModule.SongName.None;
            _hitPoints.Value = 1;
            SetPhase(Phase.Init);
            sprite.Priority = true;
        }

        private void FireAimedBullet(Point origin)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnTimer = true;
            bullet.DestroyOnCollision = true;

            bullet.WorldSprite.X = origin.X;
            bullet.WorldSprite.Y = origin.Y;

            var vector = origin.GetVectorTo(_player.Bounds.Center, 40);

            bullet.AcceleratedMotion.SetXSpeed(vector.X);
            bullet.AcceleratedMotion.SetYSpeed(vector.Y);
        }

        private void FireRandomBullet(Point origin)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnTimer = true;
            bullet.DestroyOnCollision = true;

            bullet.WorldSprite.X = origin.X;
            bullet.WorldSprite.Y = origin.Y;

            bullet.AcceleratedMotion.SetXSpeed(-32 + _rng.Generate(6));
            bullet.AcceleratedMotion.SetYSpeed(-32 + _rng.Generate(6));
        }

        private void FireRandomBullet2(Point origin)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnTimer = true;
            bullet.DestroyOnCollision = true;

            bullet.WorldSprite.X = origin.X;
            bullet.WorldSprite.Y = origin.Y;

            bullet.AcceleratedMotion.SetXSpeed(-32 + _rng.Generate(6));
            bullet.AcceleratedMotion.SetYSpeed(24 + _rng.Generate(4));
        }

        private void FireGravityBullet(Point origin)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnTimer = false;
            bullet.DestroyOnCollision = true;

            bullet.WorldSprite.X = origin.X;
            bullet.WorldSprite.Y = origin.Y;

            bullet.AcceleratedMotion.YSpeed = -40;
            bullet.AcceleratedMotion.TargetYSpeed = 40;
            bullet.AcceleratedMotion.YAcceleration = 4;
        }

        protected override void UpdateActive()
        {
            _music.CurrentSong = MusicModule.SongName.FinalBossPart3;
            if (_phase.Value ==  Phase.Init)
            {
                if(_levelTimer.IsMod(16))
                    MoveToward(_player.X, _player.Y - 24, 16);

                if (_levelTimer.IsMod(32))
                    _stateTimer.Value ++;

                if (_stateTimer.Value == 15)
                    SetPhase(Phase.Attack1);


                _rotation.Value++;
            }
            else if(_phase.Value == Phase.Attack1)
            {
                if (_stateTimer.Value < 8)
                    MoveToward(_player.X + 16, _player.Y - 28, 16);
                else
                    MoveToward(_player.X - 16, _player.Y - 28, 16);

                if (_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    if(_stateTimer.Value == 0)
                        SetPhase(Phase.Attack2);
                }

                _rotation.Value += 2;

                if(_stateTimer.Value.IsMod(4) && _levelTimer.IsMod(8))
                {
                    FireAimedBullet(_eye1.WorldSprite.Center);
                    FireAimedBullet(_eye2.WorldSprite.Center);
                }
            }
            else if (_phase.Value == Phase.Attack2)
            {
                MoveToward(_player.X, _player.Y - 24, 32);
                
                if (_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    if(_stateTimer.Value == 0)
                        SetPhase(Phase.Attack3);
                }

                _rotation.Value += 4;

                if (_levelTimer.IsMod(8))
                {
                    FireGravityBullet(_eye1.WorldSprite.Center);
                    FireGravityBullet(_eye2.WorldSprite.Center);
                }
            }
            else if (_phase.Value == Phase.Attack3)
            {
                if (_stateTimer.Value < 8)
                    MoveToward(_player.X + 16, _player.Y - 24, 16);
                else
                    MoveToward(_player.X - 16, _player.Y - 24, 16);

                if (_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                        SetPhase(Phase.Attack4);
                }

                _rotation.Value += 8;

                if (_stateTimer.Value.IsMod(2) && _levelTimer.IsMod(8))
                {
                    FireAimedBullet(_eye1.WorldSprite.Center);
                    FireAimedBullet(_eye2.WorldSprite.Center);
                }
            }
            else if (_phase.Value == Phase.Attack4)
            {
                MoveToward(_player.X, _player.Y - 24, 32);

                if (_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                        SetPhase(Phase.Attack5);
                }

                _rotation.Value += 16;

                if (_levelTimer.IsMod(8))
                {
                    FireGravityBullet(_eye1.WorldSprite.Center);
                    FireGravityBullet(_eye2.WorldSprite.Center);
                }
            }
            else if (_phase.Value == Phase.Attack5)
            {
                MoveToward(60, 70, 8);

                if (_levelTimer.IsMod(64))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                        SetPhase(Phase.Dying);
                }

                _rotation.Value += 16;

                if (_levelTimer.IsMod(8))
                {
                    FireRandomBullet2(_eye1.WorldSprite.Center);
                    FireRandomBullet2(_eye2.WorldSprite.Center);

                    CreateExplosion();
                }
            }
            else if (_phase.Value == Phase.Dying)
            {
                MoveToward(60, 80, 1);

                if (_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 0)
                    {
                        _eye1.Sprite.Visible = false;
                        _eye2.Sprite.Visible = false;
                        WorldSprite.Visible = false;
                        CollisionEnabled = false;
                        SetPhase(Phase.Dead);
                    }
                }

                _rotation.Value += 16;

                if (_levelTimer.IsMod(4))
                {
                    CreateExplosion();
                }
            }
            else if(_phase.Value == Phase.Dead)
            {
                if (_stateTimer.Value == 0)
                {
                    if (_levelTimer.Value == 0)
                    {
                        _gemControllers.Execute(p => { p.Expanding = true; });
                        _stateTimer.Value = 1;
                    }
                }
                else
                {
                    if (_levelTimer.Value.IsMod(32))
                    {
                        _stateTimer.Value++;

                        if (_music.PlayPosition >= 90000)
                            _exitModule.GotoNextLevel();
                    }
                }
            }

                UpdateOffset();
            _motionController.Update();
            UpdatePartPositions();
        }

        private void MoveToward(int x, int y, int target)
        { 
            if(WorldSprite.X < x)
            {
                _motion.TargetXSpeed = target;
                _motion.XAcceleration = 4;
            }
            else
            {
                _motion.TargetXSpeed = -target;
                _motion.XAcceleration = 4;
            }

            if (WorldSprite.Y < y)
            {
                _motion.TargetYSpeed = target;
                _motion.YAcceleration = 4;
            }
            else
            {
                _motion.TargetYSpeed = -target;
                _motion.YAcceleration = 4;
            }
        }

        private void UpdateOffset()
        {
            float deg = (float)(360.0 * (_rotation.Value / 256.0));
            var pt = new Point(0, 16).RotateDeg(deg);

            _eye1.XOffset = pt.X;
            _eye1.YOffset = pt.Y;

            deg = 360.0f - deg;
            pt = new Point(0, 16).RotateDeg(deg);

            _eye2.XOffset = pt.X;
            _eye2.YOffset = pt.Y;
        }

        private void UpdatePartPositions()
        {
            _eye1.UpdatePosition(WorldSprite);
            _eye2.UpdatePosition(WorldSprite);
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {            
            return false;
        }

        private void FireBullet()
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            bullet.WorldSprite.Center = WorldSprite.Center;

            var target = _player.Center;

            bullet.AcceleratedMotion.SetYSpeed(6);
            
        }

        private void FadeIn()
        {
            var targetSpritePalette = _paletteModule.GetPalette(PaletteKey.BlueGrayEnemy);
         
            var spritePalette = _graphics.GetSpritePalette(SpritePalette.Enemy1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 2);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 3);
        }



        private void SetPhase(Phase phase)
        {
            _phase.Value = phase;
            _stateTimer.Value = 0;

            if (phase == Phase.Init)
            {
                _music.CurrentSong = MusicModule.SongName.FinalBossPart3;
                WorldSprite.X = 44;
                WorldSprite.Y = 90;
                SetupBossParts();

                for(int i = 0; i < 8; i++)
                {
                    var gem = _gemControllers.TryAddNew();
                    gem.Index = (byte)i;
                }

            }
        }

        protected override void UpdateDying()
        {
           
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
    }
}
