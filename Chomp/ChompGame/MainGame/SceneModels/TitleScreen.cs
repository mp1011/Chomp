using ChompGame.Data;
using ChompGame.GameSystem;

namespace ChompGame.MainGame.SceneModels
{
    internal class TitleScreen
    {
        private readonly ChompGameModule _gameModule;
        private GameByte _state;

        public TitleScreen(ChompGameModule gameModule, GameByte state, NBitPlane masterPatternTable)
        {
           _gameModule = gameModule;
        }

        public void Update()
        {
            _gameModule.MusicModule.CurrentSong = MusicModule.SongName.Story;
        }
    }
}
