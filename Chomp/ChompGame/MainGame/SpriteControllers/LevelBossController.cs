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

namespace ChompGame.MainGame.SpriteControllers
{
    class LevelBossController : EnemyController
    {
        private AcceleratedMotion _motion;
        private IMotionController _motionController;

        private const byte BossLightningAppearValue = 100;
        private const byte FloatSpeed = 20;
        private const byte FloatTurnAngle = 16;

        private DynamicBlockController _dynamicBlockController;
        private SpriteDefinition _jawSpriteDefinition;
        private CoreGraphicsModule _graphicsModule;
        private EnemyOrBulletSpriteControllerPool<BossBulletController> _bulletControllers;
        private TileModule _tileModule;
        private PaletteModule _paletteModule;
        private GameByteGridPoint _position;
        private GameByte _levelBossBackgroundEnd;
        private GameByte _bossDeathTimer;
        private WorldSprite _player;
        private GameByte _jawSpriteIndex;
        private GameByte _jawPosition;
        private GameByte _internalTimer;
        private MusicModule _musicModule;

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

        private GameByteEnum<Phase> _phase;

        public LevelBossController(ChompGameModule gameModule,
            WorldSprite player,
            EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.LevelBoss, SpriteTileIndex.Enemy1, gameModule, memoryBuilder)
        {
            _player = player;
            _dynamicBlockController = gameModule.DynamicBlocksController;
            _bulletControllers = bulletControllers;
            _musicModule = gameModule.MusicModule;
            _graphicsModule = gameModule.GameSystem.CoreGraphicsModule;
            _tileModule = gameModule.TileModule;
            _paletteModule = gameModule.PaletteModule;
            _position = gameModule.BossBackgroundHandler.BossPosition;
            _levelBossBackgroundEnd = gameModule.BossBackgroundHandler.BossBackgroundEnd;
            _bossDeathTimer = gameModule.BossBackgroundHandler.BossDeathTimer;
            _phase = new GameByteEnum<Phase>(memoryBuilder.AddByte());
            _jawSpriteIndex = memoryBuilder.AddByte();
            _jawSpriteDefinition = new SpriteDefinition(SpriteType.BossJaw, memoryBuilder.Memory);
            _internalTimer = memoryBuilder.AddByte();
            _jawPosition = memoryBuilder.AddByte();
        }

        protected override void BeforeInitializeSprite()
        {
            _position.X = 200;
            _position.Y = 16;

            WorldSprite.Y = 80;
            WorldSprite.X = 16;

            _bossDeathTimer.Value = 0;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _hitPoints.Value = 4;
            _state.Value = 0;
        }

        protected override bool HandleDestroy()
        {
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
            else if(_phase.Value == Phase.FadeOut)
            {
                if(_internalTimer.Value==0)
                {
                    _audioService.PlaySound(ChompAudioService.Sound.Lightning);
                    HideBoss();
                    _spritesModule.GetSprite(_jawSpriteIndex).Visible = false;
                    GetSprite().Visible = false;
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
                else if(_internalTimer.Value <= 25)
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
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetBossTiles()
        {
            _tileModule.NameTable.SetFromString(0, 15,
            @"13335
                    9$BBD
                    @#BBA
                    00$D0");
        }

        private void EraseBossTiles()
        {
            _tileModule.NameTable.SetFromString(0, 15,
                   @"00000
                           00000
                           00000
                           00000");
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

        private void HideBoss()
        {
            var spritePalette = _graphicsModule.GetSpritePalette(2);
            spritePalette.SetColor(1, 0);
            spritePalette.SetColor(2, 0);
            spritePalette.SetColor(3, 0);

            var bossPalette = _paletteModule.BgPalette2;
            bossPalette.SetColor(1, 0);
            bossPalette.SetColor(2, 0);
            bossPalette.SetColor(3, 0);
        }

        private void PositionBossAbovePlayer()
        {
            WorldSprite.X = _player.X + 16;
            int maxX = (_tileModule.NameTable.Width - 4) * _spritesModule.Specs.TileWidth;
            if (WorldSprite.X > maxX)
                WorldSprite.X = maxX;

            WorldSprite.Y = 80;
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

        private void FireBullet()
        {
            var bullet = _bulletControllers.TryAddNew(3);
            if (bullet != null)
            {
                bullet.WorldSprite.TileIndex = SpriteTileIndex.Extra1;
                bullet.WorldSprite.X = WorldSprite.X;
                bullet.WorldSprite.Y = WorldSprite.Y + 8;
                bullet.WorldSprite.FlipX = true;
                bullet.Motion.XSpeed = -40;
                _audioService.PlaySound(ChompAudioService.Sound.Fireball);
            }
        }

        private ISpriteController CreateFirebomb()
        {
            var bullet = _bulletControllers.TryAddNew(3);
            if (bullet != null)
            {
                bullet.WorldSprite.TileIndex = SpriteTileIndex.Extra2;
                bullet.WorldSprite.X = WorldSprite.X + 16;
                bullet.WorldSprite.Y = WorldSprite.Y + 8;
                _audioService.PlaySound(ChompAudioService.Sound.Fireball);

                bullet.WorldSprite.Y = 64;
                bullet.WorldSprite.X = _player.X;
                throw new System.NotImplementedException();
                //bullet.Motion.SetYSpeed(20);
                //bullet.Motion.TargetYSpeed = 40;
                //bullet.Motion.YAcceleration = 4;
                
            }

            return bullet;
        }

        private void UpdatePartPositions()
        {
            _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 66);

            var sprite = GetSprite();
            var jawSprite = _spritesModule.GetSprite(_jawSpriteIndex);
            jawSprite.X = (byte)(sprite.X - 7);
            jawSprite.Y = (byte)(sprite.Y + 8 + _jawPosition.Value);
        }

        protected override void UpdateBehavior()
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
                _levelBossBackgroundEnd.Value = (byte)(_spritesModule.Specs.ScreenHeight - (_spritesModule.Specs.TileHeight * 2));

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
                    if(_jawPosition.Value > 0 && _levelTimer.IsMod(16))
                    {
                        _jawPosition.Value--;
                    }

                    _motion.TargetXSpeed = -50;
                    _motion.XAcceleration = 8;

                    if(WorldSprite.X < 8)
                    {
                        _internalTimer.Value = 0;
                        _phase.Value = Phase.FireRain;
                    }
                }
            }
            else if(_phase.Value == Phase.FireRain)
            {
                _dynamicBlockController.PositionFreeCoinBlocksNearPlayer(
                    (byte)(_player.X / _spritesModule.Specs.TileWidth),
                    (byte)(_spritesModule.Specs.NameTableHeight - 6));

                if(_internalTimer.Value == 0)
                {
                    CollisionEnabled = false;
                    _audioService.PlaySound(ChompAudioService.Sound.Lightning);
                    _internalTimer.Value++;
                    _motion.SetXSpeed(0);
                    _motion.SetYSpeed(0);
                    HideBoss();

                    _levelBossBackgroundEnd.Value = (byte)(_spritesModule.Specs.ScreenHeight - (_spritesModule.Specs.TileHeight * 4));
                }


                if (_internalTimer.Value <= 7)
                    _paletteModule.BgColor = ColorIndex.LightGray(7 - _internalTimer.Value).Value;
                else if (_internalTimer <= 14)
                    _paletteModule.BgColor = ColorIndex.LightGray(14 - _internalTimer.Value).Value;
                else
                {
                    _paletteModule.BgColor = ColorIndex.Black;
                    PositionBossAbovePlayer();
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

                        if(_internalTimer.Value == 40)
                        {
                            _phase.Value = Phase.BossAppear;
                            _dynamicBlockController.ResetCoinsForLevelBoss();
                            _levelBossBackgroundEnd.Value = (byte)(_spritesModule.Specs.ScreenHeight - (_spritesModule.Specs.TileHeight * 2));
                            SetBossTiles();
                            _internalTimer.Value = 0;
                        }
                        else if(_internalTimer.Value >= 20)
                        {
                            CreateFirebomb();
                        }
                    }
                }
            }           
        }
   
        private void CreateExplosion()
        {
            var explosion = _bulletControllers.TryAddNew(3);
            if (explosion != null)
            {
                explosion.Explode();
                var rng = new RandomHelper(_levelTimer.Value);
                explosion.WorldSprite.X = WorldSprite.X + rng.RandomItem(-8, -4, 0, 4, 8);
                explosion.WorldSprite.Y = WorldSprite.Y + 4 + rng.RandomItem(-8, -4, 0, 4, 8);
            }
        }
    }
}
