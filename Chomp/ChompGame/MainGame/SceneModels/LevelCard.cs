using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ChompGame.MainGame.SceneModels
{
    internal class LevelCard
    {
        private GameByteEnum<Phase> _state;
        private GameByte _timer;
        private ChompGameModule _gameModule;
        private NBitPlane _masterPatternTable;

        private MainSystem GameSystem => _gameModule.GameSystem;
        private CoreGraphicsModule CoreGraphicsModule => GameSystem.CoreGraphicsModule;

        private enum Phase : byte
        {
            Load,
            FadeIn,
            Display,
            FadeOut,
            Skip
        }
        

        public LevelCard(ChompGameModule gameModule, GameByte state, NBitPlane masterPatternTable)
        {
            _state = new GameByteEnum<Phase>(state);
            _gameModule = gameModule;
            _masterPatternTable = masterPatternTable;
            _timer = gameModule.LevelTimer;
        }

        public void Reset()
        {
            _timer.Value = 0;
            _state.Value = Phase.Load;
            _gameModule.TileModule.Scroll.X = 0;
            _gameModule.TileModule.Scroll.Y = 0;
        }

        public bool Update()
        {
            GameDebug.Watch1 = new DebugWatch("Timer", () => _timer.Value);
            GameDebug.Watch2 = new DebugWatch("State", () => (int)_state.Value);


            if (_gameModule.InputModule.Player1.StartKey.IsDown() ||
                _gameModule.InputModule.Player1.AKey.IsDown() ||
                _gameModule.InputModule.Player1.BKey.IsDown())
            {
                if (_state.Value < Phase.FadeOut)
                {
                    _state.Value = Phase.Skip;
                    _timer.Value = 0;
                }
            }


            switch (_state.Value)
            {
                case Phase.Load:
                    _gameModule.MusicModule.CurrentSong = MusicModule.SongName.None;
                    SetTiles(CurrentLevel());
                    ResetSprites();
                    SetPalette();
                    CoreGraphicsModule.FadeAmount = 0;
                    _state.Value = Phase.FadeIn;
                    return false;
                case Phase.FadeIn:
                    if (_timer.Value >= 64)
                    {
                        _state.Value = Phase.Display;
                        _timer.Value = 0;
                    }
                    return false;
                case Phase.Display:
                    if(_gameModule.InputModule.Player1.StartKey.IsDown() ||
                        _gameModule.InputModule.Player1.AKey.IsDown() ||
                        _gameModule.InputModule.Player1.BKey.IsDown() ||
                        _timer.Value >= 64)
                    {
                        _state.Value = Phase.FadeOut;
                        _timer.Value = 0;
                    }
                        
                    return false;
                case Phase.FadeOut:
                    if (_timer.Value.IsMod(16))
                        CoreGraphicsModule.FadeAmount++;

                    if (_timer.Value >= 64)
                        return true;

                    return false;
                case Phase.Skip:
                    CoreGraphicsModule.FadeAmount = 0;
                    _gameModule.TileModule.Scroll.X = 0;
                    _gameModule.TileModule.Scroll.Y = 0;
                    _timer.Value = 0;
                    _state.Value = Phase.FadeOut;
                    return false;

            }

            return false;
        }

        private void ResetSprites()
        {
            for(int i = 0; i < _gameModule.Specs.MaxSprites; i++)
            {
                var sprite = _gameModule.SpritesModule.GetSprite(i);
                sprite.Visible = false;
                sprite.Tile = 0;
            }
        }

        private void SetPalette()
        {
            var palette = GameSystem.CoreGraphicsModule.GetBackgroundPalette(BgPalette.Background);
            palette.SetColor(0, ColorIndex.Black);
            palette.SetColor(1, ColorIndex.Red2);
            palette.SetColor(2, ColorIndex.Red2);
            palette.SetColor(3, ColorIndex.White);
        }

        private int CurrentLevel()
        {
            if (_gameModule.CurrentLevel < Level.Level2_1_Intro)
                return 1;
            else if (_gameModule.CurrentLevel < Level.Level3_1_City)
                return 2;
            else if (_gameModule.CurrentLevel < Level.Level4_1_Desert)
                return 3;
            else if (_gameModule.CurrentLevel < Level.Level5_1_Mist)
                return 4;
            else if (_gameModule.CurrentLevel < Level.Level6_1_Techbase)
                return 5;
            else if (_gameModule.CurrentLevel < Level.Level7_1_GlitchCore)
                return 6;
            else
                return 7;
        }

        public void OnHBlank()
        {
            switch(_state.Value)
            {
                case Phase.FadeIn:

                    double mod = 1.0 - (_timer.Value / 64.0);
                    if (_timer.Value > 64)
                        mod = 0;

                    int effectY = CoreGraphicsModule.ScreenPoint.Y - 32; 
                    //var p = CoreGraphicsModule.GetBackgroundPalette(SceneModels.BgPalette.Background);

                    if (effectY >= 0 && effectY <= 32)
                    {
                      //  p.SetColor(0, ColorIndex.Yellow1);
                         _gameModule.TileModule.Scroll.Y = (byte)(255 - effectY * mod);
                    }
                    else
                    {
                        _gameModule.TileModule.Scroll.Y = 0;
                      //  p.SetColor(0, ColorIndex.Black);
                    }
                    _gameModule.TileModule.Scroll.X = 0;
                    break;
                case Phase.Display:
                    _gameModule.TileModule.Scroll.X = 0;
                    _gameModule.TileModule.Scroll.Y = 0;
                    break;
                case Phase.FadeOut:
                    mod = (_timer.Value / 64.0);
                    if (_timer.Value >= 64)
                        mod = 1.0;
                   
                    effectY = CoreGraphicsModule.ScreenPoint.Y - 32;
                    if (effectY >= 0 && effectY <= 32)
                    {
                        _gameModule.TileModule.Scroll.X = (byte)(255 - (5 - effectY) * mod * 8.0);
                    }
                    else
                    {
                        _gameModule.TileModule.Scroll.X = 0;
                    }
                    break;
            }
        }

        private void SetTiles(int level)
        {
            _gameModule.GameSystem.CoreGraphicsModule.BackgroundPatternTable.Reset();
            _gameModule.TileModule.NameTable.Reset();
            _gameModule.TileModule.AttributeTable.Reset();

            _gameModule.RasterInterrupts.SetScene(null);
            _gameModule.PaletteModule.SetScene(null, Level.Level1_1_Start, _gameModule.GameSystem.Memory, _gameModule.BossBackgroundHandling);
         
            var palette = GameSystem.CoreGraphicsModule.GetBackgroundPalette(0);
            palette.SetColor(0, ColorIndex.Black);
            palette.SetColor(2, ColorIndex.Red1);

            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(7, 3, 1, 1),
                destinationPoint: new Point(1, 0),
                _gameModule.Specs,
                GameSystem.Memory);

            _masterPatternTable.CopyTilesTo(
               destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
               source: new InMemoryByteRectangle(8, 5, 4, 1),
               destinationPoint: new Point(2, 0),
               _gameModule.Specs,
               GameSystem.Memory);

            _masterPatternTable.CopyTilesTo(
              destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
              source: new InMemoryByteRectangle(6, 4, 8, 1),
              destinationPoint: new Point(0, 1),
              _gameModule.Specs,
              GameSystem.Memory);

            _gameModule.TileModule.Scroll.X = 0;
            _gameModule.TileModule.Scroll.Y = 0;

            int wordX = 4;
            int wordY = 8;

            _gameModule.TileModule.NameTable[wordX++, wordY] = 1;
            _gameModule.TileModule.NameTable[wordX++, wordY] = 2;
            _gameModule.TileModule.NameTable[wordX++, wordY] = 3;
            _gameModule.TileModule.NameTable[wordX++, wordY] = 4;
            _gameModule.TileModule.NameTable[wordX++, wordY] = 5;



            wordX++;
            _gameModule.TileModule.NameTable[wordX++, wordY] = (byte)(7 + level);

        }
    }
}
