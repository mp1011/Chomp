using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level3BossController : LevelBossController
    {
        private BossPart _leftEye, _rightEye, _leftJaw, _rightJaw;
        private NibbleEnum<Phase> _phase;
        private GameBit _leftJawOpen, _rightJawOpen;
        private MaskedByte _eyeFlashTimer;

        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        public Level3BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _leftEye = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _rightEye = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _leftJaw = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossJaw, memoryBuilder.Memory));
            _rightJaw = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossJaw, memoryBuilder.Memory));

            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));
            _leftJawOpen = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit4, memoryBuilder.Memory);
            _rightJawOpen = new GameBit(memoryBuilder.CurrentAddress, Bit.Bit5, memoryBuilder.Memory);
            _eyeFlashTimer = new MaskedByte(memoryBuilder.CurrentAddress, Bit.Left2, memoryBuilder.Memory, leftShift:6);
            memoryBuilder.AddByte();
        }

        private enum Phase : byte
        {
            BeforeBoss,
            Init,
            RightHook,
            LeftHook
        }

        protected override int BossHP => 5;

        protected override string BossTiles { get; } =
            @"BAAAAA9008AAAAAC
              6MMMJII98IIHMMM5
              00BAAIIIIIIAAAC0
              006MJIIIIII4M500
              00000GIIII400000
              000000DE12000000";


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
                SetupBossParts();
                _leftEye.Sprite.Visible = false;
                _rightEye.Sprite.Visible = false;
                _leftJaw.Sprite.Visible = false;
                _rightJaw.Sprite.Visible = false;

                _paletteModule.BgColor = ColorIndex.Black;

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
            else if(p == Phase.RightHook)
            {
                WorldSprite.Y = 62;
                WorldSprite.X = _player.X - 48;
                HideBoss();

                _motion.TargetXSpeed = -80;
                _motion.XSpeed = 80;
                _motion.XAcceleration = 3;

                _motion.YSpeed = 20;
                _motion.TargetYSpeed = 1;
                _motion.YAcceleration = 1;
                _leftJawOpen.Value = true;
                _rightJawOpen.Value = false;
            }
            else if (p == Phase.LeftHook)
            {
                WorldSprite.Y = 62;
                WorldSprite.X = _player.X;
                HideBoss();

                _motion.TargetXSpeed = 80;
                _motion.XSpeed = -80;
                _motion.XAcceleration = 3;

                _motion.YSpeed = 20;
                _motion.TargetYSpeed = 1;
                _motion.YAcceleration = 1;
                _leftJawOpen.Value = false;
                _rightJawOpen.Value = true;
            }
        }

        protected override void UpdateActive()
        {
            if(_eyeFlashTimer.Value > 0)
            {
                if (_levelTimer.IsMod(16))
                    _eyeFlashTimer.Value--;

                if(_eyeFlashTimer.Value == 0)
                {
                    _leftEye.Sprite.Palette = SpritePalette.Enemy1;
                    _rightEye.Sprite.Palette = SpritePalette.Enemy1;
                }
            }
            if(_phase.Value == Phase.BeforeBoss)
            {
                if (_player.X >= 40)
                    SetPhase(Phase.Init);
                WorldSprite.X = 20;
                WorldSprite.Y = 80;
            }
            else if(_phase.Value == Phase.Init)
            {
                // debug
                //FadeIn();
                //var ks = Microsoft.Xna.Framework.Input.Keyboard.GetState();
                //if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B))
                //{
                //    if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                //        WorldSprite.X--;
                //    else if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                //        WorldSprite.X++;
                //    else if (ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.C))
                //        SetBossTiles();
                //}

                SetPhase(Phase.RightHook);
            }
            else if(_phase.Value == Phase.RightHook)
            {
                PositionFreeCoinBlocksNearPlayer();
                if (_stateTimer.Value == 15 || WorldSprite.X <= -55)
                {
                    SetPhase(Phase.LeftHook);
                }
                else if (_levelTimer.IsMod(24))
                {                   
                    if (_stateTimer.Value < 4)
                        FadeIn();
                    else if (_stateTimer.Value >= 8)
                        FadeOut();
                    
                    _stateTimer.Value++;
                }

                if (_stateTimer.Value == 5)
                {
                    _leftJawOpen.Value = false;
                    FireBullet(_leftJaw, -1);
                    FireBullet(_leftJaw, -25);
                    FireBullet(_leftJaw, -40);
                    _stateTimer.Value++;
                }
            }
            else if (_phase.Value == Phase.LeftHook)
            {
                PositionFreeCoinBlocksNearPlayer();
                if (_stateTimer.Value == 15 || WorldSprite.X <= -55)
                {
                    SetPhase(Phase.RightHook);
                }
                else if (_levelTimer.IsMod(24))
                {
                    if (_stateTimer.Value < 4)
                        FadeIn();
                    else if (_stateTimer.Value >= 8)
                        FadeOut();

                    _stateTimer.Value++;
                }
                if (_stateTimer.Value == 5)
                {
                    _rightJawOpen.Value = false;
                    FireBullet(_rightJaw, 1);
                    FireBullet(_rightJaw, 25);
                    FireBullet(_rightJaw, 40);
                    _stateTimer.Value++;
                }
            }

            _motionController.Update();
            _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 66);

            UpdateJaw(_leftJaw, _leftJawOpen.Value);
            UpdateJaw(_rightJaw, _rightJawOpen.Value);

            UpdatePartPositions();

            if (_phase.Value > Phase.Init)
                HideOffscreenBossTiles(7, 71, 16);

            //if (_stateTimer.Value > 1 && _stateTimer.Value < 5 && _levelTimer.IsMod(32))
            //{
            //    FireBullet(_rightJaw, 20);
            //    FireBullet(_rightJaw, 40);
            //    FireBullet(_rightJaw, 80);
            //}
        }

        private void FireBullet(BossPart origin, int xSpeed)
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.WorldSprite.TileIndex = SpriteTileIndex.Extra2;
            bullet.WorldSprite.X = origin.WorldSprite.X;
            bullet.WorldSprite.Y = origin.WorldSprite.Y - 4;
            bullet.WorldSprite.FlipX = xSpeed < 0;

            bullet.AcceleratedMotion.XSpeed = xSpeed;
            bullet.AcceleratedMotion.TargetXSpeed = 0;
            bullet.AcceleratedMotion.XAcceleration = 1;

            bullet.Motion.YSpeed = -10;
            bullet.AcceleratedMotion.TargetYSpeed = 20;
            bullet.AcceleratedMotion.YAcceleration = 2;
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            var x = WorldSprite.X;
            var y = WorldSprite.Y;

            bool eyeHit = CheckEyeHit(_leftEye, bomb) || CheckEyeHit(_rightEye, bomb);

            WorldSprite.X = x;
            WorldSprite.Y = y;
            return eyeHit;
        }

        private bool CheckEyeHit(BossPart eye, WorldSprite bomb)
        {
            WorldSprite.X = eye.WorldSprite.X;
            WorldSprite.Y = eye.WorldSprite.Y;
            return base.CollidesWithBomb(bomb);
        }
        public override BombCollisionResponse HandleBombCollision(WorldSprite player)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Lightning);
            _leftEye.Sprite.Palette = SpritePalette.Fire;
            _rightEye.Sprite.Palette = SpritePalette.Fire;
            _eyeFlashTimer.Value = 3;
            
            return BombCollisionResponse.Destroy;
        }

        private void UpdateJaw(BossPart jaw, bool open)
        {
            if (_levelTimer.IsMod(16))
            {
                var targetOffset = open ? 24 : 20;
                if (jaw.YOffset > targetOffset)
                    jaw.YOffset--;
                else if (jaw.YOffset < targetOffset)
                    jaw.YOffset++;
            }
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

        private void FadeOut()
        {
            Theme theme = new Theme(_graphicsModule.GameSystem.Memory, ThemeType.CityBoss);
            var spritePalette = _graphicsModule.GetSpritePalette(SpritePalette.Enemy1);
            _paletteModule.Darken(spritePalette, 1) ;
            _paletteModule.Darken(spritePalette, 2);
            _paletteModule.Darken(spritePalette, 3);

            var bossPalette = _paletteModule.BgPalette2;
            _paletteModule.Darken(bossPalette, 1);
            _paletteModule.Darken(bossPalette, 2);
            _paletteModule.Darken(bossPalette, 3);
        }

        private void SetupBossParts()
        {
            var leftEyeSprite = _leftEye.PrepareSprite(SpriteTileIndex.Enemy1);
            leftEyeSprite.Tile2Offset = 1;
            leftEyeSprite.FlipX = true;
            _leftEye.XOffset = 14;
            _leftEye.YOffset = 8;

            var rightEyeSprite = _rightEye.PrepareSprite(SpriteTileIndex.Enemy1);
            rightEyeSprite.Tile2Offset = 1;
            rightEyeSprite.FlipX = false;
            _rightEye.XOffset = 26;
            _rightEye.YOffset = 8;

            var leftJawSprite = _leftJaw.PrepareSprite(SpriteTileIndex.Enemy2);
            leftJawSprite.Tile2Offset = 0;
            leftJawSprite.FlipX = false;
            _leftJaw.XOffset = 16;
            _leftJaw.YOffset = 20;

            var rightJawSprite = _rightJaw.PrepareSprite(SpriteTileIndex.Enemy2);
            rightJawSprite.Tile2Offset = 0;
            rightJawSprite.FlipX = true;
            _rightJaw.XOffset = 26;
            _rightJaw.YOffset = 20;

        }


        protected override void UpdatePartPositions()
        {
            WorldSprite.Visible = false;
            _leftEye.UpdatePosition(WorldSprite);
            _rightEye.UpdatePosition(WorldSprite);
            _leftJaw.UpdatePosition(WorldSprite);
            _rightJaw.UpdatePosition(WorldSprite);
        }
    }
}
