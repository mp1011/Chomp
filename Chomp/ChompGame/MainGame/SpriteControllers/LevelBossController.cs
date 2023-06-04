using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class LevelBossController : EnemyController
    {
        private const byte BossLightningAppearValue = 100;
        private const byte FloatSpeed = 20;
        private const byte FloatTurnAngle = 16;

        private SpriteDefinition _jawSpriteDefinition;
        private CoreGraphicsModule _graphicsModule;
        private TileModule _tileModule;
        private PaletteModule _paletteModule;
        private GameByteGridPoint _position;
        private WorldSprite _player;
        private GameByte _jawSpriteIndex;
        private GameByte _lightningValue;
        private MusicModule _musicModule;

        private enum Phase : byte
        {
            Init,
            Lightning,
            BossAppear,
            Float,
        }

        private GameByteEnum<Phase> _phase;

        public LevelBossController(ChompGameModule gameModule,
            WorldSprite player,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.LevelBoss, gameModule, memoryBuilder)
        {
            _player = player;
            _musicModule = gameModule.MusicModule;
            _graphicsModule = gameModule.GameSystem.CoreGraphicsModule;
            _tileModule = gameModule.TileModule;
            _paletteModule = gameModule.PaletteModule;
            _position = gameModule.LevelBossPosition;
            _phase = new GameByteEnum<Phase>(memoryBuilder.AddByte());
            _jawSpriteIndex = memoryBuilder.AddByte();
            _jawSpriteDefinition = new SpriteDefinition(SpriteType.BossJaw, memoryBuilder.Memory);
            _lightningValue = memoryBuilder.AddByte();
        }

        protected override void BeforeInitializeSprite()
        {
            _position.X = 200;
            _position.Y = 16;

            WorldSprite.Y = 80;
            WorldSprite.X = 16;
            _hitPoints.Value = 4;
        }

        private void CreateBoss()
        {
            _paletteModule.BgColor = ColorIndex.Black;
            _tileModule.NameTable.SetFromString(0, 15,
                @"13335
                        9$BBD
                        @#BBA
                        00$D0");

            Motion.XAcceleration = 10;
            Motion.YAcceleration = 10;
            Motion.XSpeed = 0;
            Motion.YSpeed = 0;

            _jawSpriteIndex.Value = _spritesModule.GetFreeSpriteIndex();
            var jawSprite = _spritesModule.GetSprite(_jawSpriteIndex);
            jawSprite.Tile = _jawSpriteDefinition.Tile;
            jawSprite.SizeX = _jawSpriteDefinition.SizeX;
            jawSprite.SizeY = _jawSpriteDefinition.SizeY;
            jawSprite.Tile2Offset = 0;
            jawSprite.Visible = true;
            jawSprite.Palette = 2;

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
            Motion.SetXSpeed(20);
            _phase.Value = Phase.Float;

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

        protected override void UpdateBehavior()
        {
            if (_phase.Value >= Phase.BossAppear || _lightningValue.Value >= BossLightningAppearValue)
            {
                if (WorldSprite.Y < 64)
                    WorldSprite.Y = 64;
                
                _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
                _position.Y = (byte)(WorldSprite.Y - 66);

                var sprite = GetSprite();
                var jawSprite = _spritesModule.GetSprite(_jawSpriteIndex);
                jawSprite.X = (byte)(sprite.X - 7);
                jawSprite.Y = (byte)(sprite.Y + 8);
            }

            if (_phase.Value == Phase.Init)
            {
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
            else if(_phase.Value == Phase.Lightning)
            {
                byte strikeValue = (byte)(_lightningValue.Value % 96);

                if (strikeValue == 0)
                {
                    if(_lightningValue.Value >= BossLightningAppearValue)
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
                    _lightningValue.Value++;

                    if (_lightningValue.Value == BossLightningAppearValue)
                    {
                        CreateBoss();
                        PositionBossAbovePlayer();
                    }

                    if (_lightningValue.Value == 0)
                    {
                        _musicModule.CurrentSong = MusicModule.SongName.Nemesis;
                        _phase.Value = Phase.BossAppear;
                        _lightningValue.Value = 0;
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

                    _lightningValue.Value++;
                }

                if (_lightningValue.Value == 16)
                {
                    _phase.Value = Phase.Float;
                }

            }
            else if (_phase.Value == Phase.Float)
            {
                int targetX = _player.X + 16;

                int maxX = (_tileModule.NameTable.Width - 4) * _spritesModule.Specs.TileWidth;
                if (targetX > maxX)
                    targetX = maxX;

                if (_levelTimer.IsMod(8))
                    Motion.TurnTowards(WorldSprite, new Point(targetX, 80), FloatTurnAngle, FloatSpeed);

                _movingSpriteController.Update();               
            }
        }
    }
}
