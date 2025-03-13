using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SceneModels.SmartBackground;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss6Controller : EnemyController
    {
        public const int MidX = 28;
        public const int MidY = 20;

        public const int LeftX = 12;
        public const int LeftY = 28;

        public const int RightX = 48;
        public const int RightY = 28;


        public const int ArmSize = 6;
        public const int BossHp = GameDebug.BossTest ? 1 : 5;
        private readonly PaletteModule _paletteModule;
        private readonly WorldScroller _scroller;
        private readonly EnemyOrBulletSpriteControllerPool<WavingBossBulletController> _bullets;
        private readonly MusicModule _music;
        private readonly ExitsModule _exitModule;
        private readonly Specs _specs;
        private WorldSprite _player;
        private CoreGraphicsModule _graphics;

        private ByteVector _arm1Pos, _arm2Pos;
        private ChompTail _arm1, _arm2;

        enum Phase : byte 
        {
            Init,
            BuildWall,
            Appear,
            ExtendArms,
            Attack,
            Fall,
            Dying
        }
      
        private GameByteEnum<Phase> _phase;

        protected override bool DestroyBombOnCollision => true;
        protected override bool AlwaysActive => true;

        public ChompBoss6Controller(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<WavingBossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _graphics = gameModule.GameSystem.CoreGraphicsModule;
            _paletteModule = gameModule.PaletteModule;
            _exitModule = gameModule.ExitsModule;
            _music = gameModule.MusicModule;
            _scroller = gameModule.WorldScroller;
            _bullets = bullets;
            _player = player;

            _phase = new GameByteEnum<Phase>(new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right6, memoryBuilder.Memory));
             memoryBuilder.AddByte();

            _specs = gameModule.Specs;
            Palette = SpritePalette.Enemy1;

            _arm1Pos = new ByteVector(memoryBuilder.AddByte(), memoryBuilder.AddByte());
            _arm2Pos = new ByteVector(memoryBuilder.AddByte(), memoryBuilder.AddByte());
            _arm1 = new ChompTail(memoryBuilder, ArmSize, gameModule);
            _arm2 = new ChompTail(memoryBuilder, ArmSize, gameModule);

            memoryBuilder.AddByte();



            GameDebug.Watch1 = new DebugWatch("T", () => _stateTimer.Value);
        }

        protected override void BeforeInitializeSprite()
        {
            _arm1.CreateTail();
            _arm2.CreateTail();
            SpriteIndex = _spritesModule.GetFreeSpriteIndex();
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _music.CurrentSong = MusicModule.SongName.None;
            _hitPoints.Value = BossHp;
            SetPhase(Phase.Init);
            sprite.Priority = true;
        }

        private void HideArms()
        {
            HideArm(_arm1);
            HideArm(_arm2);
        }

        private void HideArm(ChompTail arm)
        {
            for(int i = 0; i < ArmSize; i++)
            {
                arm.GetSprite(i).Visible = false;
            }
        }

        private void PositionArm(ChompTail arm, ByteVector position)
        {
            Point anchor = new Point(position.X, position.Y);
            float intervalX = (WorldSprite.X+4 - anchor.X) / (float)ArmSize;
            float intervalY = (WorldSprite.Y+4 - anchor.Y) / (float)ArmSize;

            for (int i = 0; i < ArmSize; i++)
            {
                var sprite = arm.GetSprite(i);
                sprite.X = (byte)(anchor.X + (intervalX * (i+1)));
                sprite.Y = (byte)(anchor.Y + (intervalY * (i+1)));
                sprite.Palette = SpritePalette.Enemy1;
                sprite.Priority = true;
                sprite.Visible = true;
            }
        }

        protected override void UpdateActive()
        {
            if(_phase.Value ==  Phase.Init)
            {
                if (_player.X > 32)
                    SetPhase(Phase.BuildWall);
            }
            else if(_phase.Value == Phase.BuildWall)
            {
                if(_levelTimer.IsMod(8))
                {
                    BuildWall();
                    _stateTimer.Value++;
                    if (_stateTimer.Value == 4)
                        SetPhase(Phase.Appear);
                }
            }
            else if(_phase.Value == Phase.Appear)
            {
                WorldSprite.Visible = _levelTimer.IsMod(2);

                if (_levelTimer.IsMod(16))
                {
                    _stateTimer.Value++;
                    FadeIn();
                }

                if(_stateTimer.Value == 8)
                {
                    SetPhase(Phase.ExtendArms);
                }
            }
            else if(_phase.Value == Phase.ExtendArms)
            {
                PositionArm(_arm1, _arm1Pos);
                PositionArm(_arm2, _arm2Pos);

                if(WorldSprite.X < MidX)
                {
                    _arm1Pos.Y--;
                    _arm2Pos.Y++;
                }
                else if(WorldSprite.X > MidX)
                {
                    _arm1Pos.Y--;
                    _arm2Pos.Y++;
                }
                else
                {
                    _arm1Pos.X -= 2;
                    _arm1Pos.Y++;

                    _arm2Pos.X += 2;
                    _arm2Pos.Y++;
                }

                if (_arm2Pos.Y >= _specs.ScreenHeight - 16 || _arm1Pos.X < 2)
                    SetPhase(Phase.Attack);
            }
            else if(_phase.Value == Phase.Attack)
            {
                if(_levelTimer.IsMod(128))                
                    FireBullet();

                switch (_hitPoints.Value)
                {
                    case 4:
                        AttackPhaseMotion(LeftX, LeftY);
                        break;
                    case 3:
                        AttackPhaseMotion(RightX, RightY);
                        break;
                    default:
                        AttackPhaseMotion(MidX, MidY);
                        break;
                }

                _motion.XAcceleration = 4;
                _motion.YAcceleration = 4;
                
                PositionArm(_arm1, _arm1Pos);
                PositionArm(_arm2, _arm2Pos);

                _motionController.Update();

            }
            else if(_phase.Value == Phase.Fall)
            {              
                _motionController.Update();
                PositionArm(_arm1, _arm1Pos);
                PositionArm(_arm2, _arm2Pos);

                RetractArm(_arm1Pos);
                RetractArm(_arm2Pos);

                if (WorldSprite.Y > _specs.ScreenHeight + 8)
                {
                    WorldSprite.Y = _specs.ScreenHeight + 8;
                    if (_levelTimer.IsMod(16))
                        _stateTimer.Value++;

                    if(_stateTimer.Value >= 10)
                        SetPhase(Phase.Appear);
                }
            }
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (_phase.Value >= Phase.Dying)
                return false;

            if (base.CollidesWithPlayer(player))
                return true;

            var partIndex = _levelTimer.Value % ArmSize;

            var part1 = _arm1.GetWorldSprite(partIndex);
            var part2 = _arm2.GetWorldSprite(partIndex);

            return player.CollidesWith(part1) || player.CollidesWith(part2);
        }

        private void AttackPhaseMotion(int x, int y)
        {
            if (_levelTimer.IsMod(8))
            {
                if (WorldSprite.Y < y)
                    _motion.TargetYSpeed = 8;
                else
                    _motion.TargetYSpeed = -8;
            }

            if (_levelTimer.IsMod(12))
            {
                if (WorldSprite.X < x)
                    _motion.TargetXSpeed = 8;
                else
                    _motion.TargetXSpeed = -8;
            }
        }

        private void RetractArm(ByteVector pos)
        {
            if (pos.X < WorldSprite.X)
                pos.X++;
            else if (pos.X > WorldSprite.X)
                pos.X--;

            if (pos.Y < WorldSprite.Y)
                pos.Y++;
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

        private void BuildWall()
        {
            _audioService.PlaySound(ChompAudioService.Sound.Break);
            int y = 7 - _stateTimer.Value;

            _scroller.ModifyTiles((tilemap, attr) =>
            {
                tilemap[0, y] = _spriteTileTable.DestructibleBlockTile;
                tilemap[1, y] = _spriteTileTable.DestructibleBlockTile;

                tilemap[tilemap.Width - 1, y] = _spriteTileTable.DestructibleBlockTile;
                tilemap[tilemap.Width - 2, y] = _spriteTileTable.DestructibleBlockTile;

                attr[0, y / 2] = 1;
                attr[(tilemap.Width / 2) - 1, y / 2] = 1;

            });
        }

        private void OpenWall()
        {
            _audioService.PlaySound(ChompAudioService.Sound.Break);
          
            _scroller.ModifyTiles((tilemap, attr) =>
            {
                for(int y = 4; y <= 7; y++)
                {
                    tilemap[tilemap.Width - 1, y] = 0;
                    tilemap[tilemap.Width - 2, y] = 0;

                    attr[0, y / 2] = 1;
                    attr[(tilemap.Width / 2) - 1, y / 2] = 1;
                }

            });
        }

        private void SetPhase(Phase phase)
        {
            _phase.Value = phase;
            _stateTimer.Value = 0;

            if (phase == Phase.Init)
            {
                CollisionEnabled = false;
                WorldSprite.Visible = false;
                WorldSprite.Y = 0;
            }
            else if (phase == Phase.Appear)
            {
                _motion.SetXSpeed(0);
                _motion.SetYSpeed(0);

                _music.CurrentSong = MusicModule.SongName.Threat;
                var spritePalette = _graphics.GetSpritePalette(SpritePalette.Enemy1);
                spritePalette.SetColor(1, ColorIndex.Black);
                spritePalette.SetColor(2, ColorIndex.Black);
                spritePalette.SetColor(3, ColorIndex.Black);
                CollisionEnabled = false;
                WorldSprite.Visible = true;

                switch(_hitPoints.Value)
                {
                    case 4:
                        WorldSprite.X = LeftX;
                        WorldSprite.Y = LeftY;
                        break;
                    case 3:
                        WorldSprite.X = RightX;
                        WorldSprite.Y = RightY;
                        break;
                    default:
                        WorldSprite.X = MidX;
                        WorldSprite.Y = MidY;
                        break;
                }
            }
            else if (phase == Phase.ExtendArms)
            {
                WorldSprite.Visible = true;
                CollisionEnabled = true;
                _arm1Pos.X = WorldSprite.X;
                _arm1Pos.Y = WorldSprite.Y;
                _arm2Pos.X = WorldSprite.X;
                _arm2Pos.Y = WorldSprite.Y;
            }
            else if(phase == Phase.Fall)
            {
                Palette = SpritePalette.Enemy1;
                CollisionEnabled = false;
                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 80;
                _motion.YAcceleration = 5;
            }
        }

        protected override void UpdateDying()
        {
            if(_hitPoints > 0)
            {
                if(_levelTimer.IsMod(16))
                {
                    _stateTimer.Value++;

                    if(_stateTimer.Value == 8)
                    {
                        WorldSprite.Status = WorldSpriteStatus.Active;
                        Palette = SpritePalette.Enemy1;
                        GetSprite().Palette = SpritePalette.Enemy1;
                        SetPhase(Phase.Fall);
                    }
                }
            }

            if(_phase.Value < Phase.Dying)            
                SetPhase(Phase.Dying);

            if (_phase.Value == Phase.Dying)
            {
                _music.CurrentSong = MusicModule.SongName.None;
                if (_levelTimer.IsMod(8))
                {
                    _stateTimer.Value++;
                    CreateExplosion();

                    if (_stateTimer.Value < ArmSize)
                    {
                        _arm1.Erase(_stateTimer.Value);
                        _arm2.Erase(_stateTimer.Value);
                    }

                    if (_stateTimer.Value == 0)
                    {
                        Destroy();
                        OpenWall();
                    }
                }
            }
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
