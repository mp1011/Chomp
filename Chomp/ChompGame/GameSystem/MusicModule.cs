using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SceneModels;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace ChompGame.GameSystem
{
    class MusicModule : Module
    {
        public const bool Enabled = true;

        private readonly ContentManager _contentManager;

        public enum SongName : byte 
        {
            None,
            SeaDreams,
            Adventure,
            Adventure2,
            Threat
        }

        private GameByteEnum<SongName> _currentSong;

        private GameBit _newSong;

        public SongName CurrentSong
        {
            get => _currentSong.Value;
            set
            {
                if (_currentSong.Value != value)
                {
                    _newSong.Value = true;
                    _currentSong.Value = value;
                }
            }
        }

        public MusicModule(MainSystem mainSystem, ContentManager contentManager) 
            : base(mainSystem)
        {
            _contentManager = contentManager;
        }


        public void PlaySongForLevel(Level level)
        {
            if (level >= Level.Level1_1_Start && level <= Level.Level1_10_Stair)
                CurrentSong = SongName.Adventure;
            else if (level >= Level.Level1_12_Horizontal2)
                CurrentSong = SongName.Adventure2;
            else
                CurrentSong = SongName.None;
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            int address = memoryBuilder.CurrentAddress;
            _currentSong = new GameByteEnum<SongName>(new MaskedByte(address, Bit.Right7, memoryBuilder.Memory));
            _newSong = new GameBit(address, Bit.Bit7, memoryBuilder.Memory);

            memoryBuilder.AddByte();
        }

        public override void OnStartup()
        {
        }

        public void Update()
        {
            if (!Enabled)
                return;

            if (!_newSong.Value)
                return;
            
            _newSong.Value = false;
            if (_currentSong.Value == SongName.None)
            {
                MediaPlayer.Stop();
                return;
            }

            var song = _contentManager.Load<Song>(@"Music\" + _currentSong.Value.ToString());
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);
        }
    }
}
