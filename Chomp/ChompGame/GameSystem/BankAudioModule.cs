using ChompGame.Audio;
using ChompGame.Data;
using ChompGame.Data.Memory;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;
using SoundBank = ChompGame.Audio.SoundBank;

namespace ChompGame.GameSystem
{
    class BankAudioModule : Module, ILogicUpdateModule
    {
        private int _soundHeaderAddress;
        
        private AudioChannel[] _audioChannels;

        public SoundBank SoundBank { get; private set; }
        public NoteSequence NoteSequence { get; private set; }
        
        public BankAudioModule(MainSystem mainSystem) : base(mainSystem)
        {
            //List<SoundEffect> tones = new List<SoundEffect>();

            //MusicNote[] notes = new MusicNote[]
            //{
            //    MusicNote.A,
            //    MusicNote.ASharp,
            //    MusicNote.B,
            //    MusicNote.C,
            //    MusicNote.CSharp,
            //    MusicNote.D,
            //    MusicNote.DSharp,
            //    MusicNote.E,
            //    MusicNote.F,
            //    MusicNote.FSharp,
            //    MusicNote.G,
            //    MusicNote.GSharp,
            //};

            //for (int octave = 0; octave < 4; octave++)
            //{
            //    foreach (var note in notes)
            //    {
            //        tones.Add(GenerateSine(
            //        frequency: note.GetFrequency(octave),
            //        volume: 128,
            //        seconds: 5));
            //    }
            //}

            //ToneBank = new ToneBank(tones);
        }

        public AudioChannel GetChannel(byte index)
        {
            if (index < _audioChannels.Length)
                return _audioChannels[index];
            else
                return new AudioChannel(_audioChannels[0].Address + (index * AudioChannel.Bytes), GameSystem.Memory, this);
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _soundHeaderAddress = memoryBuilder.CurrentAddress;
            for (int i = 0; i < Specs.NumSounds; i++)
            {
                new SoundHeader(memoryBuilder);
            }

            NoteSequence = new NoteSequence(memoryBuilder, Specs);
            _audioChannels = Enumerable.Range(0, Specs.AudioChannels)
                .Select(i => new AudioChannel(memoryBuilder, this))
                .ToArray();
        }


        public SoundHeader GetSound(int index)
        {
            return new SoundHeader(_soundHeaderAddress + (index * SoundHeader.Length), GameSystem.Memory);
        }

        public void PrepareSounds()
        {
            List<SoundEffect> sounds = new List<SoundEffect>();
            int i = 0;
            while(i < Specs.NumSounds)
            {
                SoundHeader sound = GetSound(i);
                if (sound.NoteDuration == 0)
                    break;

                sounds.Add(SoundGenerator.Generate(sound, NoteSequence));
                i++;
            }

            SoundBank = new SoundBank(sounds);
        }

         

        public void OnLogicUpdate()
        {
            foreach (var channel in _audioChannels)
            {
                channel.Update();
            }
        }

        public override void OnStartup()
        {
           
        }
    }
}
