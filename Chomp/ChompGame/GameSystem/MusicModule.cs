﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SceneModels;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.Collections;

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
            Threat,
            Nemesis,
            Flight,
            City,
            Railway,
            Dusk,
            Stronghold,
            Moonstruck,
            Space,
            CommandCenter,
            Infiltration,
            SystemMalfunction,
            VeryDefinitelyFinalDungeon,
            FinalBossPart1,
            FinalBossPart1End,
            FinalBossPart2,
            FinalBossPart2End,
            FinalBossPart3
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

        public bool IsPlaying => MediaPlayer.State == MediaState.Playing;

        public int PlayPosition => (int)MediaPlayer.PlayPosition.TotalMilliseconds;

        public MusicModule(MainSystem mainSystem, ContentManager contentManager) 
            : base(mainSystem)
        {
            _contentManager = contentManager;
        }


        public void PlaySongForLevel(Level level)
        {
            if (level >= Level.Level1_1_Start && level <= Level.Level1_10_Stair)
                CurrentSong = SongName.Adventure;
            else if (level >= Level.Level1_12_Horizontal2 && level < Level.Level2_1_Intro)
                CurrentSong = SongName.Adventure2;
            else if (level >= Level.Level2_1_Intro && level < Level.Level2_3_Beach)
                CurrentSong = SongName.Flight;
            else if (level >= Level.Level2_3_Beach && level < Level.Level2_12_Boss)
                CurrentSong = SongName.SeaDreams;
            else if (level >= Level.Level3_1_City && level < Level.Level3_20_Midboss)
                CurrentSong = SongName.City;
            else if (level >= Level.Level3_21_CityAfterMidboss && level < Level.Level4_1_Desert)
                CurrentSong = SongName.Railway;
            else if (level >= Level.Level4_1_Desert && level < Level.Level4_31_Midboss)
                CurrentSong = SongName.Dusk;
            else if (level >= Level.Level4_32_Desert4 && level < Level.Level4_40_Boss)
                CurrentSong = SongName.Stronghold;
            else if (level >= Level.Level5_1_Mist && level < Level.Level5_22_MidBoss)
                CurrentSong = SongName.Moonstruck;
            else if (level >= Level.Level5_23_Plane_Begin && level < Level.Level5_27_Boss)
                CurrentSong = SongName.Space;
            else if (level >= Level.Level6_1_Techbase && level < Level.Level6_17_Midboss)
                CurrentSong = SongName.CommandCenter;
            else if (level >= Level.Level6_18_Techbase11 && level < Level.Level6_37_Boss)
                CurrentSong = SongName.Infiltration;
            else if (level >= Level.Level7_1_GlitchCore && level < Level.Level7_13_PreFinal1)
                CurrentSong = SongName.SystemMalfunction;
            else if (level >= Level.Level7_17_Final1 && level < Level.Level7_37_BeforeBoss1)
                CurrentSong = SongName.VeryDefinitelyFinalDungeon;

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

        public bool CurrentSongRepeats()
        {
            return _currentSong.Value switch
            {
                SongName.FinalBossPart1End => false,
                _ => true
            };
        }

        public void Pause()
        {
            MediaPlayer.Pause();
        }

        public void Resume()
        {
            MediaPlayer.Resume();
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

            try
            {
                var song = _contentManager.Load<Song>(@"Music\" + _currentSong.Value.ToString());
                MediaPlayer.IsRepeating = CurrentSongRepeats();
                MediaPlayer.Play(song);
            }
            catch
            {
                MediaPlayer.Stop();
            }
        }
    }
}
