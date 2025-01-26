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
        private GameBit _hornClosing;
        private BossPart _leg1, _leg2, _leg3, _leftHorn, _rightHorn;
        private MaskedByte _counter;

        private const int GroundY = 100;
        private const int BuriedY = 133;
        private const int FullBuriedY = 136;
        private const int LegGroundY = 12;
        private const int Leg1RestX = -10;
        private const int Leg2RestX = 2;
        private const int Leg3RestX = 14;


        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        public Level4BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));
            _hornClosing = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);
            _counter = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left3, memoryBuilder.Memory, 5);
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
            Emerge,
            Bait,
            Trap,
            LeapOut,
            Land,
            WalkLeft,
            WalkRight,
            Jump,
            Hurt,
            Dying1,
            Dying2
        }

        protected override int BossHP => 4;

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
            _phase.Value = p;
            _stateTimer.Value = 0;

            if(p == Phase.BeforeBoss)
            {
                _gameModule.BossBackgroundHandler.ShowCoins = false;
                _paletteModule.BgColor = ColorIndex.Black;
                HideBoss();
            }
            else if(p == Phase.Init)
            {
                _counter.Value = 0;
                GameDebug.Watch1 = new DebugWatch("Boss X", () => WorldSprite.X);
                GameDebug.Watch2 = new DebugWatch("Boss Y", () => WorldSprite.Y);
                GameDebug.Watch3 = new DebugWatch("State Timer", () => _stateTimer.Value);

                SetBossTiles();              
                SetupBossParts();
            }
            else if(p == Phase.Emerge)
            {
                WorldSprite.Y = FullBuriedY;
                WorldSprite.X = _player.X + 30;

                _leftHorn.Sprite.Priority = false;
                _rightHorn.Sprite.Priority = false;
            }
            else if(p == Phase.Bait)
            {
                _gameModule.BossBackgroundHandler.ShowCoins = true;
                CreateCoins(4);
            }
            else if(p == Phase.LeapOut)
            {
                _gameModule.BossBackgroundHandler.ShowCoins = false;
                _dynamicBlockController.ResetCoinsForLevelBoss();
                _musicModule.CurrentSong = MusicModule.SongName.Nemesis;
                _counter.Value = 0;
                _motion.YSpeed = -80;
                _motion.TargetYSpeed = 60;
                _motion.YAcceleration = 4;
            }
            else if(p == Phase.Jump)
            {
                _counter.Value = 0;
            }
            else if(p == Phase.Hurt)
            {
                _motion.SetXSpeed(0);
                _motion.YSpeed = -80;
                _motion.TargetYSpeed = 60;
                _motion.YAcceleration = 4;

                var bossPalette = _paletteModule.BgPalette2;
                bossPalette.SetColor(1, ColorIndex.Red3);
                bossPalette.SetColor(2, ColorIndex.Red2);
                bossPalette.SetColor(3, ColorIndex.Red1);
            }

        }

        private void CreateCoins(int xOffset)
        {
            var bullet = FireArcBullet(0);
            if (bullet == null)
                return;

            bullet.WorldSprite.Y = 115;
            bullet.WorldSprite.X += xOffset;
            bullet.Explode();
        }

        private bool MoveLeg1(Direction targetDirection) =>
            MoveLeg(_leg1, targetDirection, Leg1RestX, LegGroundY);

        private bool MoveLeg2(Direction targetDirection) =>
            MoveLeg(_leg2, targetDirection, Leg2RestX, LegGroundY);

        private bool MoveLeg3(Direction targetDirection) =>
            MoveLeg(_leg3, targetDirection, Leg3RestX, LegGroundY);

        private bool MoveLeg(BossPart leg, Direction targetDirection, int restX, int restY)
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
            _leg1.XOffset = Leg1RestX;
            _leg1.YOffset = LegGroundY;
            leg1Sprite.Visible = true;

            var leg2Sprite = _leg2.PrepareSprite(SpriteTileIndex.Enemy2);
            leg2Sprite.Tile2Offset = 1;
            leg2Sprite.FlipX = false;
            _leg2.XOffset = Leg2RestX;
            _leg2.YOffset = LegGroundY;
            leg2Sprite.Visible = true;

            var leg3Sprite = _leg3.PrepareSprite(SpriteTileIndex.Enemy2);
            leg3Sprite.Tile2Offset = 1;
            leg3Sprite.FlipX = true;
            _leg3.XOffset = Leg3RestX;
            _leg3.YOffset = LegGroundY;
            leg3Sprite.Visible = true;

            var horn1Sprite = _leftHorn.PrepareSprite(SpriteTileIndex.Enemy1);
            horn1Sprite.Tile2Offset = 1;
            horn1Sprite.FlipX = false;
            _leftHorn.XOffset = -8;
            _leftHorn.YOffset = -20;
            horn1Sprite.Visible = true;

            var horn2Sprite = _rightHorn.PrepareSprite(SpriteTileIndex.Enemy1);
            horn2Sprite.Tile2Offset = 1;
            horn2Sprite.FlipX = true;
            _rightHorn.XOffset = 10;
            _rightHorn.YOffset = -20;
            horn2Sprite.Visible = true;
        }


        protected override void UpdateActive()
        {
            if (_phase.Value == Phase.BeforeBoss)
            {
                _musicModule.CurrentSong = MusicModule.SongName.None;
                if (_player.X >= 40)
                    SetPhase(Phase.Init);
                WorldSprite.X = 64;
                WorldSprite.Y = 100;
            }
            else if (_phase.Value == Phase.Init)
            {
                WorldSprite.Visible = false;
                FadeIn();
                SetPhase(Phase.Emerge);
            }
            else if (_phase.Value == Phase.Emerge)
            {
                FadeIn();
                _motion.SetYSpeed(-5);

                if (WorldSprite.Y < BuriedY)
                {
                    _motion.SetYSpeed(0);
                    WorldSprite.Y = BuriedY;
                    SetPhase(Phase.Bait);
                }
            }
            else if (_phase.Value == Phase.Bait)
            {
                PositionFreeCoinBlocks(WorldSprite.X);

                MoveHorns();

                if (_player.XDistanceTo(WorldSprite) <= 4 &&
                    _player.Y >= 110)
                {
                    SetPhase(Phase.Trap);
                }
            }
            else if (_phase.Value == Phase.Trap)
            {
                PositionFreeCoinBlocks(WorldSprite.X);

                int hornDistance = _rightHorn.XOffset - _leftHorn.XOffset;

                if (_stateTimer.Value < 5)
                {
                    if (_levelTimer.IsMod(4))
                        _stateTimer.Value++;
                }
                else if (_stateTimer.Value == 5)
                {
                    if (_levelTimer.IsMod(2))
                    {
                        if (hornDistance < 20)
                        {
                            _rightHorn.XOffset++;
                            _leftHorn.XOffset--;
                        }
                        else
                        {
                            _stateTimer.Value++;
                        }
                    }
                }
                else if (_stateTimer.Value == 6)
                {
                    if (_levelTimer.IsMod(2))
                    {
                        if (hornDistance > 8)
                        {
                            _rightHorn.XOffset--;
                            _leftHorn.XOffset++;
                        }
                        else
                        {
                            _stateTimer.Value++;
                        }
                    }
                }
                else if (_stateTimer.Value < 11)
                {
                    if (_levelTimer.IsMod(16))
                    {
                        _stateTimer.Value++;
                    }
                }
                else if (_stateTimer.Value == 11)
                {
                    FireArcBullet((_rng.Generate(4) - 8) * 4);
                    FireArcBullet((_rng.Generate(4) - 8) * 4);
                    FireArcBullet((_rng.Generate(4) - 8) * 4);
                    FireArcBullet((_rng.Generate(4) - 8) * 4);

                    _stateTimer.Value++;
                }
                else
                {
                    if (_levelTimer.IsMod(8))
                    {
                        if (hornDistance < 16)
                        {
                            _rightHorn.XOffset++;
                            _leftHorn.XOffset--;
                        }
                        else
                        {
                            _counter.Value++;
                            if (_counter.Value == 4)
                                SetPhase(Phase.LeapOut);
                            else
                                SetPhase(Phase.Bait);
                        }
                    }
                }

            }
            else if (_phase.Value == Phase.LeapOut)
            {
                if (_motionController.Motion.YSpeed > 0 && WorldSprite.Y >= GroundY)
                {
                    _motionController.Motion.SetYSpeed(0);
                    WorldSprite.Y = GroundY;
                    SetPhase(Phase.Land);
                }
            }
            else if (_phase.Value == Phase.Land)
            {
                PlaceOnGround(_leg1);
                PlaceOnGround(_leg2);
                PlaceOnGround(_leg3);

                if (_stateTimer.Value == 0)
                {
                    _motion.SetYSpeed(10);

                    if (WorldSprite.Y >= GroundY + 4)
                    {
                        _stateTimer.Value = 1;
                        _motion.SetYSpeed(-10);
                    }
                }
                else
                {
                    if (WorldSprite.Y < GroundY)
                    {
                        WorldSprite.Y = GroundY;
                        _motion.SetYSpeed(0);
                        SetPhase(Phase.WalkLeft);
                    }
                }
            }
            else if (_phase.Value == Phase.Jump)
            {
                if (_motion.YSpeed < 0 && WorldSprite.Y < GroundY - 8 && !_gameModule.BossBackgroundHandler.ShowCoins)
                {
                    _gameModule.BossBackgroundHandler.ShowCoins = true;
                    PositionFreeCoinBlocks(WorldSprite.X);
                    CreateCoins(0);
                    CreateCoins(-4);
                    CreateCoins(4);
                }
                else if (_motion.YSpeed > 0 && WorldSprite.Y >= GroundY - 8 && _gameModule.BossBackgroundHandler.ShowCoins)
                {
                    _gameModule.BossBackgroundHandler.ShowCoins = false;
                    _dynamicBlockController.ResetCoinsForLevelBoss();
                }

                if (_stateTimer.Value == 0)
                {
                    PlaceOnGround(_leg1);
                    PlaceOnGround(_leg2);
                    PlaceOnGround(_leg3);
                    _motion.SetXSpeed(0);
                    _motion.SetYSpeed(0);
                    WorldSprite.Y = GroundY;
                    _stateTimer.Value = 1;
                }
                else if (_stateTimer.Value == 1)
                {
                    _motion.SetYSpeed(10);
                    PlaceOnGround(_leg1);
                    PlaceOnGround(_leg2);
                    PlaceOnGround(_leg3);

                    if (WorldSprite.Y >= GroundY + 4)
                    {
                        _stateTimer.Value = 2;
                        _motion.YSpeed = -80;
                        _motion.TargetYSpeed = 60;
                        _motion.YAcceleration = 4;

                        if (WorldSprite.X > 70)
                        {
                            _motion.XSpeed = -40;
                        }
                        else if (WorldSprite.X < 30)
                        {
                            _motion.XSpeed = 40;
                        }
                        else
                        {
                            _motion.XSpeed = _rng.Generate(1) switch {
                                0 => -40,
                                _ => 40,
                            };
                        }

                        _motion.TargetXSpeed = 0;
                        _motion.XAcceleration = 1;
                    }
                }
                else if (_stateTimer.Value == 2)
                {
                    MoveLeg1(Direction.Down);
                    MoveLeg2(Direction.Down);
                    MoveLeg3(Direction.Down);

                    if (WorldSprite.Y > GroundY && _motion.YSpeed > 0)
                    {
                        WorldSprite.Y = GroundY;
                        _motion.SetYSpeed(0);
                        SetPhase(Phase.Land);
                    }
                }
            }
            else if (_phase.Value == Phase.WalkLeft)
            {
                MoveHorns();
                if (WorldSprite.X < 50)
                {
                    _counter.Value++;
                    if (_counter.Value == 2)
                        SetPhase(Phase.Jump);
                    else
                        SetPhase(Phase.WalkRight);
                }

                WalkLeft();

                FireBulletsForWalkingPhase();
            }
            else if (_phase.Value == Phase.WalkRight)
            {
                MoveHorns();
                if (WorldSprite.X > 75)
                {
                    _counter.Value++;
                    if (_counter.Value == 2)
                        SetPhase(Phase.Jump);
                    else
                        SetPhase(Phase.WalkLeft);
                }

                WalkRight();

                FireBulletsForWalkingPhase();
            }
            else if (_phase.Value == Phase.Hurt)
            {
                if (_levelTimer.IsMod(8))
                {
                    FadeIn();
                    _stateTimer.Value++;
                }

                if (WorldSprite.Y >= GroundY)
                {
                    _motionController.Motion.SetYSpeed(0);
                    WorldSprite.Y = GroundY;
                }

                if (_stateTimer.Value == 10)
                    SetPhase(Phase.Jump);
            }

            _motionController.Update();
            _position.X = (byte)(WorldSprite.X - 16 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 77);

            if(_phase.Value >= Phase.Init)
                UpdatePartPositions();
        }

        private void FireBulletsForWalkingPhase()
        {
            if (_stateTimer.Value != 1 || !_levelTimer.IsMod(4))
                return;

            if(_bulletControllers.ActiveCount <= 3)
                FireAimedBullet();

        }

        private void MoveHorns()
        {
            int hornDistance = _rightHorn.XOffset - _leftHorn.XOffset;
            if (hornDistance < 16)
                _hornClosing.Value = false;
            else if (hornDistance >= 21)
                _hornClosing.Value = true;

            if (_levelTimer.IsMod(8))
            {
                if (_hornClosing.Value)
                {
                    _rightHorn.XOffset--;
                    _leftHorn.XOffset++;
                }
                else
                {
                    _rightHorn.XOffset++;
                    _leftHorn.XOffset--;
                }
            }
        }

        private void PlaceOnGround(BossPart leg)
        {
            int dipAmount = WorldSprite.Y - GroundY;
            if (dipAmount < 0)
                dipAmount = 0;

            leg.YOffset = LegGroundY - dipAmount;
        }

        private void WalkRight()
        {
            if (!_levelTimer.IsMod(2))
                return;

            if (_stateTimer.Value == 0)
            {

                var leg1 = MoveLeg1(Direction.Down);
                var leg2 = MoveLeg2(Direction.Down);
                var leg3 = MoveLeg3(Direction.Down);

                if (leg1 && leg2 && leg3)
                {
                    _stateTimer.Value = 1;
                }

                WorldSprite.Y = GroundY;
            }
            else if (_stateTimer.Value == 1)
            {
                int leg1X = _leg1.WorldSprite.X;
                int leg1Y = _leg1.WorldSprite.Y;
                var leg1 = MoveLeg1(Direction.Left);
                var leg2 = MoveLeg2(Direction.Down);
                var leg3 = MoveLeg3(Direction.Left);

                UpdatePartPositions();
                FixLegPosition(_leg1, leg1X, leg1Y);

                if (leg1 && leg2 && leg3)
                    _stateTimer.Value = 2;
            }
            else if (_stateTimer.Value == 2)
            {
                var leg1 = MoveLeg1(Direction.Right);
                var leg2 = MoveLeg2(Direction.Up);
                var leg3 = MoveLeg3(Direction.Right);

                UpdatePartPositions();

                if (leg1 && leg2 && leg3)
                    _stateTimer.Value = 1;
            }
        }


        private void WalkLeft()
        {    
            if (!_levelTimer.IsMod(2))
                return;

            if (_stateTimer.Value == 0)
            {

                var leg1 = MoveLeg1(Direction.Down);
                var leg2 = MoveLeg2(Direction.Down);
                var leg3 = MoveLeg3(Direction.Down);

                if (leg1 && leg2 && leg3)
                {
                    _stateTimer.Value = 1;
                }

                WorldSprite.Y = GroundY;
            }
            else if (_stateTimer.Value == 1)
            {
                int leg1X = _leg1.WorldSprite.X;
                int leg1Y = _leg1.WorldSprite.Y;
                var leg1 = MoveLeg1(Direction.Right);
                var leg2 = MoveLeg2(Direction.Up);
                var leg3 = MoveLeg3(Direction.Right);

                UpdatePartPositions();
                FixLegPosition(_leg1, leg1X, leg1Y);

                if (leg1 && leg2 && leg3)
                    _stateTimer.Value = 2;
            }
            else if (_stateTimer.Value == 2)
            {
                var leg1 = MoveLeg1(Direction.Left);
                var leg2 = MoveLeg2(Direction.Down);
                var leg3 = MoveLeg3(Direction.Left);

                UpdatePartPositions();

                if (leg1 && leg2 && leg3)
                    _stateTimer.Value = 1;
            }
        }

        private void FixLegPosition(BossPart leg, int fixX, int fixY)
        {
            WorldSprite.X += (fixX - leg.WorldSprite.X);
            WorldSprite.Y += (fixY - leg.WorldSprite.Y);
        }

        protected override void UpdateDying()
        {
            if(_phase.Value < Phase.Dying1)
            {
                _phase.Value = Phase.Dying1;
                _musicModule.CurrentSong = MusicModule.SongName.None;
                _stateTimer.Value = 0;
                _counter.Value = 0;
                _motion.SetXSpeed(0);
                _motion.SetYSpeed(40);

                _leftHorn.Sprite.Palette = SpritePalette.Fire;
                _rightHorn.Sprite.Palette = SpritePalette.Fire;
                _leg1.Sprite.Palette = SpritePalette.Fire;
                _leg2.Sprite.Palette = SpritePalette.Fire;
                _leg3.Sprite.Palette = SpritePalette.Fire;
            }

            if (_phase.Value == Phase.Dying1)
            {
                if (_levelTimer.IsMod(8))
                    FadeIn();

                if(_levelTimer.IsMod(4))
                {
                    CreateExplosions();
                }

                if(_levelTimer.IsMod(32))
                {
                    _stateTimer.Value++;
                    var bossPalette = _paletteModule.BgPalette2;
                    bossPalette.SetColor(1, ColorIndex.Red3);
                    bossPalette.SetColor(2, ColorIndex.Red2);
                    bossPalette.SetColor(3, ColorIndex.Red1);
                }

                if(_stateTimer.Value == 8)
                {
                    _phase.Value = Phase.Dying2;
                    _stateTimer.Value = 0;
                    _bossBackgroundHandler.BossBgEffectType = BackgroundEffectType.FadeFromTop;
                    _bossBackgroundHandler.BossBgEffectValue = 0;
                }
            }
            else if (_phase.Value == Phase.Dying2)
            {
                if (_bossBackgroundHandler.BossBgEffectValue < 255)
                {                    
                    _bossBackgroundHandler.BossBgEffectValue++;                    
                    _leftHorn.XOffset -= 1;
                    _rightHorn.XOffset += 1;

                    if (_bossBackgroundHandler.BossBgEffectValue == 8)
                    {
                        _leftHorn.Sprite.Visible = false;
                        _rightHorn.Sprite.Visible = false;
                    }
                }
                else
                {
                    _leg1.Sprite.Visible = false;
                    _leg2.Sprite.Visible = false;
                    _leg3.Sprite.Visible = false;

                    if(_levelTimer.Value.IsMod(8))
                        _stateTimer.Value++;

                    if (_stateTimer.Value == 8)
                    {
                        Destroy();
                        _exitsModule.GotoNextLevel();
                    }
                }
            }


            if (WorldSprite.Y >= GroundY)
            {
                _motionController.Motion.SetYSpeed(0);
                WorldSprite.Y = GroundY;
            }

            _motionController.Update();
            _position.X = (byte)(WorldSprite.X - 16 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 77);

            UpdatePartPositions();
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
            CreateExplosion((WorldSprite.X - 8) + x, WorldSprite.Y + y);
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            if (_phase.Value < Phase.LeapOut)
                return false;

            return bomb.Bounds.Intersects(new Rectangle(WorldSprite.X - 4, WorldSprite.Y, 8, 16));
        }

        public override BombCollisionResponse HandleBombCollision(WorldSprite player)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Lightning);           
            _hitPoints.Value--;
            if (_hitPoints.Value == 0)
                WorldSprite.Status = WorldSpriteStatus.Dying;
            else
                SetPhase(Phase.Hurt);

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

        private BossBulletController FireArcBullet(int xSpeed)
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return null;

            bullet.AcceleratedMotion.SetXSpeed(xSpeed);
            bullet.AcceleratedMotion.YSpeed = -40;
            bullet.AcceleratedMotion.TargetYSpeed = 20;
            bullet.AcceleratedMotion.YAcceleration = 2;

            bullet.DestroyOnTimer = false;
            bullet.DestroyOnCollision = true;
            bullet.WorldSprite.X = WorldSprite.X;
            bullet.WorldSprite.Y = WorldSprite.Y - 18;        
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);

            return bullet;
        }

        private BossBulletController FireAimedBullet()
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return null;

            bullet.DestroyOnTimer = true;
            bullet.DestroyOnCollision = false;

            bullet.WorldSprite.X = WorldSprite.X;
            bullet.WorldSprite.Y = WorldSprite.Y - 18;
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);


            bullet.AcceleratedMotion.YAcceleration = 3;
            bullet.AcceleratedMotion.XAcceleration = 3;
            bullet.AcceleratedMotion.TargetTowards(bullet.WorldSprite, _player.Bounds.Center, 50);

            return bullet;
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (_phase.Value <= Phase.Init)
                return false;

            return player.WorldSprite.Bounds.Intersects(_leftHorn.WorldSprite.Bounds)
                || player.WorldSprite.Bounds.Intersects(_rightHorn.WorldSprite.Bounds)
                || player.WorldSprite.Bounds.Intersects(_leg1.WorldSprite.Bounds)
                || player.WorldSprite.Bounds.Intersects(_leg2.WorldSprite.Bounds)
                || player.WorldSprite.Bounds.Intersects(_leg3.WorldSprite.Bounds);

        }
    }
}
