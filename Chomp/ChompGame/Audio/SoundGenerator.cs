using ChompGame.Data;
using ChompGame.Helpers;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.Audio
{
    static class SoundGenerator
    {
        private const int SampleRate = 44100;

        public static SoundEffect Generate(Func<int, short> generator, double seconds)
        {
            byte[] buffer = new byte[(int)(44100 * 2 * seconds)];

            for (int i = 0; i < buffer.Length / 2; i++)
            {
                short value = generator(i);
                buffer[2 * i] = (byte)value;
                buffer[(2 * i) + 1] = (byte)(value >> 8);
            }

            return new SoundEffect(buffer, 44100, AudioChannels.Mono);
        }

        public static SoundEffect Generate(List<short> data)
        {
            byte[] buffer = new byte[data.Count * 2];
            for (int i = 0; i < buffer.Length / 2; i++)
            {
                short value = data[i];
                buffer[2 * i] = (byte)value;
                buffer[(2 * i) + 1] = (byte)(value >> 8);
            }

            return new SoundEffect(buffer, SampleRate, AudioChannels.Mono);
        }

        public static SoundEffect Generate(SoundHeader soundHeader, NoteSequence noteSequence)
        {
            byte octave = 0;
            byte sequenceIndex = soundHeader.SequenceStart;

            List<short> soundData = new List<short>();

            while (sequenceIndex < soundHeader.SequenceStart + soundHeader.SequenceLength)
            {
                AudioAction audioAction = noteSequence[sequenceIndex];

                switch (audioAction)
                {
                    case AudioAction.OctaveUp:
                        octave++;
                        break;
                    case AudioAction.OctaveDown:
                        octave--;
                        break;
                    case AudioAction.Rest:
                        AddRest(soundData, soundHeader.NoteDuration);
                        break;
                    case AudioAction.Unused:
                        break;
                    default:
                        MusicNote note = (MusicNote)audioAction;
                        AddNote(soundData, note, octave, soundHeader.NoteDuration);
                        break;
                }

                sequenceIndex++;
            }

            return Generate(soundData);
        }

        private static void AddRest(List<short> soundData, byte duration)
        {
            double seconds = duration * 0.01;

            for (int i = 0; i < seconds * SampleRate; i++)
            {
                soundData.Add(0);
            }
        }

        private static void AddNote(List<short> soundData, MusicNote note, byte octave, byte duration)
        {
            double seconds = duration * 0.01;

            double theta = (note.GetFrequency(octave) * 2 * Math.PI) / (double)SampleRate;
            double volume = 30000;

            for(int i = 0; i < seconds * SampleRate; i++)
            {
                soundData.Add((short)(volume * Math.Sin(i * theta)));
            }

            int ix = (int)(seconds * SampleRate) - 1;
            short sx = (short)(volume * Math.Sin(ix * theta));
            if (sx == 0)
                return;
            else if(sx < 0)
            {
                while(true)
                {
                    ix++;
                    sx = (short)(volume * Math.Sin(ix * theta));
                    if (sx >= 0)
                        return;
                    else
                        soundData.Add(sx);
                }
            }
            else if(sx > 0)
            {
                while (true)
                {
                    ix++;
                    sx = (short)(volume * Math.Sin(ix * theta));
                    if (sx <= 0)
                        break;
                    else
                        soundData.Add(sx);
                }

                while (true)
                {
                    ix++;
                    sx = (short)(volume * Math.Sin(ix * theta));
                    if (sx >= 0)
                        return;
                    else
                        soundData.Add(sx);
                }
            }
        }

        //public SoundEffect GenerateWave(ushort frequency, byte volume, double seconds, int overtones, Func<double, double> function)
        //{
        //    double theta = (frequency * (2 * Math.PI)) / (double)44100;
        //    double v = ((double)volume / 255.0) * 30000;
        //    return Generate(i =>
        //    {
        //        var tone = (short)(v * function(i * theta));

        //        for (int ot = 1; ot <= overtones; ot++)
        //        {
        //            double theta2 = (frequency * Math.Pow(2, ot) * (2 * Math.PI)) / (double)44100;
        //            var overtone = (short)(v / (2 * ot) * function(i * theta2));
        //            tone += overtone;
        //        }

        //        return tone;

        //    }, seconds);
        //}

        //public SoundEffect GenerateSine(ushort frequency, byte volume, double seconds)
        //{
        //    return GenerateWave(frequency, volume, seconds, 3, Math.Sin);
        //}

       
    }
}
