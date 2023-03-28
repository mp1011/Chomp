using ChompGame.GameSystem;

namespace ChompGame.MainGame
{
    class ChompAudioService
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
            PlayerDie
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

            //_audioModule.NoteSequence[0] = Audio.AudioAction.OctaveUp;
            //_audioModule.NoteSequence[1] = Audio.AudioAction.OctaveUp;
            //_audioModule.NoteSequence[2] = Audio.AudioAction.PlayG;
            //_audioModule.NoteSequence[3] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[4] = Audio.AudioAction.PlayASharp;

            //_audioModule.NoteSequence[5] = Audio.AudioAction.PlayG;
            //_audioModule.NoteSequence[6] = Audio.AudioAction.PlayFSharp;
            //_audioModule.NoteSequence[7] = Audio.AudioAction.PlayF;
            //_audioModule.NoteSequence[8] = Audio.AudioAction.PlayE;
            //_audioModule.NoteSequence[9] = Audio.AudioAction.PlayDSharp;
            //_audioModule.NoteSequence[10] = Audio.AudioAction.PlayD;

            //_audioModule.NoteSequence[11] = Audio.AudioAction.OctaveUp;
            //_audioModule.NoteSequence[12] = Audio.AudioAction.OctaveUp;
            //_audioModule.NoteSequence[13] = Audio.AudioAction.PlayASharp;
            //_audioModule.NoteSequence[14] = Audio.AudioAction.PlayB;
            //_audioModule.NoteSequence[15] = Audio.AudioAction.PlayC;
            //_audioModule.NoteSequence[16] = Audio.AudioAction.PlayCSharp;
            //_audioModule.NoteSequence[17] = Audio.AudioAction.PlayD;

            //_audioModule.NoteSequence[18] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[19] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[20] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[21] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[22] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[22] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[23] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[24] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[25] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[26] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[27] = Audio.AudioAction.PlayD;

            //_audioModule.NoteSequence[28] = Audio.AudioAction.AddNoise;
            //_audioModule.NoteSequence[29] = Audio.AudioAction.OctaveUp;
            //_audioModule.NoteSequence[30] = Audio.AudioAction.PlayGSharp;
            //_audioModule.NoteSequence[31] = Audio.AudioAction.PlayG;
            //_audioModule.NoteSequence[32] = Audio.AudioAction.PlayFSharp;
            //_audioModule.NoteSequence[33] = Audio.AudioAction.PlayG;
            //_audioModule.NoteSequence[34] = Audio.AudioAction.PlayFSharp;
            //_audioModule.NoteSequence[36] = Audio.AudioAction.PlayF;

            //_audioModule.NoteSequence[36] = Audio.AudioAction.OctaveUp;
            //_audioModule.NoteSequence[37] = Audio.AudioAction.PlayD;
            //_audioModule.NoteSequence[38] = Audio.AudioAction.PlayCSharp;
            //_audioModule.NoteSequence[39] = Audio.AudioAction.PlayC;            
            //_audioModule.NoteSequence[40] = Audio.AudioAction.PlayB;
            //_audioModule.NoteSequence[41] = Audio.AudioAction.PlayASharp;

            //64 byte limit


            //_audioModule
            //    .GetSound(0)
            //    .Set(5, 32, 6);

            //_audioModule
            //    .GetSound(1)
            //    .Set(0, 4, 5);


            //_audioModule
            //   .GetSound(3)
            //   .Set(18, 64, 1);

            ////noise
            //_audioModule
            //    .GetSound(4)
            //    .Set(28, 4, 8);

            ////door open
            //_audioModule
            //  .GetSound(5)
            //  .Set(12, 3, 6);

            ////door close
            //_audioModule
            //  .GetSound(6)
            //  .Set(36, 3, 6);

            //_audioModule
            //    .GetSound(7)
            //    .Set(0, 0, 0);

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
    }
}
