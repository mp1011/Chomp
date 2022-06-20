using Microsoft.Xna.Framework.Audio;

namespace ChompGame.Data
{
    class AudioChannel
    {
        private SoundEffectInstance _sfx;
        private readonly GameShort _value;
        private readonly GameBit _playing;

        public bool Playing
        {
            get => _playing.Value;
            set => _playing.Value = value;
        }

        public ushort Value
        {
            get => _value.Value;
            set
            {
                if(value != _value.Value)
                {
                    _value.Value = value;
                    Generate();
                }
            }
        }

        private void Generate()
        {
            byte[] buffer = new byte[(int)(44100 * 0.5)];
            buffer = new byte[44100 * 1];
            short v = 10000;
            for (int i = 0; i < buffer.Length; i += 2)
            {
                buffer[i] = (byte)v;
                buffer[i + 1] = (byte)(v >> 8);
                v += (short)Value;
            }


            var sfx = new SoundEffect(buffer, 44100, AudioChannels.Mono);
            _sfx = sfx.CreateInstance();
        }

        public void Update()
        {
            if (Playing && _sfx != null && _sfx.State == SoundState.Stopped)
                _sfx.Play();
        }

        public AudioChannel(GameShort value, GameBit playing)
        {
            _value = value;
            _playing = playing;
        }
    }
}
