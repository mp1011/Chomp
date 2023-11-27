using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level1BossController : LevelBossController
    {
        private enum Phase : byte
        {
            Init,
            Lightning,
            BossAppear,
            Float,
            BeforeFireballAttack,
            FireballAttack,
            Rush,
            FireRain,
            Dying,
            FadeOut
        }

        private const byte BossLightningAppearValue = 100;
        private const byte FloatSpeed = 20;
        private const byte FloatTurnAngle = 16;

        private GameByteEnum<Phase> _phase;
        private GameByte _jawSpriteIndex;
        private GameByte _jawPosition;
        private SpriteDefinition _jawSpriteDefinition;

        protected override string BossTiles { get; } =
            @"13335
              98BBD
              67BBA
              00CA0";

        protected override string BlankBossTiles { get; } =
            @"00000
              00000
              00000
              00000";

        public Level1BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _phase = new GameByteEnum<Phase>(memoryBuilder.AddByte());
            _jawSpriteIndex = memoryBuilder.AddByte();
            _jawSpriteDefinition = new SpriteDefinition(SpriteType.BossJaw, memoryBuilder.Memory);
            _internalTimer = memoryBuilder.AddByte();
            _jawPosition = memoryBuilder.AddByte();
        }

        protected override void UpdatePartPositions()
        {
            _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 66);

            var sprite = GetSprite();
            var jawSprite = _spritesModule.GetSprite(_jawSpriteIndex);
            jawSprite.X = (byte)(sprite.X - 7);
            jawSprite.Y = (byte)(sprite.Y + 8 + _jawPosition.Value);
        }
        private void PositionBossAbovePlayer()
        {
            WorldSprite.X = _player.X + 16;
            int maxX = (_tileModule.NameTable.Width - 4) * _spritesModule.Specs.TileWidth;
            if (WorldSprite.X > maxX)
                WorldSprite.X = maxX;

            WorldSprite.Y = 80;

            Visible = true;
            var jawSprite = _spritesModule.GetSprite(_jawSpriteIndex);
            jawSprite.Visible = true;

        }
        private void CreateBoss()
        {
            _paletteModule.BgColor = ColorIndex.Black;
            SetBossTiles();

            _motion.XAcceleration = 10;
            _motion.YAcceleration = 10;
            _motion.XSpeed = 0;
            _motion.YSpeed = 0;

            _jawSpriteIndex.Value = _spritesModule.GetFreeSpriteIndex();
            var jawSprite = _spritesModule.GetSprite(_jawSpriteIndex);
            jawSprite.Tile = _spriteTileTable.GetTile(SpriteTileIndex.Enemy2);
            jawSprite.SizeX = _jawSpriteDefinition.SizeX;
            jawSprite.SizeY = _jawSpriteDefinition.SizeY;
            jawSprite.Tile2Offset = 0;
            jawSprite.Visible = true;
            jawSprite.Palette = 2;
            jawSprite.FlipX = false;

            HideBoss();

            WorldSprite.FlipX = false;
        }


        private void BossTest()
        {
            CreateBoss();
            PositionBossAbovePlayer();
            _motion.SetXSpeed(20);

            _internalTimer.Value = 0;
            _phase.Value = Phase.FireRain;

            Theme theme = new Theme(_graphicsModule.GameSystem.Memory, ThemeType.PlainsBoss);
            var targetSpritePalette = _paletteModule.GetPalette(theme.Enemy1);
            var targetBossPalette = _paletteModule.GetPalette(theme.Background2);

            var spritePalette = _graphicsModule.GetSpritePalette(2);
            spritePalette.SetColor(1, (byte)targetSpritePalette.GetColorIndex(1));
            spritePalette.SetColor(2, (byte)targetSpritePalette.GetColorIndex(2));
            spritePalette.SetColor(3, (byte)targetSpritePalette.GetColorIndex(3));

            var bossPalette = _paletteModule.BgPalette2;
            bossPalette.SetColor(1, (byte)targetBossPalette.GetColorIndex(1));
            bossPalette.SetColor(2, (byte)targetBossPalette.GetColorIndex(2));
            bossPalette.SetColor(3, (byte)targetBossPalette.GetColorIndex(3));
        }


        protected override void UpdateActive()
        {
            if (_phase.Value >= Phase.BossAppear || _internalTimer.Value >= BossLightningAppearValue)
            {
                if (_phase.Value != Phase.FireRain)
                {
                    if (WorldSprite.Y < 64)
                        WorldSprite.Y = 64;
                }

                if (WorldSprite.X > 128)
                    WorldSprite.X = 128;

                UpdatePartPositions();
            }

            if (_phase.Value == Phase.Init)
            {
                CollisionEnabled = false;
                SetBossBackgroundEnd(2);

                _musicModule.CurrentSong = MusicModule.SongName.None;
                WorldSprite.X = 0;
                WorldSprite.Y = 0;

                _paletteModule.BgColor = ColorIndex.Black;
                if (_player.X > 32)
                {
                    _phase.Value = Phase.Lightning;
                }

                // BossTest();
            }
            else if (_phase.Value == Phase.Lightning)
            {
                byte strikeValue = (byte)(_internalTimer.Value % 96);

                if (strikeValue == 0)
                {
                    if (_internalTimer.Value >= BossLightningAppearValue)
                    {
                        PositionBossAbovePlayer();
                    }

                    _audioService.PlaySound(ChompAudioService.Sound.Lightning);
                }

                if (strikeValue <= 7)
                    _paletteModule.BgColor = ColorIndex.LightGray(7 - strikeValue).Value;
                else if (strikeValue <= 14)
                    _paletteModule.BgColor = ColorIndex.LightGray(14 - strikeValue).Value;
                else
                    _paletteModule.BgColor = ColorIndex.Black;

                if (_levelTimer.IsMod(2))
                {
                    _internalTimer.Value++;

                    if (_internalTimer.Value == BossLightningAppearValue)
                    {
                        CreateBoss();
                        PositionBossAbovePlayer();
                    }

                    if (_internalTimer.Value == 0)
                    {
                        _musicModule.CurrentSong = MusicModule.SongName.Nemesis;
                        _phase.Value = Phase.BossAppear;
                        _internalTimer.Value = 0;
                    }
                }
            }

            if (_phase.Value == Phase.BossAppear)
            {
                PositionBossAbovePlayer();

                if (_levelTimer.IsMod(16))
                {
                    Theme theme = new Theme(_graphicsModule.GameSystem.Memory, ThemeType.PlainsBoss);
                    var targetSpritePalette = _paletteModule.GetPalette(theme.Enemy1);
                    var targetBossPalette = _paletteModule.GetPalette(theme.Background2);

                    var spritePalette = _graphicsModule.GetSpritePalette(2);
                    _paletteModule.FadeColor(spritePalette, targetSpritePalette, 1);
                    _paletteModule.FadeColor(spritePalette, targetSpritePalette, 2);
                    _paletteModule.FadeColor(spritePalette, targetSpritePalette, 3);

                    var bossPalette = _paletteModule.BgPalette2;
                    _paletteModule.FadeColor(bossPalette, targetBossPalette, 1);
                    _paletteModule.FadeColor(bossPalette, targetBossPalette, 2);
                    _paletteModule.FadeColor(bossPalette, targetBossPalette, 3);

                    _internalTimer.Value++;
                }

                if (_internalTimer.Value == 8)
                {
                    _phase.Value = Phase.Float;
                    _internalTimer.Value = 0;
                }

            }
            else if (_phase.Value == Phase.Float)
            {
                CollisionEnabled = true;
                if (_jawPosition.Value > 0)
                    _jawPosition.Value--;

                int targetX = _player.X + 16;

                int maxX = (_tileModule.NameTable.Width - 4) * _spritesModule.Specs.TileWidth;
                if (targetX > maxX)
                    targetX = maxX;

                if (_levelTimer.IsMod(8))
                    _motion.TurnTowards(WorldSprite, new Point(targetX, 80), FloatTurnAngle, FloatSpeed);

                _motionController.Update();

                if (_levelTimer.IsMod(8))
                {
                    _internalTimer.Value++;
                    if (_internalTimer.Value == 50)
                    {
                        _phase.Value = Phase.BeforeFireballAttack;
                        _internalTimer.Value = 0;
                    }
                }
            }
            else if (_phase.Value == Phase.BeforeFireballAttack)
            {
                _motion.TargetXSpeed = 0;
                _motion.TargetYSpeed = 0;
                _motion.XAcceleration = 1;
                _motion.YAcceleration = 1;

                if (_jawPosition.Value < 4 && _levelTimer.IsMod(8))
                    _jawPosition.Value++;

                _motionController.Update();

                if (_motion.XSpeed == 0 && _motion.YSpeed == 0)
                {
                    _internalTimer.Value++;
                    if (_internalTimer.Value == 50)
                        _phase.Value = Phase.FireballAttack;
                }
            }
            else if (_phase.Value == Phase.FireballAttack)
            {
                if (_jawPosition.Value < 4)
                    _jawPosition.Value++;

                _motion.SetYSpeed(20);

                if (WorldSprite.X < _player.X)
                    _motion.SetXSpeed(20);

                _motionController.Update();

                if (_levelTimer.IsMod(32))
                    FireBullet();

                if (WorldSprite.Y >= ((_tileModule.NameTable.Height - 6) * _tileModule.Specs.TileHeight))
                {
                    _internalTimer.Value = 0;
                    _phase.Value = Phase.Rush;
                }
            }
            else if (_phase.Value == Phase.Rush)
            {
                _motionController.Update();

                if (_internalTimer.Value < 25 && _levelTimer.IsMod(32))
                    FireBullet();

                if (_internalTimer.Value < 100)
                {
                    _motion.TargetXSpeed = 0;
                    _motion.TargetYSpeed = 0;
                    _motion.XAcceleration = 8;
                    _motion.YAcceleration = 8;

                    _internalTimer.Value++;
                }
                else
                {
                    if (_jawPosition.Value > 0 && _levelTimer.IsMod(16))
                    {
                        _jawPosition.Value--;
                    }

                    _motion.TargetXSpeed = -50;
                    _motion.XAcceleration = 8;

                    if (WorldSprite.X < 8)
                    {
                        _internalTimer.Value = 0;
                        _phase.Value = Phase.FireRain;
                    }
                }
            }
            else if (_phase.Value == Phase.FireRain)
            {
                _dynamicBlockController.PositionFreeCoinBlocksNearPlayer(
                    (byte)(_player.X / _spritesModule.Specs.TileWidth),
                    (byte)(_spritesModule.Specs.NameTableHeight - 6));

                if (_internalTimer.Value == 0)
                {
                    CollisionEnabled = false;
                    _audioService.PlaySound(ChompAudioService.Sound.Lightning);
                    _internalTimer.Value++;
                    _motion.SetXSpeed(0);
                    _motion.SetYSpeed(0);
                    HideBoss();

                    SetBossBackgroundEnd(4);
                }


                if (_internalTimer.Value <= 7)
                    _paletteModule.BgColor = ColorIndex.LightGray(7 - _internalTimer.Value).Value;
                else if (_internalTimer <= 14)
                    _paletteModule.BgColor = ColorIndex.LightGray(14 - _internalTimer.Value).Value;
                else
                {
                    _paletteModule.BgColor = ColorIndex.Black;
                    PositionBossAbovePlayer();

                    Visible = false;
                    var jawSprite = _spritesModule.GetSprite(_jawSpriteIndex);
                    jawSprite.Visible = false;
                }

                if (_internalTimer < 15)
                {
                    if (_levelTimer.IsMod(8))
                        _internalTimer.Value++;
                }
                else
                {
                    if (_levelTimer.IsMod(32))
                    {
                        _internalTimer.Value++;

                        if (_internalTimer.Value == 45)
                        {
                            _phase.Value = Phase.BossAppear;

                            Visible = true;
                            var jawSprite = _spritesModule.GetSprite(_jawSpriteIndex);
                            jawSprite.Visible = true;

                            _dynamicBlockController.ResetCoinsForLevelBoss();
                            SetBossBackgroundEnd(2);

                            SetBossTiles();
                            _internalTimer.Value = 0;
                        }
                        else if (_internalTimer.Value >= 20 && _internalTimer.Value < 40)
                        {
                            CreateFirebomb();
                        }
                    }
                }
            }
        }
        private void FireBullet()
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet != null)
            {
                bullet.WorldSprite.TileIndex = SpriteTileIndex.Extra1;
                bullet.WorldSprite.X = WorldSprite.X - 8;
                bullet.WorldSprite.Y = WorldSprite.Y + 8;
                bullet.WorldSprite.FlipX = true;
                bullet.Motion.XSpeed = -40;
                _audioService.PlaySound(ChompAudioService.Sound.Fireball);
            }
        }

        private ISpriteController CreateFirebomb()
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet != null)
            {
                bullet.WorldSprite.TileIndex = SpriteTileIndex.Extra2;
                bullet.WorldSprite.X = WorldSprite.X + 16;
                bullet.WorldSprite.Y = WorldSprite.Y + 8;
                _audioService.PlaySound(ChompAudioService.Sound.Fireball);

                bullet.WorldSprite.Y = 64;
                bullet.WorldSprite.X = _player.X;
                bullet.Motion.YSpeed = 40;
            }

            return bullet;
        }

        protected override void UpdateDying()
        {
            if (_hitPoints.Value > 0)
            {
                base.UpdateDying();
                GetSprite().Palette = Palette;
                return;
            }

            if (_phase.Value < Phase.Dying)
            {
                _internalTimer.Value = 0;
                _phase.Value = Phase.Dying;
            }

            UpdatePartPositions();

            if (_phase.Value == Phase.Dying)
            {
                CollisionEnabled = false;

                if (_internalTimer.Value == 0)
                {
                    _musicModule.CurrentSong = MusicModule.SongName.None;
                    _motion.SetXSpeed(0);
                    _motion.SetYSpeed(0);
                    _internalTimer.Value++;
                }

                if (WorldSprite.Y > 84)
                {
                    _motion.SetYSpeed(-10);
                    _motionController.Update();
                }
                else
                    _motion.SetYSpeed(0);

                if (_levelTimer.IsMod(16))
                {
                    _internalTimer.Value++;
                    CreateExplosion();
                }

                if (_internalTimer.Value == 20)
                {
                    _internalTimer.Value = 0;
                    _phase.Value = Phase.FadeOut;
                }
            }
            else if (_phase.Value == Phase.FadeOut)
            {
                if (_internalTimer.Value == 0)
                {
                    _audioService.PlaySound(ChompAudioService.Sound.Lightning);
                    HideBoss();
                    Visible = false;
                    _spritesModule.GetSprite(_jawSpriteIndex).Visible = false;
                    _internalTimer.Value = 1;
                    _bossDeathTimer.Value = 1;
                }

                if (_internalTimer.Value <= 25)
                {
                    if (_levelTimer.IsMod(2))
                    {
                        _bossDeathTimer.Value++;
                        if (_bossDeathTimer.Value == 255)
                            _bossDeathTimer.Value = 1;
                    }
                }
                else
                {
                    _bossDeathTimer.Value = 0;
                }

                if (_internalTimer.Value <= 7)
                {
                    _paletteModule.BgColor = ColorIndex.LightGray(7 - _internalTimer.Value).Value;
                    if (_levelTimer.IsMod(8))
                        _internalTimer.Value++;
                }
                else if (_internalTimer.Value <= 25)
                {
                    if (_levelTimer.IsMod(8))
                    {
                        _internalTimer.Value++;
                        int fadeStart = 10;
                        if (_internalTimer.Value >= fadeStart && _internalTimer.Value < fadeStart + 14)
                        {
                            var bossPalette = _paletteModule.BgPalette2;
                            byte color = (byte)(_internalTimer.Value - fadeStart);
                            if (color > 8)
                                color = 8;
                            bossPalette.SetColor(1, color);
                            bossPalette.SetColor(2, color);
                            bossPalette.SetColor(3, color);
                        }
                    }
                }
                else
                {
                    EraseBossTiles();

                    if (_internalTimer.Value <= 32)
                    {
                        byte skyColor = (byte)(32 - _internalTimer.Value);
                        _paletteModule.BgColor = skyColor;
                    }
                    else
                    {
                        _paletteModule.BgColor = 0;
                    }

                    if (_levelTimer.IsMod(16))
                        _internalTimer.Value++;

                    if (_internalTimer.Value == 36)
                    {
                        Destroy();
                        return;
                    }
                }
            }
        }

    }
}
