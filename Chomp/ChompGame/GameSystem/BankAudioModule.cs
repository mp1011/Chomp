using ChompGame.Audio;
using ChompGame.Data;
using ChompGame.Helpers;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.GameSystem
{
    class BankAudioModule : BaseAudioModule
    {
        private AudioChannel[] _audioChannels;

        public ToneBank ToneBank { get; private set; }
        public NoteSequence NoteSequence { get; private set; }

        public BankAudioModule(MainSystem mainSystem) : base(mainSystem)
        {
            List<SoundEffect> tones = new List<SoundEffect>();

            MusicNote[] notes = new MusicNote[]
            {
                MusicNote.A,
                MusicNote.ASharp,
                MusicNote.B,
                MusicNote.C,
                MusicNote.CSharp,
                MusicNote.D,
                MusicNote.DSharp,
                MusicNote.E,
                MusicNote.F,
                MusicNote.FSharp,
                MusicNote.G,
                MusicNote.GSharp,
            };

            for (int octave = 0; octave < 4; octave++)
            {
                foreach (var note in notes)
                {
                    tones.Add(GenerateSine(
                    frequency: note.GetFrequency(octave),
                    volume: 128,
                    seconds: 1));
                }
            }

            ToneBank = new ToneBank(tones);
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
            NoteSequence = new NoteSequence(memoryBuilder, Specs);
            _audioChannels = Enumerable.Range(0, Specs.AudioChannels)
                .Select(i => new AudioChannel(memoryBuilder, this))
                .ToArray();
        }

        public override void OnLogicUpdate()
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
