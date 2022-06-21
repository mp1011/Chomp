using ChompGame.GameSystem;
using Microsoft.Xna.Framework.Audio;

namespace ChompGame.Data
{
    class AudioChannel
    {
        private readonly AudioModule _audioModule;
        private readonly GameByte _volume;
        private readonly GameShort _value;
        private readonly GameBit _playing;
        private SoundEffectInstance _sfx;

        public bool Playing
        {
            get => _playing.Value;
            set
            {
                if (_sfx != null && _sfx.State == SoundState.Playing && !value)
                    _sfx.Stop();

                _playing.Value = value;
            }
        }

        public ushort Value
        {
            get => _value.Value;
            set
            {
                if(value != _value.Value)
                {
                    _value.Value = value;
                    GenerateSound();
                }
            }
        }

        public byte Volume
        {
            get => _volume.Value;
            set
            {
                if (value != _volume.Value)
                {
                    _volume.Value = value;
                    GenerateSound();
                }
            }
        }

        private void GenerateSound()
        {
            if(_sfx != null && _sfx.State == SoundState.Playing)
                _sfx.Stop();

            _sfx = null;
            _sfx = _audioModule.GenerateSpecial(_value.Value, _volume.Value, 1).CreateInstance();
        }
       

        public void Update()
        {
            if (Playing && _sfx != null && _sfx.State == SoundState.Stopped)
                _sfx.Play();
        }

        public AudioChannel(AudioModule audio, GameShort value, GameByte volume, GameBit playing)
        {
            _audioModule = audio;
            _value = value;
            _volume = volume;
            _playing = playing;
        }
    }
}
