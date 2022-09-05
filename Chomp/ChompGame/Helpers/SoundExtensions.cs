using ChompGame.Data;
using System;

namespace ChompGame.Helpers
{
    static class SoundExtensions
    {
        public static ushort GetFrequency(this MusicNote note, int octave)
        {
            int semitone = (int)note;
            var thisOctave = 110 * Math.Pow(2, octave);
            var frequency = thisOctave * Math.Pow(2, (double)semitone / 12.0);
            return (ushort)frequency;
        }
    }
}
