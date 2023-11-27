using ChompGame.Audio;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;

namespace ChompGame.MainGame
{
    class ChompAudioService : IModule
    {
        public enum Sound : byte
        {
            Jump,
            CollectCoin,
            Break,    
            Lightning,
            PlayerHit,
            DoorOpen,
            DoorClose,
            Fireball,
            Reward,
            ButtonPress,
            PlayerDie,
            CrocodileBark,
            PlaneTakeoff,
            Max = PlaneTakeoff
        }

        private readonly BankAudioModule _audioModule;

        public ChompAudioService(BankAudioModule audioModule)
        {
            _audioModule = audioModule;
        }

        public void OnStartup()
        {
            byte index = 0;

            index = DefineSound(index,
                Sound.Jump,
                noteDuration: 2,
                soundData: "+ + A A# B C C# D");

            index = DefineSound(index,
                Sound.CollectCoin,
                noteDuration: 3,
                soundData: "+ + + A G C D");

            index = DefineSound(index,
               Sound.Break,
               noteDuration: 4,
               soundData: "* * * * * * * G F E D C B A");

            index = DefineSound(index,
             Sound.Lightning,
             noteDuration: 6,
             soundData: "+ * * G C A D C A");

            index = DefineSound(index,
              Sound.PlayerHit,
              noteDuration: 2,
              soundData: "+ + G C# B A# A");


            index = DefineSound(index,
                Sound.DoorOpen,
                noteDuration: 3,
                soundData: "+ A# B C C# D");

            index = DefineSound(index,
                Sound.DoorClose,
                noteDuration: 3,
                soundData: "+ D C# C B A");

            index = DefineSound(index,
              Sound.Fireball,
              noteDuration: 5,
              soundData: "+ + D# D C A");

            index = DefineSound(index,
                Sound.Reward,
                noteDuration: 5,
                soundData: "+ + A C E B D F C E G");

            index = DefineSound(index,
              Sound.ButtonPress,
              noteDuration: 12,
              soundData: "+ + A G");

            index = DefineSound(index,
                Sound.PlayerDie,
                noteDuration: 24,
                soundData: "+ G F# E D# D C#");

            index = DefineSound(index,
                Sound.CrocodileBark,
                noteDuration: 4,
                soundData: "+ G D C + A F# C B");

            index = DefineSound(index,
               Sound.PlaneTakeoff,
               noteDuration: 20,
               soundData: "* A A# B C C# D D# E F G");

            _audioModule.PrepareSounds();
        }
        
        private byte DefineSound(byte index,
            Sound sound,
            byte noteDuration,
            string soundData)
        {
            var dataTokens = soundData.Split(' ');

            _audioModule
             .GetSound((int)sound)
             .Set(index, noteDuration, (byte)dataTokens.Length);
           
            return _audioModule.NoteSequence.SetData(index, dataTokens);
        }


        public void Update()
        {
            _audioModule.OnLogicUpdate();
        }

        public void PlaySound(Sound sound)
        {
            GetChannel(sound).Play((int)sound);
        }

        private AudioChannel GetChannel(Sound sound)
        {
            return sound switch {
                Sound.Break => _audioModule.GetChannel(1),
                Sound.PlayerDie => _audioModule.GetChannel(1),
                Sound.Reward => _audioModule.GetChannel(1),
                Sound.CrocodileBark => _audioModule.GetChannel(1),
                _ => _audioModule.GetChannel(0)
            };
        }

        public void BuildMemory(SystemMemoryBuilder memoryBuilder) { }
    }
}
