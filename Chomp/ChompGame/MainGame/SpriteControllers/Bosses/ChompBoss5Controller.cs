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
        public const int BridgeY = 58;

        public const int NumTailSections = 4;
        public const int BossHp = 3;
        private const int Speed = 40;
        private readonly ChompTail _tail;

        private readonly WorldScroller _scroller;
        private readonly EnemyOrBulletSpriteControllerPool<BossBulletController> _bullets;
        private readonly MusicModule _music;
        private readonly Specs _specs;
        private WorldSprite _player;
        private PrecisionMotion _firstTailSectionMotion;
        private CoreGraphicsModule _graphics;
        private BossBackgroundHandler _bossBgHandler;

        enum Phase : byte 
        {
            Init,
            Swing
        }
      
        private GameByteEnum<Phase> _phase;

        protected override bool DestroyBombOnCollision => true;

        public ChompBoss5Controller(WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bullets,
            ChompGameModule gameModule, 
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _graphics = gameModule.GameSystem.CoreGraphicsModule;
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
            SetPhase(Phase.Init);
            sprite.Priority = true;
        }

        protected override void UpdateActive()
        {
            if (_phase.Value == Phase.Init)
            {
                WorldSprite.X = 64;
                WorldSprite.Y = 24;
                ResetTail();
                SetPhase(Phase.Swing);
            }
            else if(_phase.Value == Phase.Swing)
            {
                GetSprite().Priority = _motion.YSpeed >= 0;

                int dy = WorldSprite.Center.Y - BridgeY;
                _motion.YAcceleration = 5;

                int dya = Math.Abs(dy);

                GameDebug.Watch1 = new DebugWatch("DYA", () => dya);
           
                if(_motion.YSpeed >= 0)
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

                if(dy < -16)
                {
                    _motion.TargetYSpeed = 40;
                }
                else if (dy > 16)
                {
                    _motion.TargetYSpeed = -40;
                }

                UpdateTail();
                
            }


            _motionController.Update();

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
            }
        }

        private void UpdateTail()
        {
            Point anchor = new Point(WorldSprite.X+2, BridgeY);

            int intervalX = (WorldSprite.X - anchor.X) / NumTailSections;
            int intervalY = (WorldSprite.Y - anchor.Y) / NumTailSections;

            for (int i = 1; i < NumTailSections+1; i++)
            {
                var sprite = _tail.GetSprite(i-1);
                sprite.X = (byte)(anchor.X + (intervalX * i));
                sprite.Y = (byte)(anchor.Y + (intervalY * i));
                sprite.Priority = _motion.YSpeed >= 0;
            }
        }
      
        private void SetPhase(Phase phase)
        {
            _phase.Value = phase;
            _stateTimer.Value = 0;
        }

        protected override void UpdateDying()
        {
            
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (base.CollidesWithPlayer(player))
                return true;

            var partIndex = _levelTimer.Value % NumTailSections;

            var part = _tail.GetWorldSprite(partIndex);

            return player.CollidesWith(part);
        } 
    }
}
