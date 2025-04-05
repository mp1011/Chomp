using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
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

        public GlitchCoreBgModule(SystemMemoryBuilder memoryBuilder, ChompGameModule gameModule)
        {
            _rng = gameModule.RandomModule;
            _timer = gameModule.LevelTimer;
            _scroller = gameModule.WorldScroller;
            _levelDestructTimer = memoryBuilder.AddByte();
            _musicModule = gameModule.MusicModule;
            _scenePartsDestroyed = gameModule.ScenePartsDestroyed;
            _audioService = gameModule.AudioService;
        
            if (!gameModule.ScenePartsDestroyed.SwitchBlocksOff)
                gameModule.DynamicBlocksController.ToggleSwitchBlocks();
        }

        public void Update()
        {
            if(_levelDestructTimer.Value == 0 && !_scenePartsDestroyed.SwitchBlocksOff)
            {
                _levelDestructTimer.Value = 1;
            }

            if (_levelDestructTimer.Value > 0)
                HandleLevelDestruct();

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
                if (_timer.IsMod(16))
                {
                    _levelDestructTimer.Value++;
                    int x = (_levelDestructTimer.Value - 2) * 2;
                    int y = _scroller.LevelNameTable.Height - 4;
                    BlockWarn(x, y);
                    if (_levelDestructTimer.Value > 3)
                        BlockDestroy(x - 2, y);

                    if(_levelDestructTimer.Value == 6)
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
            _scroller.ModifyTiles((t, a) =>
            {
                a[x / 2, y / 2] = 2;
            });
        }

        private void BlockDestroy(int x, int y)
        {
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
