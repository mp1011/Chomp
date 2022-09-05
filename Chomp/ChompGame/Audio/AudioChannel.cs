using ChompGame.Data;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework.Audio;

namespace ChompGame.Audio
{
    class AudioChannel
    {
        private readonly BankAudioModule _audioModule;

        public int Address => _sequencePointer.Address;
        public static int Bytes => 5;

        private GameByte _sequencePointer;
        private GameByte _noteDuration;
        private GameByte _remainingNotes;
        private GameByte _currentOctave;
        private GameByte _timer;

        private SoundEffectInstance _currentSound;

        public AudioChannel(SystemMemoryBuilder memoryBuilder, BankAudioModule audioModule)
        {
            _audioModule = audioModule;
            _sequencePointer = memoryBuilder.AddByte();
            _noteDuration = memoryBuilder.AddByte();
            _remainingNotes = memoryBuilder.AddByte();
            _currentOctave = memoryBuilder.AddByte();
            _timer = memoryBuilder.AddByte();
        }

        public AudioChannel(int address, SystemMemory systemMemory, BankAudioModule audioModule)
        {
            _audioModule = audioModule;
            _sequencePointer = new GameByte(address, systemMemory);
            _noteDuration = new GameByte(address + 1, systemMemory);
            _remainingNotes = new GameByte(address + 2, systemMemory);
            _currentOctave = new GameByte(address + 3, systemMemory);
            _timer = new GameByte(address + 4, systemMemory);
        }

        public void Play(byte sequenceStart, byte noteDuration, byte sequenceLength)
        {
            _sequencePointer.Value = sequenceStart;
            _noteDuration.Value = noteDuration;
            _remainingNotes.Value = sequenceLength;
            _currentOctave.Value = 0;

            if (HandleAudioAction())
                _timer.Value = _noteDuration.Value;
            else
                _timer.Value = 1;
        }

        private bool HandleAudioAction()
        {
            var action = _audioModule.NoteSequence[_sequencePointer.Value];
            switch(action)
            {
                case AudioAction.OctaveUp:
                    _currentOctave.Value++;
                    return false;
                case AudioAction.OctaveDown:
                    _currentOctave.Value--;
                    return false;
                case AudioAction.Unused:
                case AudioAction.Rest:
                    if (_currentSound != null)
                        _currentSound.Stop();
                    return true;
                default:

                    if (_currentSound != null)
                        _currentSound.Stop();

                    int toneIndex = (_currentOctave.Value * 12) + (int)action;
                    _currentSound = _audioModule.ToneBank.CreateInstance(toneIndex);
                    _currentSound.Play();
                    return true; ;
            }
        }

        public void Update()
        {
            if (_timer.Value == 0)
                return;

            _timer.Value--;
            if(_timer.Value == 0)
            {
                _remainingNotes.Value--;
                if (_remainingNotes.Value == 0)
                {
                    if (_currentSound != null)
                        _currentSound.Stop();
                    return;
                }

                _sequencePointer.Value++;

                if (HandleAudioAction())
                    _timer.Value = _noteDuration.Value;
                else
                    _timer.Value = 1;                
            }
        }
    }
}
