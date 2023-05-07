using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class ChompBoss1Controller : EnemyController
    {
        private readonly MovingWorldSprite _player;
        private readonly ChompAudioService _audioService;
        private readonly MusicModule _music;
        private readonly WorldScroller _scroller;
        private readonly Specs _specs;

        private const int MaxY = 32;

        enum Phase : byte 
        {
            Init=0,
            BeforeTrap=1,
            TrapStart=2,
            TrapEnd=12,
            Appear=15,
            BeforeAttack=16,
        }

        private GameByteEnum<Phase> _phase;

        public ChompBoss1Controller(MovingWorldSprite player, ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.Chomp, gameModule, memoryBuilder)
        {
            _player = player;
            _audioService = gameModule.AudioService;
            _music = gameModule.MusicModule;
            _phase = new GameByteEnum<Phase>(_state);
            _scroller = gameModule.WorldScroller;
            _specs = gameModule.Specs;
        }

        protected override void UpdateBehavior()
        {
            _movingSpriteController.Update();


            if (_phase.Value == Phase.Init)
            {
                WorldSprite.X = ((_scroller.LevelNameTable.Width / 2)-1) * _specs.TileWidth;
                WorldSprite.Y = 0;

                _music.CurrentSong = MusicModule.SongName.None;
                _phase.Value = Phase.BeforeTrap;
            }
            else if (_phase.Value == Phase.BeforeTrap && _player.X > 20)
            {
                _phase.Value = Phase.TrapStart;
            }
            else if(_phase.Value >= Phase.TrapStart && _phase.Value < Phase.TrapEnd)
            {
                if((_levelTimer.Value % 8) == 0)
                {
                    _audioService.PlaySound(ChompAudioService.Sound.Break);
                    int y = 13 - _state.Value;

                    _scroller.ModifyTiles((tilemap, attr) =>
                    {
                        tilemap[0, y] = Constants.DestructibleBlockTile;
                        tilemap[1, y] = Constants.DestructibleBlockTile;

                        tilemap[tilemap.Width - 1, y] = Constants.DestructibleBlockTile;
                        tilemap[tilemap.Width - 2, y] = Constants.DestructibleBlockTile;

                        attr[0, y / 2] = 1;
                        attr[(tilemap.Width/2)-1, y / 2] = 1;

                    });

                    _state.Value++;
                }
            }
            else if (_phase.Value >= Phase.TrapEnd && _phase.Value < Phase.Appear)
            {
                if ((_levelTimer.Value % 8) == 0)
                    _state.Value++;
            }
            else if (_phase.Value == Phase.Appear)
            {
                Motion.SetYSpeed(5);
                
                if(WorldSprite.Y >= MaxY)
                {
                    Motion.SetYSpeed(0);
                    _phase.Value = Phase.BeforeAttack;
                }
            }
        }
    }
}
