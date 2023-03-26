using ChompGame.Data;
using ChompGame.Data.Memory;
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

        public byte SetData(byte index, string[] data)
        {
            foreach(var token in data)
            {
                this[index++] = token switch
                {
                    "+" => AudioAction.OctaveUp,
                    "-" => AudioAction.OctaveDown,
                    "A" => AudioAction.PlayA,
                    "A#" => AudioAction.PlayASharp,
                    "B" => AudioAction.PlayB,
                    "C" => AudioAction.PlayC,
                    "C#" => AudioAction.PlayCSharp,
                    "D" => AudioAction.PlayD,
                    "D#" => AudioAction.PlayDSharp,
                    "E" => AudioAction.PlayE,
                    "F" => AudioAction.PlayF,
                    "F#" => AudioAction.PlayFSharp,
                    "G" => AudioAction.PlayG,
                    "G#" => AudioAction.PlayGSharp,
                    "*" => AudioAction.AddNoise,
                    _ => AudioAction.Rest,
                };
            }

            return index;
        }
    }
}
