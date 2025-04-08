using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Graphics;
using System.Xml;

namespace ChompGame.MainGame.SceneModels
{
    class GlitchCoreBgModule
    {
        private GameByte _timer;
        private WorldScroller _scroller;
        private RandomModule _rng;
        private ScenePartsDestroyed _scenePartsDestroyed;
        private MusicModule _musicModule;
        private ChompAudioService _audioService;
        private GameByte _levelDestructTimer;
        private PaletteModule _paletteModule;
        private bool _firstScene;

        public GlitchCoreBgModule(SystemMemoryBuilder memoryBuilder, ChompGameModule gameModule, bool firstScene)
        {
            _firstScene = firstScene;
            _rng = gameModule.RandomModule;
            _paletteModule = gameModule.PaletteModule;
            _timer = gameModule.LevelTimer;
            _scroller = gameModule.WorldScroller;
            _levelDestructTimer = memoryBuilder.AddByte();
            _musicModule = gameModule.MusicModule;
            _scenePartsDestroyed = gameModule.ScenePartsDestroyed;
            _audioService = gameModule.AudioService;

            if (!gameModule.ScenePartsDestroyed.SwitchBlocksOff)
                gameModule.DynamicBlocksController.ToggleSwitchBlocks();

            if(!_firstScene)
                _levelDestructTimer.Value = 3;

            if (gameModule.CurrentLevel < Level.Level7_17_Final1 
                || gameModule.CurrentLevel == Level.Level7_28_Final12_VerticalRetry
                || gameModule.CurrentLevel >= Level.Level7_33_Final17_Chamber)
                _levelDestructTimer.Value = 0;
        }

        public void Update()
        {
            if (_levelDestructTimer.Value == 255)
                return;

            if(_levelDestructTimer.Value == 0 && !_scenePartsDestroyed.SwitchBlocksOff)
            {
                _levelDestructTimer.Value = 1;
            }

            if (_levelDestructTimer.Value > 0)
            {
                _paletteModule.BgColor = ColorIndex.Black;
                 HandleLevelDestruct();
            }

            if (!_timer.Value.IsMod(32))
                return;

            _scroller.ModifyTiles((bg, atr) =>
            {
                bg.ForEach((x, y, b) =>
                {
                    if (b == 24 || b == 25)
                    {
                        if (_rng.Generate(1) == 0)
                            bg[x, y] = 24;
                        else
                            bg[x, y] = 25;
                    }
                });
            });
        }

        private void HandleLevelDestruct()
        {
            if(_levelDestructTimer.Value < 3)
            {
                if (_timer.IsMod(128))
                    _levelDestructTimer.Value++;
            }
            else
            {
                int x = (_levelDestructTimer.Value - 4) * 2;
                int y = _scroller.LevelNameTable.Height - 4;

                byte delay = 16;
                if (x >= _scroller.LevelNameTable.Width - 2)
                    delay = 28;


                if (_timer.Value.IsMod(delay))
                {
                    _levelDestructTimer.Value++;                   
                    if (x >= _scroller.LevelNameTable.Width-2)
                    {
                        y = _scroller.LevelNameTable.Height - (x - _scroller.LevelNameTable.Width);
                        y = y.Clamp(0, _scroller.LevelNameTable.Height - 1);

                        if (y >= 4)
                        {
                            for (x = 0; x < _scroller.LevelNameTable.Width; x += 2)
                            {
                                BlockWarn(x, y);
                                BlockDestroy(x, y + 2);
                            }
                        }
                    }
                    else
                    {
                        BlockWarn(x, y);
                        BlockWarn(x, y + 2);

                        if (_levelDestructTimer.Value > 3)
                        {
                            BlockDestroy(x - 2, y);
                            BlockDestroy(x - 2, y + 2);

                        }

                        if (_firstScene)
                        {
                            if (_levelDestructTimer.Value == 6)
                            {
                                BlockWarn(_scroller.LevelNameTable.Width - 4, y - 2);
                                BlockWarn(_scroller.LevelNameTable.Width - 4, y - 4);
                            }
                            else if (_levelDestructTimer.Value == 7)
                            {
                                BlockDestroy(_scroller.LevelNameTable.Width - 4, y - 2);
                                BlockDestroy(_scroller.LevelNameTable.Width - 4, y - 4);
                            }
                        }
                    }
                }
            }

            if (_levelDestructTimer.Value >= 2)
            {
                _scroller.OffsetCamera(_rng.Generate(1), _rng.Generate(1));

                if (_levelDestructTimer.Value == 2 && _timer.IsMod(8))
                    _audioService.PlaySound(ChompAudioService.Sound.Lightning);

            }

            if (_levelDestructTimer.Value == 3)
                _musicModule.CurrentSong = MusicModule.SongName.VeryDefinitelyFinalDungeon;
        }

        private void BlockWarn(int x, int y)
        {
            x /= 2;
            y /= 2;

            x = x.Clamp(0, _scroller.LevelAttributeTable.Width - 1);
            y = y.Clamp(0, _scroller.LevelAttributeTable.Height - 1);

            _scroller.ModifyTiles((t, a) =>
            {
                a[x, y] = 2;
            });
        }

        private void BlockDestroy(int x, int y)
        {
            x = x.Clamp(0, _scroller.LevelNameTable.Width - 2);
            y = y.Clamp(0, _scroller.LevelNameTable.Height - 2);

            _scroller.ModifyTiles((t, a) =>
            {
                t[x, y] = 0;
                t[x+1, y] = 0;
                t[x, y+1] = 0;
                t[x+1, y+1] = 0;

            });
        }
    }
}
