using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level4BossController : LevelBossController
    {
        private NibbleEnum<Phase> _phase;

        private TwoBitEnum<Direction> _legPosition1, _legPosition2;
        private BossPart _leg1, _leg2, _leg3, _leftHorn, _rightHorn;

        private const int GroundY = 100;
        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        public Level4BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));
            _legPosition1 = new TwoBitEnum<Direction>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 4);
            _legPosition2 = new TwoBitEnum<Direction>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 6);
            memoryBuilder.AddByte();

            _leg1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg3 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leftHorn = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossHorn, memoryBuilder.Memory));
            _rightHorn = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossHorn, memoryBuilder.Memory));
        }

        private enum Phase : byte
        {
            BeforeBoss,
            Init,
            WalkRight,
            WalkLeft
        }

        protected override int BossHP => 5;

        protected override string BossTiles { get; } =
            @"0008AA900
              000J98H00
              0000GH000
              0008AA900
              00BAIIA90
              08IIIIII9
              0JF1E1EFH
              000000000";


        protected override string BlankBossTiles => "0";

        protected override void BeforeInitializeSprite()
        {
            SetPhase(Phase.BeforeBoss);
            base.BeforeInitializeSprite();
        }

        private void SetPhase(Phase p)
        {
            _bossBackgroundHandler.ShowCoins = true;
            _phase.Value = p;
            _stateTimer.Value = 0;

            if(p == Phase.BeforeBoss)
            {
                _paletteModule.BgColor = ColorIndex.Black;
                _legPosition1.Value = Direction.Down;
                _legPosition2.Value = Direction.Down;
            }
            else if(p == Phase.Init)
            {
                GameDebug.Watch1 = new DebugWatch("Boss X", () => WorldSprite.X);
                GameDebug.Watch2 = new DebugWatch("Boss Y", () => WorldSprite.Y);
                GameDebug.Watch3 = new DebugWatch("State Timer", () => _stateTimer.Value);

                SetBossTiles();
              
                SetupBossParts();
                WorldSprite.X = 20;
                WorldSprite.Y = 80;
                HideBoss();
            }
        }

        private bool UpdateLeg(BossPart leg, Direction targetDirection, int restX, int restY)
        {
            return targetDirection switch {
                Direction.Down => MoveLegTowardLinear(leg, restX, restY),
                Direction.Up => MoveLegTowardLinear(leg, restX, restY - 4),
                Direction.Left => RotateLegToward(leg, restX, restY, 270),
                _ => RotateLegToward(leg, restX, restY, 90),
            };
        }

        private bool MoveLegTowardLinear(BossPart leg, int targetX, int targetY)
        {
            leg.XOffset = leg.XOffset.MoveToward(targetX, 1);
            leg.YOffset = leg.YOffset.MoveToward(targetY, 1);

            return leg.XOffset == targetX && leg.YOffset == targetY;
        }

        private bool RotateLegToward(BossPart leg, int restX, int restY, int targetAngle)
        {
            var legPosition = new Point(leg.XOffset - restX, leg.YOffset - restY);
            int degrees = legPosition.Degrees();
            int targetDegrees = degrees.MoveToward(targetAngle, 16);

            if (degrees == targetDegrees)
                return true;

            var newLegPosition = GameMathHelper.PointFromAngle(targetDegrees, 4);
            leg.XOffset = restX + newLegPosition.X;
            leg.YOffset = restY + newLegPosition.Y;

            return false;
        }


        private void SetupBossParts()
        {
            var leg1Sprite = _leg1.PrepareSprite(SpriteTileIndex.Enemy2);
            leg1Sprite.Tile2Offset = 1;
            leg1Sprite.FlipX = false;
            _leg1.XOffset = -10;
            _leg1.YOffset = 12;
            leg1Sprite.Visible = true;

            var leg2Sprite = _leg2.PrepareSprite(SpriteTileIndex.Enemy2);
            leg2Sprite.Tile2Offset = 1;
            leg2Sprite.FlipX = false;
            _leg2.XOffset = 2;
            _leg2.YOffset = 12;
            leg2Sprite.Visible = true;

            var leg3Sprite = _leg3.PrepareSprite(SpriteTileIndex.Enemy2);
            leg3Sprite.Tile2Offset = 1;
            leg3Sprite.FlipX = true;
            _leg3.XOffset = 14;
            _leg3.YOffset = 12;
            leg3Sprite.Visible = true;

            var horn1Sprite = _leftHorn.PrepareSprite(SpriteTileIndex.Enemy1);
            horn1Sprite.Tile2Offset = 1;
            horn1Sprite.FlipX = false;
            _leftHorn.XOffset = -8;
            _leftHorn.YOffset = -18;
            horn1Sprite.Visible = true;

            var horn2Sprite = _rightHorn.PrepareSprite(SpriteTileIndex.Enemy1);
            horn2Sprite.Tile2Offset = 1;
            horn2Sprite.FlipX = true;
            _rightHorn.XOffset = 10;
            _rightHorn.YOffset = -18;
            horn2Sprite.Visible = true;
        }


        protected override void UpdateActive()
        {
            _gameModule.BossBackgroundHandler.ShowCoins = false;
            if(_phase.Value == Phase.BeforeBoss)
            {
                _musicModule.CurrentSong = MusicModule.SongName.None;
              //  if (_player.X >= 40)
                    SetPhase(Phase.Init);
                WorldSprite.X = 64;
                WorldSprite.Y = 100;
            }
            else if(_phase.Value == Phase.Init)
            {
                WorldSprite.Visible = false;
                _musicModule.CurrentSong = MusicModule.SongName.Nemesis;
                SetPhase(Phase.WalkLeft);
                FadeIn();
            }
            else if(_phase.Value == Phase.WalkLeft)
            {
                FadeIn();
                if (WorldSprite.X < 24)
                    SetPhase(Phase.WalkRight);

                WalkLeft();
            }
            else if (_phase.Value == Phase.WalkRight)
            {
                if (WorldSprite.X > 100)
                    SetPhase(Phase.WalkLeft);

                WalkRight();
            }
            _motionController.Update();
            _position.X = (byte)(WorldSprite.X - 16 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 77);

            UpdatePartPositions();
        }

        private void WalkRight()
        {
            if (!_levelTimer.IsMod(4))
                return;

            if(_legPosition1.Value == Direction.Right)
            {
                WorldSprite.Y = 100;

                UpdateLeg(_leg3, _legPosition1.Value, 14, 12);
                UpdateLeg(_leg2, Direction.Left, 2, 12);

                if (UpdateLeg(_leg1, _legPosition1.Value, -10, 12))
                    _legPosition1.Value = Direction.Left;
            }
            else if (_legPosition1.Value == Direction.Left)
            {
                int legX = _leg1.WorldSprite.X;
                int legY = _leg1.WorldSprite.Y;

                UpdateLeg(_leg3, _legPosition1.Value, 14, 12);
                UpdateLeg(_leg2, Direction.Right, 2, 12);

                if (UpdateLeg(_leg1, _legPosition1.Value, -10, 12))
                    _legPosition1.Value = Direction.Right;
                _leg1.UpdatePosition(WorldSprite);
                WorldSprite.X += (legX - _leg1.WorldSprite.X);
                WorldSprite.Y += (legY - _leg1.WorldSprite.Y);

            }
            else
            {
                _legPosition1.Value = Direction.Right;
            }
        }

        private void WalkLeft()
        {
            if (!_levelTimer.IsMod(4))
                return;

            if (_legPosition1.Value == Direction.Left)
            {
                WorldSprite.Y = 100;

                UpdateLeg(_leg3, _legPosition1.Value, 14, 12);
                UpdateLeg(_leg2, Direction.Right, 2, 12);

                if (UpdateLeg(_leg1, _legPosition1.Value, -10, 12))
                    _legPosition1.Value = Direction.Right;
            }
            else if (_legPosition1.Value == Direction.Right)
            {
                int legX = _leg1.WorldSprite.X;
                int legY = _leg1.WorldSprite.Y;

                UpdateLeg(_leg3, _legPosition1.Value, 14, 12);
                UpdateLeg(_leg2, Direction.Left, 2, 12);

                if (UpdateLeg(_leg1, _legPosition1.Value, -10, 12))
                    _legPosition1.Value = Direction.Left;
                _leg1.UpdatePosition(WorldSprite);
                WorldSprite.X += (legX - _leg1.WorldSprite.X);
                WorldSprite.Y += (legY - _leg1.WorldSprite.Y);

            }
            else
            {
                _legPosition1.Value = Direction.Right;
            }
        }

        protected override void UpdateDying()
        {
        }
        private void FadeIn()
        {
            Theme theme = new Theme(_graphicsModule.GameSystem.Memory, ThemeType.CityBoss);
            var targetSpritePalette = _paletteModule.GetPalette(theme.Enemy1);
            var targetBossPalette = _paletteModule.GetPalette(theme.Background2);

            var spritePalette = _graphicsModule.GetSpritePalette(SpritePalette.Enemy1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 2);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 3);

            var bossPalette = _paletteModule.BgPalette2;
            _paletteModule.FadeColor(bossPalette, targetBossPalette, 1);
            _paletteModule.FadeColor(bossPalette, targetBossPalette, 2);
            _paletteModule.FadeColor(bossPalette, targetBossPalette, 3);
        }
        private void CreateExplosions()
        {
            var x = _rng.Generate(5);
            var y = _rng.Generate(4);
            CreateExplosion(WorldSprite.X + 8 + x, WorldSprite.Y + y);
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            return false;
        }

        public override BombCollisionResponse HandleBombCollision(WorldSprite player)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Lightning);           
            _hitPoints.Value--;
            if (_hitPoints.Value == 0)
                WorldSprite.Status = WorldSpriteStatus.Dying;
            return BombCollisionResponse.Destroy;
        }

        protected override void UpdatePartPositions()
        {
            _leg1.UpdatePosition(WorldSprite);
            _leg2.UpdatePosition(WorldSprite);
            _leg3.UpdatePosition(WorldSprite);
            _leftHorn.UpdatePosition(WorldSprite);
            _rightHorn.UpdatePosition(WorldSprite);
        }
    }
}
