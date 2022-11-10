using ChompGame.Data;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework.Audio;

namespace ChompGame.Audio
{
    class AudioChannel
    {
        private readonly BankAudioModule _audioModule;

        public int Address => _timer.Address;
        public static int Bytes => 5;

        private GameByte _timer;
        private SoundEffectInstance _currentSound;

        public AudioChannel(SystemMemoryBuilder memoryBuilder, BankAudioModule audioModule)
        {
            _audioModule = audioModule;
            _timer = memoryBuilder.AddByte();
        }

        public AudioChannel(int address, SystemMemory systemMemory, BankAudioModule audioModule)
        {
            _audioModule = audioModule;
            _timer = new GameByte(address, systemMemory);
        }

        public void Play(int soundIndex)
        {
            StopCurrentSound();
            _currentSound = _audioModule.SoundBank.CreateInstance(soundIndex);
            _currentSound.Play();
        }

        private void StopCurrentSound()
        {
            if (_currentSound == null)
                return;

            if (_currentSound.State == SoundState.Playing)
                _currentSound.Stop();

            _currentSound.Dispose();
            _currentSound = null;
        }

        public void Update()
        {
           
        }
    }
}
