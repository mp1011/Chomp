using ChompGame.Data;
using ChompGame.GameSystem;

namespace ChompGame.Audio
{
    class NoteSequence
    {
        private readonly NibbleArray _notes;

        public NoteSequence(SystemMemoryBuilder memoryBuilder, Specs specs)
        {
            _notes = new NibbleArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(specs.AudioROMBytes);
        }

        public AudioAction this[byte index]
        {
            get => (AudioAction)_notes[index];
            set
            {
                _notes[index] = (byte)value;
            }
        }
    }
}
