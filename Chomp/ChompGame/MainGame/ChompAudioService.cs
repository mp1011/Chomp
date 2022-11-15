using ChompGame.GameSystem;

namespace ChompGame.MainGame
{
    class ChompAudioService
    {
        public enum Sound : byte
        {
            PlayerDie=1,
            PlayerHit=2,
            Jump=3,
            Test=4,
            Noise=5
        }

        private readonly BankAudioModule _audioModule;

        public ChompAudioService(BankAudioModule audioModule)
        {
            _audioModule = audioModule;
        }

        public void OnStartup()
        {
            _audioModule.NoteSequence[0] = Audio.AudioAction.OctaveUp;
            _audioModule.NoteSequence[1] = Audio.AudioAction.OctaveUp;
            _audioModule.NoteSequence[2] = Audio.AudioAction.PlayG;
            _audioModule.NoteSequence[3] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[4] = Audio.AudioAction.PlayASharp;

            _audioModule.NoteSequence[5] = Audio.AudioAction.PlayG;
            _audioModule.NoteSequence[6] = Audio.AudioAction.PlayFSharp;
            _audioModule.NoteSequence[7] = Audio.AudioAction.PlayF;
            _audioModule.NoteSequence[8] = Audio.AudioAction.PlayE;
            _audioModule.NoteSequence[9] = Audio.AudioAction.PlayDSharp;
            _audioModule.NoteSequence[10] = Audio.AudioAction.PlayD;

            _audioModule.NoteSequence[11] = Audio.AudioAction.OctaveUp;
            _audioModule.NoteSequence[12] = Audio.AudioAction.OctaveUp;
            _audioModule.NoteSequence[13] = Audio.AudioAction.PlayASharp;
            _audioModule.NoteSequence[14] = Audio.AudioAction.PlayB;
            _audioModule.NoteSequence[15] = Audio.AudioAction.PlayC;
            _audioModule.NoteSequence[16] = Audio.AudioAction.PlayCSharp;
            _audioModule.NoteSequence[17] = Audio.AudioAction.PlayD;

            _audioModule.NoteSequence[18] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[19] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[20] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[21] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[22] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[22] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[23] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[24] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[25] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[26] = Audio.AudioAction.PlayD;
            _audioModule.NoteSequence[27] = Audio.AudioAction.PlayD;

            _audioModule.NoteSequence[28] = Audio.AudioAction.AddNoise;
            _audioModule.NoteSequence[29] = Audio.AudioAction.OctaveUp;
            _audioModule.NoteSequence[30] = Audio.AudioAction.PlayGSharp;
            _audioModule.NoteSequence[31] = Audio.AudioAction.PlayG;
            _audioModule.NoteSequence[32] = Audio.AudioAction.PlayFSharp;
            _audioModule.NoteSequence[33] = Audio.AudioAction.PlayG;
            _audioModule.NoteSequence[34] = Audio.AudioAction.PlayFSharp;
            _audioModule.NoteSequence[35] = Audio.AudioAction.PlayF;


            _audioModule
                .GetSound(0)
                .Set(5, 32, 6);

            _audioModule
                .GetSound(1)
                .Set(0, 4, 5);

            //jump
            _audioModule
                .GetSound(2)
                .Set(11, 2, 7);

            _audioModule
               .GetSound(3)
               .Set(18, 64, 1);

            //noise
            _audioModule
                .GetSound(4)
                .Set(28, 4, 8);

            _audioModule
                .GetSound(5)
                .Set(0, 0, 0);

            _audioModule.PrepareSounds();
        }
        
        public void Update()
        {
            _audioModule.OnLogicUpdate();
        }

        public void PlaySound(Sound sound)
        {
            switch(sound)
            {
                case Sound.PlayerDie:
                    _audioModule.GetChannel(0).Play(0);
                    break;
                case Sound.PlayerHit:
                    _audioModule.GetChannel(0).Play(1);
                    break;
                case Sound.Jump:
                    _audioModule.GetChannel(0).Play(2);
                    break;
                case Sound.Test:
                     _audioModule.GetChannel(0).Play(3);
                    break;
                case Sound.Noise:
                    _audioModule.GetChannel(0).Play(4);
                    break;
            }
        }
    }
}
