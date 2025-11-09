using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels
{
    internal class TitleScreen
    {
        private readonly ChompGameModule _gameModule;
        
        private GameByteEnum<State> _state;

        private MainSystem GameSystem => _gameModule.GameSystem;
        enum State : byte
        {
            Init = 0,
            StarFadein = 1,
            Gems = 2,
            Chomps = 3,
            ChompsEat = 4,
            ChompsEscape = 24,
            TitleFadeIn = 25,
            Title = 26
        }

        public TitleScreen(ChompGameModule gameModule, GameByte state, SystemMemoryBuilder builder)
        {
            _gameModule = gameModule;
            _state = new GameByteEnum<State>(state);
        }

        public void Update()
        {
            _gameModule.TileCopier.AnalyzeTileUsage();
            if (_state.Value == State.Init)
            {
                _gameModule.MusicModule.CurrentSong = MusicModule.SongName.Story;
                LoadTiles();
                SetPalette();

                _state.Value = State.StarFadein;
            }
            else if (_state.Value == State.StarFadein)
            {
                if (_gameModule.LevelTimer.Value.IsMod(4))
                    _gameModule.TileModule.Scroll.Y--;

                if (_gameModule.LevelTimer.Value.IsMod(32))
                {
                    _gameModule.GameSystem.CoreGraphicsModule.FadeAmount--;
                    if (_gameModule.GameSystem.CoreGraphicsModule.FadeAmount == 0)
                    {
                        var pp = _gameModule.MusicModule.PlayPosition;
                        _state.Value = State.Gems;
                    }
                }
            }
            else if (_state.Value == State.Gems)
            {
                if (_gameModule.LevelTimer.Value.IsMod(8))
                    _gameModule.TileModule.Scroll.Y--;

                var songPos = _gameModule.MusicModule.PlayPosition - 9000;
                if (songPos >= 0)
                {
                    if (songPos >= GemCount() * 1000)
                        CreateGemSprite();

                    if (GemCount() == 8)
                        _state.Value = State.Chomps;
                }

                PlaceGems();
            }
            else if (_state.Value == State.Chomps)
            {
                PlaceGems();

                var chompIndex = ChompSpriteIndex();
                if (chompIndex == -1)
                    chompIndex = CreateChompSprite();


                var chomp = _gameModule.SpritesModule.GetSprite(chompIndex);
                chomp.Visible = true;

                if (_gameModule.LevelTimer.IsMod(16))
                {
                    if (chomp.Tile == 2)
                        chomp.Tile = 4;
                    else
                        chomp.Tile = 2;
                }

                if (_gameModule.LevelTimer.IsMod(8))
                {
                    chomp.Y--;
                }

                int targetY = _gameModule.Specs.ScreenHeight / 2;
                if (chomp.Y < targetY)
                    _state.Value = State.ChompsEat;
            }
            else if (_state.Value >= State.ChompsEat && _state.Value < State.ChompsEscape)
            {
                PlaceGems(24 - (_state.Value - State.ChompsEat));

                var chompIndex = ChompSpriteIndex();
                var chomp = _gameModule.SpritesModule.GetSprite(chompIndex);

                if (_gameModule.LevelTimer.IsMod(16))
                {
                    if (chomp.Tile == 2)
                        chomp.Tile = 4;
                    else
                        chomp.Tile = 2;
                }

                _gameModule.TileModule.Scroll.X = _gameModule.RandomModule.Generate(2);
                _gameModule.TileModule.Scroll.Y = _gameModule.RandomModule.Generate(2);


                if (_gameModule.LevelTimer.IsMod(8))
                {
                    _state.Value++;
                    if(_state.Value == State.ChompsEscape)
                    {
                        DestroyGems();
                    }
                }
            }
            else if(_state.Value == State.ChompsEscape)
            {
                _gameModule.TileModule.Scroll.X = 0;
                _gameModule.TileModule.Scroll.Y = 0;

                var chompIndex = ChompSpriteIndex();
                var chomp = _gameModule.SpritesModule.GetSprite(chompIndex);

                if (_gameModule.LevelTimer.IsMod(16))
                {
                    if (chomp.Tile == 2)
                        chomp.Tile = 4;
                    else
                        chomp.Tile = 2;
                }

                if (_gameModule.LevelTimer.IsMod(4))
                {
                    chomp.Y--;

                    if (chomp.Y < 16)
                    {
                        _gameModule.GameSystem.CoreGraphicsModule.FadeAmount++;
                        if (_gameModule.GameSystem.CoreGraphicsModule.FadeAmount == 15)
                        {
                            _state.Value = State.TitleFadeIn;
                            SetTitleTiles();
                        }
                    }
                }
            }
            else if(_state.Value == State.TitleFadeIn)
            {
                if (_gameModule.LevelTimer.Value.IsMod(16))
                {
                    _gameModule.GameSystem.CoreGraphicsModule.FadeAmount--;
                    if (_gameModule.GameSystem.CoreGraphicsModule.FadeAmount == 0)
                    {
                        _state.Value = State.Title;
                    }
                }
            }
            else if(_state.Value == State.Title)
            {
                // add tiles for menu cursor, title graphic
                // erase sprites
                // set tiles to spell title
                // sprite for menu options
                // tiles for options

            }

            GemPalette();
        }

        private void SetTitleTiles()
        {
            _gameModule.TileModule.NameTable.ForEach((x, y, b) => _gameModule.TileModule.NameTable[x, y] = 0);

            // C
            _gameModule.TileModule.NameTable[1, 2] = 25;
            _gameModule.TileModule.NameTable[2, 2] = 25;
            _gameModule.TileModule.NameTable[3, 2] = 25;
            _gameModule.TileModule.NameTable[1, 3] = 25;
            _gameModule.TileModule.NameTable[1, 4] = 25;
            _gameModule.TileModule.NameTable[2, 4] = 25;
            _gameModule.TileModule.NameTable[3, 4] = 25;


        }

        private void DestroyGems()
        {
            for (int i = 0; i < _gameModule.Specs.MaxSprites; i++)
            {
                var sprite = _gameModule.SpritesModule.GetSprite(i);
                if (sprite.Tile == 1)
                    sprite.Tile = 0;
            }
        }

        private int GemCount()
        {
            int count = 0;
            for (int i = 0; i < _gameModule.Specs.MaxSprites; i++)
            {
                var sprite = _gameModule.SpritesModule.GetSprite(i);
                if (sprite.Tile == 1)
                    count++;
            }
            return count;
        }

        private void PlaceGems(int radius = 24)
        {
            for (int i = 0; i < _gameModule.Specs.MaxSprites; i++)
            {
                var sprite = _gameModule.SpritesModule.GetSprite(i);
                if (sprite.Tile != 1)
                    continue;

                var center = new Point(_gameModule.Specs.ScreenWidth / 2, _gameModule.Specs.ScreenHeight / 2);

                byte b = _gameModule.LevelTimer.Value;              
                var angle = 360 * (b / 255.0);

                angle = (angle + (45 * i)) % 360;

                Point offset = new Point(0, radius).RotateDeg((int)angle);
                sprite.X = (byte)(center.X + offset.X);
                sprite.Y = (byte)(center.Y + offset.Y);
            }

        }

        private void CreateGemSprite()
        {
            var spriteIndex = _gameModule.SpritesModule.GetFreeSpriteIndex();
            var sprite = _gameModule.SpritesModule.GetSprite(spriteIndex);

            sprite.Tile = 1;
            sprite.SizeX = 1;
            sprite.SizeY = 1;
            sprite.X = 16;
            sprite.Y = 16;
            sprite.Visible = true;
            sprite.Palette = SpritePalette.Player;
        }

        private int CreateChompSprite()
        {
            var spriteIndex = _gameModule.SpritesModule.GetFreeSpriteIndex();
            var sprite = _gameModule.SpritesModule.GetSprite(spriteIndex);

            sprite.Tile = 2;
            sprite.SizeX = 2;
            sprite.SizeY = 2;

            sprite.X = (byte)((_gameModule.Specs.ScreenWidth / 2) - 4);
            sprite.Y = (byte)_gameModule.Specs.ScreenHeight;

            sprite.Tile2Offset = 1;
            sprite.Visible = true;
            sprite.Palette = SpritePalette.Fire;
            return spriteIndex;
        }

        private int ChompSpriteIndex()
        {
            for(int i = 0; i < _gameModule.Specs.MaxSprites; i++)
            {
                var s = _gameModule.SpritesModule.GetSprite(i);
                if (s.Visible && (s.Tile == 2 || s.Tile == 4))
                    return i;
            }

            return -1;
        }

        private void SetPalette()
        {
            var palette = GameSystem.CoreGraphicsModule.GetBackgroundPalette(0);
            palette.SetColor(0, ColorIndex.Black);
            palette.SetColor(1, ColorIndex.Yellow1);
            palette.SetColor(2, ColorIndex.Gray1);
            palette.SetColor(3, ColorIndex.White);

            palette = GameSystem.CoreGraphicsModule.GetSpritePalette(0);
            palette.SetColor(0, ColorIndex.Black);
            palette.SetColor(1, ColorIndex.Green1);
            palette.SetColor(2, ColorIndex.Green3);
            palette.SetColor(3, ColorIndex.White);

            _gameModule.GameSystem.CoreGraphicsModule.FadeAmount = 15;
        }

        private void GemPalette()
        {
            var t = _gameModule.LevelTimer.Value % 32;

            var palette = GameSystem.CoreGraphicsModule.GetSpritePalette(SpritePalette.Player);

            if (t < 16)
            {
                palette.SetColor(1, ColorIndex.Blue2);
                palette.SetColor(3, ColorIndex.White);
            }
            else
            {
                palette.SetColor(1, ColorIndex.Blue1);
                palette.SetColor(3, ColorIndex.Blue8);
            }
        }

        private void LoadTiles()
        {
            _gameModule.GameSystem.CoreGraphicsModule.BackgroundPatternTable.Reset();
            _gameModule.TileModule.NameTable.Reset();
            _gameModule.TileModule.AttributeTable.Reset();

            _gameModule.RasterInterrupts.SetScene(null);
            _gameModule.PaletteModule.SetScene(null, Level.Level1_1_Start, _gameModule.GameSystem.Memory, _gameModule.BossBackgroundHandling);

            _gameModule.TileCopier.CopyTilesForTitle();

            _gameModule.TileModule.Scroll.X = 0;
            _gameModule.TileModule.Scroll.Y = 0;

            _gameModule.TileModule.NameTable.ForEach((x, y, b) =>
            {
                if(_gameModule.RandomModule.Generate(8) <= 32)
                    _gameModule.TileModule.NameTable[x, y] = 24;

                _gameModule.RandomModule.Generate(3);
            });
        }
    }
}
