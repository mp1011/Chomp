using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using System;

namespace ChompGame.MainGame.SceneModels
{
    class Ending
    {
        private const int Letter_Left = 2;
        private const int Letter_Right = 3;
        private const int Letter_UL = 4;
        private const int Letter_BL = 5;
        private const int Letter_NMid = 6;
        private const int Letter_D1 = 8;
        private const int Letter_D2 = 9;

        private bool _startPart2 = false;
        private bool _startPart3 = false;

        private const int StarIndexBegin = 0;
        private const int StarCount = 8;
        private const int GemIndexBegin = StarIndexBegin + StarCount;
        private const int GemCount = 8;

        private ChompGameModule _gameModule;
        private GameByte _state;
        private GameByte _state2;

        private CoreGraphicsModule CoreGraphicsModule => _gameModule.GameSystem.CoreGraphicsModule;

        public Ending(ChompGameModule gameModule, GameByte state, SystemMemoryBuilder builder)
        {
            _gameModule = gameModule;
            _state = state;

            _state2 = new GameByte(builder.Memory.GetAddress(AddressLabels.FreeRAM), builder.Memory);
        }

        private byte Part2RealScroll
        {
            get => _state2.Value;
            set => _state2.Value = value;
        }

        public bool Update()
        {
            _gameModule.InputModule.OnLogicUpdate();
            if(_gameModule.InputModule.Player1.StartKey == GameKeyState.Pressed)
            {
                return true;
            }

            GemPalette();
            if (_state.Value == 0)
            {
                _gameModule.TileModule.Scroll.X = 0;
                _gameModule.TileModule.Scroll.Y = 0;
                _gameModule.SpritesModule.Scroll.X = 0;
                _gameModule.SpritesModule.Scroll.Y = 0;

                LoadVRAM();
                SetPalette();
                SetStarBg();
                SetSprites();
                _state.Value++;
                _state2.Value = 0;

                if (_startPart2)
                    _state.Value = 40;
                else if (_startPart3)
                    _state.Value = 121;

                _gameModule.MusicModule.CurrentSong = GameSystem.MusicModule.SongName.Ending;
            }
            else
            {
                if (_gameModule.LevelTimer.IsMod(32) && CoreGraphicsModule.FadeAmount > 0)
                    CoreGraphicsModule.FadeAmount--;

                if (_state.Value < 41 && _gameModule.LevelTimer.IsMod(8))
                    _gameModule.TileModule.Scroll.Y--;

                if (_state.Value < 41)
                    ScrollStars();

                if (CoreGraphicsModule.FadeAmount == 0 && _state.Value == 1)
                {
                    _state.Value++;
                }

                if (_state.Value == 2)
                {
                    if (_state2.Value == 0)
                    {
                        if (MoveGems())
                        {
                            _state2.Value = 1;
                        }
                    }
                    else if (_gameModule.LevelTimer.IsMod(16))
                    {
                        _state2.Value++;
                        if (_state2.Value == 4)
                            _state.Value++;
                    }
                }
                else if (_state.Value > 2 && _state.Value < 9)
                {
                    if (_gameModule.LevelTimer.IsMod(16))
                        _state.Value++;
                }
                else if (_state.Value >= 9 && _state.Value < 40)
                {
                    if (_gameModule.LevelTimer.IsMod(2))
                        _state.Value++;
                }
                else if (_state.Value == 40)
                {
                    if (_gameModule.LevelTimer.IsMod(8) && CoreGraphicsModule.FadeAmount < 15)
                        CoreGraphicsModule.FadeAmount++;

                    if (CoreGraphicsModule.FadeAmount == 15)
                    {
                        _state.Value++;
                        SetSpritesPart2();
                        LoadVRAMPart2();
                        SetTilesForPart2();
                        SetPalettePart2();
                    }
                }
                else if (_state.Value == 41)
                {
                    if (_gameModule.LevelTimer.IsMod(16) && CoreGraphicsModule.FadeAmount > 0)
                        CoreGraphicsModule.FadeAmount--;

                    if (_gameModule.LevelTimer.IsMod(4))
                        Part2RealScroll++;

                    UpdateMonsters();

                    if (CoreGraphicsModule.FadeAmount == 0)
                        _state.Value++;
                }
                else if (_state.Value == 42)
                {
                    if (_gameModule.LevelTimer.IsMod(4))
                        Part2RealScroll++;

                    UpdateMonsters();

                    if (_gameModule.SpritesModule.GetSprite(2).X < 32)
                        _state.Value++;
                }
                else if (_state.Value > 42 && _state.Value < 100)
                {
                    if (_gameModule.LevelTimer.IsMod(4))
                        Part2RealScroll++;

                    UpdateMonsters();
                    if (_gameModule.LevelTimer.IsMod(4))
                        _state.Value++;

                    if (_state.Value >= 85)
                    {
                        BlinkSprite(0);
                        BlinkSprite(1);
                        BlinkSprite(2);
                        BlinkSprite(3);
                    }

                    if (_state.Value == 100)
                    {
                        _gameModule.SpritesModule.GetSprite(0).Tile = 0;
                        _gameModule.SpritesModule.GetSprite(1).Tile = 0;
                        _gameModule.SpritesModule.GetSprite(2).Tile = 0;
                        _gameModule.SpritesModule.GetSprite(3).Tile = 0;
                    }
                }
                else if (_state.Value >= 100 && _state.Value < 120)
                {
                    if (_gameModule.LevelTimer.IsMod(4))
                        Part2RealScroll++;

                    if (_gameModule.LevelTimer.IsMod(4))
                        _state.Value++;
                }
                else if (_state.Value == 120)
                {
                    if (_gameModule.LevelTimer.IsMod(4))
                        Part2RealScroll++;

                    if (_gameModule.LevelTimer.IsMod(8) && CoreGraphicsModule.FadeAmount < 15)
                        CoreGraphicsModule.FadeAmount++;

                    if (CoreGraphicsModule.FadeAmount == 15)
                        _state.Value = 121;
                }
                else if (_state.Value == 121)
                {
                    LoadVRAM();
                    SetTilesForPart3();
                    SetSpritesPart3();
                    SetPalette();
                    _gameModule.TileModule.Scroll.X = 0;
                    _gameModule.TileModule.Scroll.Y = 80;
                    _state.Value++;

                    CoreGraphicsModule.FadeAmount = 0;
                }
                else if (_state.Value >= 122)
                {
                    ScrollStars();

                    if (_gameModule.LevelTimer.IsMod(8))
                    {
                        if (_gameModule.TileModule.Scroll.Y < 128)
                            _gameModule.TileModule.Scroll.Y++;
                    }

                    if (_gameModule.LevelTimer.IsMod(64))
                    {
                        _state.Value++;
                        if (_state.Value == 0)
                        {
                            return true;
                        }
                    }
                }

            }

            return false;
        }

        private void BlinkSprite(int index)
        {
            var s = _gameModule.SpritesModule.GetSprite(index);
            s.Visible = !s.Visible;
        }

        private void UpdateMonsters()
        {
            UpdateLizard(0);
            UpdateLizard(1);
            UpdateOgre(2);
            UpdateOgre(3);

        }

        private void UpdateLizard(int spriteIndex)
        {
            var sprite = _gameModule.SpritesModule.GetSprite(spriteIndex);

            if (_gameModule.LevelTimer.IsMod(16))
                sprite.Tile = (byte)(sprite.Tile == 1 ? 2 : 1);

            sprite.FlipX = false;

            if (_gameModule.LevelTimer.IsMod(32))
                sprite.X--;
        }

        private void UpdateOgre(int spriteIndex)
        {
            var sprite = _gameModule.SpritesModule.GetSprite(spriteIndex);

            if (_gameModule.LevelTimer.IsMod(16))
                sprite.Tile = (byte)(sprite.Tile == 3 ? 4 : 3);

            sprite.FlipX = false;

            if (_gameModule.LevelTimer.IsMod(16))
                sprite.X--;
        }

        private void ScrollStars()
        {
            for (int i = StarIndexBegin; i < StarCount; i++)
            {
                if (_gameModule.LevelTimer.Value.IsMod((byte)(i + 1)))
                {
                    var star = _gameModule.SpritesModule.GetSprite(i);
                    star.Y++;

                    if (star.Y > _gameModule.GameSystem.Specs.ScreenHeight)
                    {
                        star.Y = 0;
                        star.X = _gameModule.RandomModule.GenerateByte().Mod(_gameModule.GameSystem.Specs.ScreenWidth);
                    }
                }
            }
        }

        private bool MoveGems()
        {
            bool allPlaced = true;

            for (int i = GemIndexBegin; i < GemIndexBegin + GemCount; i++)
            {
                int relIndex = i - GemIndexBegin;
                byte moveMod = relIndex switch {
                    0 => 7,
                    1 => 6,
                    2 => 5,
                    3 => 4,
                    4 => 4,
                    5 => 5,
                    6 => 6,
                    _ => 7,
                };

                var gem = _gameModule.SpritesModule.GetSprite(i);

                if (_gameModule.LevelTimer.Value.IsMod((byte)(moveMod * 1.5)))
                    gem.Y--;

                if (gem.Y <= 31)
                    gem.Y = 31;
                else
                    allPlaced = false;
            }

            return allPlaced;
        }

        public void OnHBlank()
        {
            if (_state.Value >= 121)
                return;

            if (_state.Value < 3)
                return;

            if (_state.Value >= 41)
            {
                int mtnY = 32;
                if (CoreGraphicsModule.ScreenPoint.Y == mtnY)
                    _gameModule.TileModule.Scroll.X = (byte)(Part2RealScroll / 4);
                else if (CoreGraphicsModule.ScreenPoint.Y == mtnY + 8)
                    _gameModule.TileModule.Scroll.X = Part2RealScroll;


                if (_state.Value > 50)
                {
                    int beamPos = _state.Value - 50;

                    if (CoreGraphicsModule.ScreenPoint.Y == 0)
                        SetPalettePart2();

                    if (CoreGraphicsModule.ScreenPoint.Y >= beamPos - 4 && CoreGraphicsModule.ScreenPoint.Y < beamPos)
                    {
                        var palette = CoreGraphicsModule.GetBackgroundPalette(BgPalette.Background);
                        _gameModule.PaletteModule.Lighten(palette, 0);
                        _gameModule.PaletteModule.Lighten(palette, 1);
                        _gameModule.PaletteModule.Lighten(palette, 2);
                        _gameModule.PaletteModule.Lighten(palette, 3);

                        palette = CoreGraphicsModule.GetBackgroundPalette(BgPalette.Foreground);
                        _gameModule.PaletteModule.Lighten(palette, 0);
                        _gameModule.PaletteModule.Lighten(palette, 1);
                        _gameModule.PaletteModule.Lighten(palette, 2);
                        _gameModule.PaletteModule.Lighten(palette, 3);

                    }
                    else if (CoreGraphicsModule.ScreenPoint.Y == beamPos)
                    {
                        var palette = CoreGraphicsModule.GetBackgroundPalette(BgPalette.Background);
                        palette.SetColor(0, ColorIndex.White);
                        palette.SetColor(1, ColorIndex.White);
                        palette.SetColor(2, ColorIndex.White);
                        palette.SetColor(3, ColorIndex.White);

                        palette = CoreGraphicsModule.GetBackgroundPalette(BgPalette.Foreground);
                        palette.SetColor(0, ColorIndex.White);
                        palette.SetColor(1, ColorIndex.White);
                        palette.SetColor(2, ColorIndex.White);
                        palette.SetColor(3, ColorIndex.White);

                        palette = CoreGraphicsModule.GetSpritePalette(SpritePalette.Enemy1);
                        palette.SetColor(1, ColorIndex.Red3);
                        palette.SetColor(2, ColorIndex.Red3);
                        palette.SetColor(3, ColorIndex.Red3);
                    }
                    else if (CoreGraphicsModule.ScreenPoint.Y > beamPos && CoreGraphicsModule.ScreenPoint.Y < beamPos + 4)
                    {
                        SetPalettePart2();
                        int lightenAmount = 4 - (CoreGraphicsModule.ScreenPoint.Y - beamPos);

                        var palette = CoreGraphicsModule.GetBackgroundPalette(BgPalette.Background);
                        _gameModule.PaletteModule.Lighten(palette, 0, lightenAmount);
                        _gameModule.PaletteModule.Lighten(palette, 1, lightenAmount);
                        _gameModule.PaletteModule.Lighten(palette, 2, lightenAmount);
                        _gameModule.PaletteModule.Lighten(palette, 3, lightenAmount);

                        palette = CoreGraphicsModule.GetBackgroundPalette(BgPalette.Foreground);
                        _gameModule.PaletteModule.Lighten(palette, 0, lightenAmount);
                        _gameModule.PaletteModule.Lighten(palette, 1, lightenAmount);
                        _gameModule.PaletteModule.Lighten(palette, 2, lightenAmount);
                        _gameModule.PaletteModule.Lighten(palette, 3, lightenAmount);

                        palette = CoreGraphicsModule.GetSpritePalette(SpritePalette.Enemy1);
                        palette.SetColor(1, ColorIndex.Red3);
                        palette.SetColor(2, ColorIndex.Red3);
                        palette.SetColor(3, ColorIndex.Red3);
                    }
                    else if (CoreGraphicsModule.ScreenPoint.Y == beamPos + 4)
                    {
                        SetPalettePart2();
                    }
                }

                return;
            }

            int beamSize = _state.Value - 3;

            if (CoreGraphicsModule.ScreenPoint.Y >= 32 - beamSize &&
                CoreGraphicsModule.ScreenPoint.Y < 32 + beamSize)
            {
                int yDiff = Math.Abs(CoreGraphicsModule.ScreenPoint.Y - 32);

                int shade = beamSize - yDiff;
                if (shade > 7)
                    shade = 7;

                var palette = CoreGraphicsModule.GetBackgroundPalette(0);
                palette.SetColor(0, ColorIndex.Blue(shade).Value);
            }

            if (CoreGraphicsModule.ScreenPoint.Y == 32 + beamSize)
            {
                var palette = CoreGraphicsModule.GetBackgroundPalette(0);
                palette.SetColor(0, ColorIndex.Black);
            }


        }


        private void LoadVRAM()
        {
            _gameModule.GameSystem.CoreGraphicsModule.BackgroundPatternTable.Reset();
            _gameModule.TileModule.NameTable.Reset();
            _gameModule.TileModule.AttributeTable.Reset();

            _gameModule.RasterInterrupts.SetScene(null);
            _gameModule.PaletteModule.SetScene(null, Level.Level1_1_Start, _gameModule.GameSystem.Memory, _gameModule.BossBackgroundHandling);

            _gameModule.TileCopier.CopyTilesForEnding();
        }

        private void LoadVRAMPart2()
        {
            _gameModule.GameSystem.CoreGraphicsModule.BackgroundPatternTable.Reset();
            _gameModule.TileModule.NameTable.Reset();
            _gameModule.TileModule.AttributeTable.Reset();

            _gameModule.RasterInterrupts.SetScene(null);
            _gameModule.PaletteModule.SetScene(null, Level.Level1_1_Start, _gameModule.GameSystem.Memory, _gameModule.BossBackgroundHandling);

            _gameModule.TileCopier.CopyTilesForPlainsTheme();

            _gameModule.TileCopier.CopyTilesForSprite(2, 0, 2, 2, 1, 0, CoreGraphicsModule.SpritePatternTable);
            _gameModule.TileCopier.CopyTilesForSprite(12, 0, 2, 2, 3, 0, CoreGraphicsModule.SpritePatternTable);

        }

        private void GemPalette()
        {
            var t = _gameModule.LevelTimer.Value % 32;

            var palette = CoreGraphicsModule.GetSpritePalette(SpritePalette.Player);

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

        private void SetPalette()
        {
            var palette = CoreGraphicsModule.GetBackgroundPalette(0);
            palette.SetColor(0, ColorIndex.Black);
            palette.SetColor(1, ColorIndex.Yellow1);
            palette.SetColor(2, ColorIndex.Gray1);
            palette.SetColor(3, ColorIndex.White);

            palette = CoreGraphicsModule.GetSpritePalette(SpritePalette.Enemy1);
            palette.SetColor(0, ColorIndex.Black);
            palette.SetColor(1, ColorIndex.Green1);
            palette.SetColor(2, ColorIndex.Green3);
            palette.SetColor(3, ColorIndex.White);

            _gameModule.GameSystem.CoreGraphicsModule.FadeAmount = 15;
        }

        private void SetPalettePart2()
        {
            var palette = CoreGraphicsModule.GetBackgroundPalette(0);
            palette.SetColor(0, ColorIndex.Blue4);
            palette.SetColor(1, ColorIndex.Red1);
            palette.SetColor(2, ColorIndex.BlueGray3);
            palette.SetColor(3, ColorIndex.BlueGray4);

            palette = CoreGraphicsModule.GetBackgroundPalette(BgPalette.Foreground);
            palette.SetColor(0, ColorIndex.Blue4);
            palette.SetColor(1, ColorIndex.Green1);
            palette.SetColor(2, ColorIndex.Green2);
            palette.SetColor(3, ColorIndex.Green3);

            palette = CoreGraphicsModule.GetSpritePalette(SpritePalette.Enemy1);
            palette.SetColor(0, ColorIndex.Black);
            palette.SetColor(1, ColorIndex.BlueGray2);
            palette.SetColor(2, ColorIndex.BlueGray3);
            palette.SetColor(3, ColorIndex.Red3);
        }

        private void SetSprites()
        {
            for (int i = StarIndexBegin; i < StarCount; i++)
            {
                var starSprite = _gameModule.SpritesModule.GetSprite(i);
                starSprite.SizeX = 1;
                starSprite.SizeY = 1;
                starSprite.Tile2Offset = 0;
                starSprite.Tile = 2;
                starSprite.Visible = true;
                starSprite.Priority = true;
                starSprite.Palette = SpritePalette.Enemy1;

                starSprite.X = _gameModule.RandomModule.GenerateByte().Mod(_gameModule.GameSystem.Specs.ScreenWidth);
                starSprite.Y = _gameModule.RandomModule.GenerateByte().Mod(_gameModule.GameSystem.Specs.ScreenHeight);
            }

            for (int i = GemIndexBegin; i < GemIndexBegin + GemCount; i++)
            {
                var gemSprite = _gameModule.SpritesModule.GetSprite(i);
                gemSprite.SizeX = 1;
                gemSprite.SizeY = 1;
                gemSprite.Tile2Offset = 0;
                gemSprite.Tile = 1;
                gemSprite.Visible = true;
                gemSprite.Priority = true;
                gemSprite.Palette = SpritePalette.Player;

                int relIndex = i - GemIndexBegin;
                int spacing = (_gameModule.GameSystem.Specs.ScreenWidth - 8) / GemCount;

                gemSprite.X = (byte)(4 + relIndex * spacing);
                gemSprite.Y = 64;
            }
        }

        private void SetSpritesPart2()
        {
            for (int i = 0; i < _gameModule.Specs.MaxSprites; i++)
            {
                var sprite = _gameModule.SpritesModule.GetSprite(i);
                sprite.Tile = 0;
            }

            AddLizard(0, 56 + 16);
            AddLizard(1, 56 + 32);
            AddOgre(2, 56 + 32);
            AddOgre(3, 56 + 56);

        }

        private void SetSpritesPart3()
        {
            for (int i = 0; i < _gameModule.Specs.MaxSprites; i++)
            {
                var sprite = _gameModule.SpritesModule.GetSprite(i);
                if(i < 8)
                {
                    sprite.SizeX = 1;
                    sprite.SizeY = 1;
                    sprite.Tile2Offset = 0;
                    sprite.Tile = 1;
                    sprite.Visible = true;
                    sprite.Priority = false;
                    sprite.Palette = SpritePalette.Player;

                    sprite.X = _gameModule.RandomModule.GenerateByte().Mod(_gameModule.GameSystem.Specs.ScreenWidth);
                    sprite.Y = _gameModule.RandomModule.GenerateByte().Mod(_gameModule.GameSystem.Specs.ScreenHeight);
                }
                else 
                    sprite.Tile = 0;
            }


        }

        private void AddLizard(int index, byte x)
        {
            var sprite = _gameModule.SpritesModule.GetSprite(index);
            sprite.Tile = 1;
            sprite.Tile2Offset = 1;
            sprite.SizeX = 1;
            sprite.SizeY = 2;
            sprite.Y = 40;
            sprite.X = x;
            sprite.FlipX = false;
            sprite.FlipY = false;
        }

        private void AddOgre(int index, byte x)
        {
            var sprite = _gameModule.SpritesModule.GetSprite(index);
            sprite.Tile = 3;
            sprite.Tile2Offset = 1;
            sprite.SizeX = 1;
            sprite.SizeY = 2;
            sprite.Y = 40;
            sprite.X = x;
            sprite.FlipX = false;
            sprite.FlipY = false;
        }

        private void SetStarBg()
        {
            _gameModule.TileModule.Scroll.X = 0;
            _gameModule.TileModule.Scroll.Y = 0;

            _gameModule.TileModule.NameTable.ForEach((x, y, b) =>
            {
                if (_gameModule.RandomModule.Generate(8) <= 16)
                    _gameModule.TileModule.NameTable[x, y] = 1;

                _gameModule.RandomModule.Generate(3);
            });
        }

        private void SetTilesForPart2()
        {
            _gameModule.TileModule.Scroll.X = 0;
            _gameModule.TileModule.Scroll.Y = 0;


            _gameModule.TileModule.AttributeTable.ForEach((x, y, b) =>
            {
                if (y < 6)
                    _gameModule.TileModule.AttributeTable[x, y] = 0;
                else
                    _gameModule.TileModule.AttributeTable[x, y] = 1;
            });

            int groundY = 12;

            _gameModule.TileModule.NameTable.ForEach((x, y, b) =>
            {
                if (y == groundY - 1 || y == groundY - 2)
                {
                    _gameModule.TileModule.NameTable[x, y] = 6;
                }
                else if (y == groundY)
                {
                    _gameModule.TileModule.NameTable[x, y] = (byte)(11 + (x.IsMod(2) ? 0 : 1));
                }
                else if (y > groundY)
                {
                    if (y.IsMod(2))
                        _gameModule.TileModule.NameTable[x, y] = (byte)(8 + (x.IsMod(2) ? 0 : 1));
                    else
                        _gameModule.TileModule.NameTable[x, y] = (byte)(8 + (x.IsMod(2) ? 1 : 0));

                }
                else
                    _gameModule.TileModule.NameTable[x, y] = 0;
            });

            int mtnY = groundY - 3;
            for (int i = 0; i < 8; i++)
            {
                byte tile = i switch {
                    0 => 1,
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    4 => 5,
                    5 => 1,
                    6 => 2,
                    _ => 4
                };

                _gameModule.TileModule.NameTable[i, mtnY] = tile;
                _gameModule.TileModule.NameTable[8 + i, mtnY] = tile;
                _gameModule.TileModule.NameTable[16 + i, mtnY] = tile;
                _gameModule.TileModule.NameTable[24 + i, mtnY] = tile;
            }
        }


        private void SetTilesForPart3()
        {
            int x = 4;
            int y = 6;
            // E
            _gameModule.TileModule.NameTable[x, y] = Letter_UL;
            _gameModule.TileModule.NameTable[x, y + 1] = Letter_UL;
            _gameModule.TileModule.NameTable[x, y + 2] = Letter_BL;

            //N
            _gameModule.TileModule.NameTable[x + 3, y] = Letter_Left;
            _gameModule.TileModule.NameTable[x + 3, y + 1] = Letter_NMid ;
            _gameModule.TileModule.NameTable[x + 3, y + 2] = Letter_Left;
            _gameModule.TileModule.NameTable[x + 4, y] = Letter_Left;
            _gameModule.TileModule.NameTable[x + 4, y + 1] = Letter_Left;
            _gameModule.TileModule.NameTable[x + 4, y + 2] = Letter_Left;

            //D
            _gameModule.TileModule.NameTable[x + 6, y] = Letter_UL;
            _gameModule.TileModule.NameTable[x + 6, y + 1] = Letter_Left;
            _gameModule.TileModule.NameTable[x + 6, y + 2] = Letter_BL;
            _gameModule.TileModule.NameTable[x + 7, y] = Letter_D1;
            _gameModule.TileModule.NameTable[x + 7, y + 1] = Letter_Left;
            _gameModule.TileModule.NameTable[x + 7, y + 2] = Letter_D2;
        }
    }
}
