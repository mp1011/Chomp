using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss5Controller : EnemyController
    {
        public const int BridgeY = 54;

        public const int NumTailSections = 4;
        public const int BossHp = GameDebug.BossTest ? 1 : 3;
        private readonly ChompTail _tail;

        private readonly WorldScroller _scroller;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly MusicModule _music;
        private readonly ExitsModule _exitModule;
        private readonly Specs _specs;
        private WorldSprite _player;
        private CoreGraphicsModule _graphics;
        private BossBackgroundHandler _bossBgHandler;
        private GameByte _anchor;
        
        enum Phase : byte 
        {
            Init,
            Fall,
            Latch,
            Detach,
            Swing,
            Dying,
            Dead
        }
      
        private GameByteEnum<Phase> _phase;

        protected override bool DestroyBombOnCollision => true;
        protected override bool AlwaysActive => true;

        public ChompBoss5Controller(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _graphics = gameModule.GameSystem.CoreGraphicsModule;
            _exitModule = gameModule.ExitsModule;
            _bossBgHandler = gameModule.BossBackgroundHandler;
            _music = gameModule.MusicModule;
            _scroller = gameModule.WorldScroller;
            _bullets = bullets;
            _player = player;

            _phase = new GameByteEnum<Phase>(new MaskedByte(memoryBuilder.CurrentAddress, Bit.Right6, memoryBuilder.Memory));
             memoryBuilder.AddByte();

            _specs = gameModule.Specs;
            _tail = new ChompTail(memoryBuilder, NumTailSections, gameModule);
            Palette = SpritePalette.Enemy1;

            memoryBuilder.AddByte();

            _anchor = memoryBuilder.AddByte();

            GameDebug.Watch1 = new DebugWatch("T", () => _stateTimer.Value);
        }

        protected override void BeforeInitializeSprite()
        {
            _tail.CreateTail();
            SpriteIndex = _spritesModule.GetFreeSpriteIndex();
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _music.CurrentSong = MusicModule.SongName.None;
            _hitPoints.Value = BossHp;
            SetPhase(Phase.Init);
            sprite.Priority = true;
        }

        protected override void UpdateActive()
        {
            if (_phase.Value == Phase.Init)
            {
                WorldSprite.X = 64;
                WorldSprite.Y = 0;
                ResetTail();

                if(_player.X >= 64)
                {
                    SetPhase(Phase.Fall);
                    _music.CurrentSong = MusicModule.SongName.Threat;
                }
            }
            else if(_phase.Value == Phase.Fall)
            {
                ResetTail();
                UpdatePriorityAndColor();
                if (WorldSprite.Y > BridgeY)
                    SetPhase(Phase.Latch);
            }
            else if(_phase.Value == Phase.Latch)
            {
                UpdatePriorityAndColor();
                UpdateTail_Latched();
                if (WorldSprite.Y < (BridgeY - 24))
                    SetPhase(Phase.Swing);
            }
            else if (_phase.Value == Phase.Detach)
            {
                if (UpdateTail_Retract())
                    SetPhase(Phase.Fall);

                if(WorldSprite.Y < (BridgeY - 48))
                    SetPhase(Phase.Fall);

                if(_levelTimer.IsMod(8))
                    FireBullet();
            }
            else if(_phase.Value == Phase.Swing)
            {
                if(WorldSprite.X > _anchor.Value + 2)
                    _motion.TargetXSpeed = -4;
                else
                    _motion.TargetXSpeed = 4;

                UpdatePriorityAndColor();
                int dy = WorldSprite.Center.Y - BridgeY;
                if (dy < -16)
                {
                    _motion.TargetYSpeed = 60;
                    _motion.YAcceleration = 8;
                }
                else if (dy > 16)
                {
                    _motion.TargetYSpeed = -60;
                    _motion.YAcceleration = 8;

                }

                if(dy < -16 && _levelTimer.IsMod(16))
                    FireBullet();

                if (_levelTimer.Value.IsMod(32))
                {
                    if (_stateTimer.Value == 0 && dy < -16)
                        SetPhase(Phase.Detach);
                    else if(_stateTimer.Value > 0)
                        _stateTimer.Value--;
                }

                UpdateTail_Latched();                
            }


            _motionController.Update();

        }

        private void FireBullet()
        {
            var bullet = _bullets.TryAddNew();
            if (bullet == null)
                return;

            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            bullet.WorldSprite.Center = WorldSprite.Center;

            var target = _player.Center;

            bullet.AcceleratedMotion.TargetTowards(bullet.WorldSprite, target, 60);
            bullet.AcceleratedMotion.XAcceleration = 4;
            bullet.AcceleratedMotion.YAcceleration = 4;
        }

        private void UpdatePriorityAndColor()
        {
            GetSprite().Priority = _motion.YSpeed >= 0;

            int dy = WorldSprite.Center.Y - BridgeY;
            _motion.YAcceleration = 5;

            int dya = Math.Abs(dy);

            if (_motion.YSpeed >= 0)
            {
                _bossBgHandler.BossBgEffectValue = 1;
                if (dya <= 8)
                    SetShade(3);
                else
                    SetShade(2);
            }
            else
            {
                _bossBgHandler.BossBgEffectValue = 0;
                if (dya <= 8)
                    SetShade(0);
                else
                    SetShade(1);
            }           
        }

        private void SetShade(int v)
        {
            var spritePalette = _graphics.GetSpritePalette(SpritePalette.Enemy1);
            spritePalette.SetColor(2, ColorIndex.Green(v).Value);
        }

        private void ResetTail()
        {
            for (int index = 0; index < NumTailSections; index++)
            {
                var tailSprite = _tail.GetWorldSprite(index);
                tailSprite.X = WorldSprite.X;
                tailSprite.Y = WorldSprite.Y;
                tailSprite.Sprite.Palette = SpritePalette.Enemy2;
                tailSprite.Sprite.Priority = false;
                tailSprite.Sprite.Visible = false;
            }
        }

        private void UpdateTail_Latched()
        {
            Point anchor = new Point(_anchor.Value + 2, BridgeY);

            float intervalX = (WorldSprite.X - anchor.X) / (float)NumTailSections;
            float intervalY = (WorldSprite.Y - anchor.Y) / (float)NumTailSections;

            for (int i = 1; i < NumTailSections+1; i++)
            {
                var sprite = _tail.GetSprite(i-1);
                sprite.X = (byte)(anchor.X + (intervalX * i));
                sprite.Y = (byte)(anchor.Y + (intervalY * i));
                sprite.Palette = SpritePalette.Enemy1;
                sprite.Priority = _motion.YSpeed >= 0;
                sprite.Visible = true;
            }
        }

        private bool UpdateTail_Retract()
        {
            bool retracted = true;
            for (int i = 1; i < NumTailSections + 1; i++)
            {
                var sprite = _tail.GetWorldSprite(i - 1);

                var vector = (sprite.Center - WorldSprite.Center).ToVector2();

                if(vector.LengthSquared() < 64)
                {
                    sprite.X = WorldSprite.X;
                    sprite.Y = WorldSprite.Y;
                    sprite.Sprite.Priority = false;
                }    
                else
                {
                    vector.Normalize();
                    vector = vector * 2;
                    sprite.X -= (int)vector.X;
                    sprite.Y -= (int)vector.Y;

                    retracted = false;
                }
            }

            return retracted;
        }

        private void SetPhase(Phase phase)
        {
            _phase.Value = phase;
            _stateTimer.Value = 0;

            if (_phase.Value == Phase.Fall)
            {
                if(_motion.YSpeed > 0)
                    _motion.YSpeed = 0;

                _motion.TargetYSpeed = 60;
                _motion.YAcceleration = 8;

                if (_player.X < WorldSprite.X)
                {
                    _motion.TargetXSpeed = -8;
                    _motion.XAcceleration = 5;
                }
                else
                {
                    _motion.TargetXSpeed = 8;
                    _motion.XAcceleration = 5;
                }
            }
            else if (_phase.Value == Phase.Latch)
            {
                _anchor.Value = (byte)WorldSprite.X;
                _motion.TargetYSpeed = -60;
                _motion.YAcceleration = 8;
            }
            else if (_phase.Value == Phase.Detach)
            {
                _motion.TargetYSpeed = 0;
                _motion.YSpeed = -40;
                _motion.YAcceleration = 4;
            }
            else if (_phase.Value == Phase.Swing)
            {
                _motion.TargetXSpeed = 0;
                _motion.XAcceleration = 5;
                _stateTimer.Value = (byte)(2 + (_rng.Generate(3)*3));
            }
        }

        protected override void UpdateDying()
        {
            if (_hitPoints.Value > 0)
            {
                base.UpdateDying();
                if (WorldSprite.Status == WorldSpriteStatus.Active)
                {
                    GetSprite().Palette = Palette;
                    SetPhase(Phase.Fall);
                }
                return;
            }

            if (_phase.Value < Phase.Dying)
            {
                _music.CurrentSong = MusicModule.SongName.None;

                _stateTimer.Value = 0;
                _phase.Value = Phase.Dying;
            }
            else if(_phase.Value == Phase.Dying)
            {
                if (_levelTimer.IsMod(8))
                {
                    CreateExplosion();
                    CreateExplosion();
                    CreateExplosion();

                    if (_stateTimer.Value < NumTailSections)
                        _tail.Erase(_stateTimer.Value);

                    _stateTimer.Value++;

                    if (_stateTimer.Value == 0)
                    {
                        WorldSprite.Visible = false;
                        ResetTail();
                        SetPhase(Phase.Dead);
                    }
                }
            }
            else if(_phase.Value == Phase.Dead)
            {
                if(_player.X > 120)
                    _exitModule.GotoNextLevel();
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
            bullet.Explode(true);
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (_phase.Value >= Phase.Dying)
                return false;

            if (base.CollidesWithPlayer(player))
                return true;

            var partIndex = _levelTimer.Value % NumTailSections;

            var part = _tail.GetWorldSprite(partIndex);

            return player.CollidesWith(part);
        } 
    }
}
