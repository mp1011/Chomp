﻿using ChompGame.Data.Memory;
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
            PlayerHit,
            DoorOpen,
            DoorClose,
            Fireball,
            Reward,
            ButtonPress,
            PlayerDie,
            Max=ButtonPress
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
            _audioModule.GetChannel(0).Play((int)sound);
        }

        public void BuildMemory(SystemMemoryBuilder memoryBuilder) { }
    }
}
