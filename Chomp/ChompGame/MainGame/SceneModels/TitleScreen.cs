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

        const int Let_E = 3;
        const int Let_S = 4;
        const int Let_O = 6;
        const int Let_R = 7;
        const int Let_L = 8;
        const int Let_D = 9;
        const int Let_T2 = 10;

        const int Let_T = 12;
        const int Let_A = 13;

        const int Solid = 25;
        const int M_Mid = 26;
        const int Left = 27;
        const int Right = 28;
        const int P_Top = 29;
        const int P_Bottom = 30;

        const int StartY = 24;
        const int LoadY = 32;
        const int DelY = 40;


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
            Title = 26,
            StartGame = 27,
            Load = 28,
            StartLoadedGame = 29
        }

        public TitleScreen(ChompGameModule gameModule, GameByte state, SystemMemoryBuilder builder)
        {
            _gameModule = gameModule;
            _state = new GameByteEnum<State>(state);
        }

        public void Update()
        {
            GemPalette();

            if (_state.Value >= State.StarFadein && _state.Value < State.Title)
            {
                if(_gameModule.InputModule.Player1.StartKey == GameKeyState.Pressed)
                {
                    SetTitleTiles();
                    SetTitleSprites();
                    _gameModule.MusicModule.CurrentSong = MusicModule.SongName.None;
                    _gameModule.GameSystem.CoreGraphicsModule.FadeAmount = 0;
                    _gameModule.TileModule.Scroll.Y = 0;
                    _state.Value = State.Title;
                    return;
                }
            }

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
                    if (_state.Value == State.ChompsEscape)
                    {
                        DestroyGems();
                    }
                }
            }
            else if (_state.Value == State.ChompsEscape)
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
                            SetTitleSprites();
                        }
                    }
                }
            }
            else if (_state.Value == State.TitleFadeIn)
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
            else if (_state.Value == State.Title)
            {
                var menuSprite = _gameModule.SpritesModule.GetSprite(0);
                if (_gameModule.InputModule.Player1.DownKey == GameKeyState.Pressed)
                {
                    if (menuSprite.Y < DelY)
                    {
                        menuSprite.Y += 8;
                        _gameModule.AudioService.PlaySound(ChompAudioService.Sound.CollectCoin);
                    }
                }
                else if (_gameModule.InputModule.Player1.UpKey == GameKeyState.Pressed)
                {
                    if (menuSprite.Y > StartY)
                    {
                        menuSprite.Y -= 8;
                        _gameModule.AudioService.PlaySound(ChompAudioService.Sound.CollectCoin);
                    }
                }
                else if (_gameModule.InputModule.Player1.StartKey == GameKeyState.Pressed)
                {
                    // TODO
                    // on Load, show numbers for each valid save file
                    // left/right to select (color via attribute table)
                    // hit start again to load

                    // on start, pick first unused save slot 

                    // delete handled same way as load

                    // if no free save slots, "start" option can't be selected

                    if (menuSprite.Y == StartY)
                    {
                        _gameModule.AudioService.PlaySound(ChompAudioService.Sound.ButtonPress);
                        _state.Value = State.StartGame;
                    }
                    else if (menuSprite.Y == LoadY)
                    {
                        if (SetTilesForLoad())
                        {
                            _gameModule.AudioService.PlaySound(ChompAudioService.Sound.ButtonPress);
                            _state.Value = State.Load;
                        }
                        else
                        {
                            _gameModule.AudioService.PlaySound(ChompAudioService.Sound.PlayerHit);
                            SetTitleTiles();
                        }
                    }
                    else if (menuSprite.Y == DelY)
                    {
                        _gameModule.AudioService.PlaySound(ChompAudioService.Sound.ButtonPress);
                    }
                }
            }
            else if (_state.Value == State.Load)
            {
                var menuSprite = _gameModule.SpritesModule.GetSprite(0);

                
                if (_gameModule.InputModule.Player1.DownKey == GameKeyState.Pressed)
                {
                    MoveSaveCursor(1);
                }
                else if (_gameModule.InputModule.Player1.UpKey == GameKeyState.Pressed)
                {
                    MoveSaveCursor(-1);
                }
                else if (_gameModule.InputModule.Player1.StartKey == GameKeyState.Pressed)
                {
                    _state.Value = State.StartLoadedGame;
                }
            }
            else if (_state.Value == State.StartGame || _state.Value == State.StartLoadedGame)
            {
                var menuSprite = _gameModule.SpritesModule.GetSprite(0);
                var chompSprite = _gameModule.SpritesModule.GetSprite(1);

                if (!chompSprite.Visible)
                {
                    _gameModule.MusicModule.CurrentSong = MusicModule.SongName.None;
                    chompSprite.Visible = true;
                    chompSprite.X = (byte)(_gameModule.Specs.NameTablePixelWidth - 7);
                    chompSprite.Y = (byte)(menuSprite.Y - 2);
                }

                if (_gameModule.LevelTimer.IsMod(2))
                {
                    if (menuSprite.Visible)
                    {
                        chompSprite.X++;
                        if (chompSprite.X == menuSprite.X)
                        {
                            menuSprite.Visible = false;
                        }
                    }
                    else if (chompSprite.X != (byte)(_gameModule.Specs.NameTablePixelWidth - 8))
                    {
                        chompSprite.X--;
                    }
                    else
                    {
                        _gameModule.GameSystem.CoreGraphicsModule.FadeAmount++;
                        if (_gameModule.GameSystem.CoreGraphicsModule.FadeAmount == 15)
                        {
                            if (_state.Value == State.StartGame)
                                _gameModule.StartGame();
                            else
                                _gameModule.LoadGame(CursorSaveSlot);
                        }
                    }
                }
            }

        }

        private void MoveSaveCursor(int direction)
        {
            var current = CursorSaveSlot;

            var next = current + direction;            
            while(!IsSlotAvailable(next))
            {
                if (next < 0 || next > 3)
                    return;

                next += direction;
            }

            CursorSaveSlot = next;
        }

        private void SetTitleSprites()
        {
            for (int i = 0; i < _gameModule.Specs.MaxSprites; i++)
            {
                var sprite = _gameModule.SpritesModule.GetSprite(i);
                sprite.Tile = 0;
            }

            int gemIndex = CreateGemSprite();
            int chompIndex = CreateChompSprite();

            var gem = _gameModule.SpritesModule.GetSprite(gemIndex);
            var chomp = _gameModule.SpritesModule.GetSprite(chompIndex);

            chomp.Visible = false;

            gem.Visible = true;
            gem.X = 12;
            gem.Y = StartY;
        }

        private void SetTitleTiles()
        {
            _gameModule.TileModule.NameTable.ForEach((x, y, b) => _gameModule.TileModule.NameTable[x, y] = 0);

            int x = 3;

            // C
            _gameModule.TileModule.NameTable[x, 2] = Solid;
            _gameModule.TileModule.NameTable[x, 3] = Left;
            _gameModule.TileModule.NameTable[x, 4] = Solid;

            // H
            _gameModule.TileModule.NameTable[x + 1, 2] = Right;
            _gameModule.TileModule.NameTable[x + 1, 3] = Right;
            _gameModule.TileModule.NameTable[x + 1, 4] = Right;
            _gameModule.TileModule.NameTable[x + 2, 2] = Right;
            _gameModule.TileModule.NameTable[x + 2, 3] = Solid;
            _gameModule.TileModule.NameTable[x + 2, 4] = Right;

            // O
            _gameModule.TileModule.NameTable[x + 3, 2] = Right;
            _gameModule.TileModule.NameTable[x + 3, 3] = Right;
            _gameModule.TileModule.NameTable[x + 3, 4] = Right;
            _gameModule.TileModule.NameTable[x + 4, 2] = Solid;
            _gameModule.TileModule.NameTable[x + 4, 3] = Right;
            _gameModule.TileModule.NameTable[x + 4, 4] = Solid;


            // M
            _gameModule.TileModule.NameTable[x + 5, 2] = Right;
            _gameModule.TileModule.NameTable[x + 5, 3] = Right;
            _gameModule.TileModule.NameTable[x + 5, 4] = Right;
            _gameModule.TileModule.NameTable[x + 7, 2] = Left;
            _gameModule.TileModule.NameTable[x + 7, 3] = Left;
            _gameModule.TileModule.NameTable[x + 7, 4] = Left;
            _gameModule.TileModule.NameTable[x + 6, 3] = M_Mid;

            // P
            _gameModule.TileModule.NameTable[x + 8, 2] = P_Top;
            _gameModule.TileModule.NameTable[x + 8, 3] = P_Bottom;
            _gameModule.TileModule.NameTable[x + 8, 4] = Left;
            _gameModule.TileModule.NameTable[x + 9, 2] = Left;
            _gameModule.TileModule.NameTable[x + 9, 3] = Left;

            x = 4;
            int y = 6;

            // START
            _gameModule.TileModule.NameTable[x, y] = Let_S;
            _gameModule.TileModule.NameTable[x + 1, y] = Let_T;
            _gameModule.TileModule.NameTable[x + 2, y] = Let_A;
            _gameModule.TileModule.NameTable[x + 3, y] = Let_R;
            _gameModule.TileModule.NameTable[x + 4, y] = Let_T2;

            // LOAD
            _gameModule.TileModule.NameTable[x, y + 2] = Let_L;
            _gameModule.TileModule.NameTable[x + 1, y + 2] = Let_O;
            _gameModule.TileModule.NameTable[x + 2, y + 2] = Let_A;
            _gameModule.TileModule.NameTable[x + 3, y + 2] = Let_D;

            // DEL
            _gameModule.TileModule.NameTable[x, y + 4] = Let_D;
            _gameModule.TileModule.NameTable[x + 1, y + 4] = Let_E;
            _gameModule.TileModule.NameTable[x + 2, y + 4] = Let_L;

        }

        private bool IsSlotAvailable(int slot)
        {
            if (slot < 0 || slot > 3)
                return false;

            return _gameModule.TileModule.NameTable[4, 8 + (2*slot)] != 0;
        }

        private bool SetTilesForLoad()
        {
            _gameModule.TileModule.NameTable.ForEach((x, y, b) =>
            {
                if (y >= 6)
                    _gameModule.TileModule.NameTable[x, y] = 0;
            });

            int x = 4;
            int y = 6;

            // LOAD
            _gameModule.TileModule.NameTable[x, y] = Let_L;
            _gameModule.TileModule.NameTable[x + 1, y] = Let_O;
            _gameModule.TileModule.NameTable[x + 2, y] = Let_A;
            _gameModule.TileModule.NameTable[x + 3, y] = Let_D;
            
            bool anyValid = false;
            for (int i =0; i < 4; i++)
            {
                y += 2;
                if(IsSaveValid(i))
                {
                    anyValid = true;
                    _gameModule.TileModule.NameTable[x, y] = (byte)(16 + i);
                }
            }

            return anyValid;
        }

        private int CursorSaveSlot
        {
            get
            {
                var menuSprite = _gameModule.SpritesModule.GetSprite(0);
                int tileY = menuSprite.Y / _gameModule.Specs.TileHeight;

                return (tileY - 8) / 2;
            }
            set
            {
                var menuSprite = _gameModule.SpritesModule.GetSprite(0);
                menuSprite.Y = (byte)((8 + (value * 2)) * _gameModule.Specs.TileHeight);
            }
        }

        private bool IsSaveValid(int slot) => _gameModule.SaveManager.IsSaveSlotValid(slot);
        

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

        private int CreateGemSprite()
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
            return spriteIndex;
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

        public void OnHBlank()
        {
            if (_state.Value != State.Title)
                return;

            var screenY = _gameModule.GameSystem.CoreGraphicsModule.ScreenPoint.Y;
            var cursor = _gameModule.SpritesModule.GetSprite(0).Y;
            if (screenY >= cursor)
            {
                if(screenY < cursor + 4)
                {
                    int y = screenY - cursor;
                    int c = 3 + (y + _gameModule.LevelTimer.Value / 4) % 4;
                    var palette = _gameModule.GameSystem.CoreGraphicsModule.GetBackgroundPalette(0);
                    palette.SetColor(2, ColorIndex.Red(c).Value);
                }
                else
                {
                    var palette = _gameModule.GameSystem.CoreGraphicsModule.GetBackgroundPalette(0);
                    palette.SetColor(2, ColorIndex.Gray2);
                }
            }
        }
    }
}
