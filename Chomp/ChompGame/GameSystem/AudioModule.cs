using Microsoft.Xna.Framework.Audio;
using System;

namespace ChompGame.GameSystem
{
    abstract class BaseAudioModule : Module, ILogicUpdateModule
    {
        public BaseAudioModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        public abstract void OnLogicUpdate();

        public SoundEffect GenerateWave(ushort frequency, byte volume, double seconds, int overtones, Func<double,double> function)
        {
            double theta = (frequency * (2 * Math.PI)) / (double)44100;
            double v = ((double)volume / 255.0) * 30000;
            return Generate(i =>
            {
                var tone = (short)(v * function(i * theta));
                
                for (int ot=1; ot <= overtones; ot++)
                {
                    double theta2 = (frequency * Math.Pow(2,ot) * (2 * Math.PI)) / (double)44100;
                    var overtone = (short)(v / (2 * ot) * function(i * theta2));
                    tone += overtone;
                }

                return tone;

            }, seconds);
        }
        public SoundEffect GenerateSpecial(ushort frequency, byte volume, double seconds)
        {
            return GenerateWave(frequency, volume, seconds, 2, Math.Sin);
        }

        public SoundEffect GenerateSine(ushort frequency, byte volume, double seconds)
        {
            return GenerateWave(frequency, volume, seconds, 3, Math.Sin);
        }

        public SoundEffect GenerateSine_Plain(ushort frequency, byte volume, double seconds)
        {
            double theta = (frequency * (2 * Math.PI)) / (double)44100;
            double v = ((double)volume / 255.0) * 30000;
            return Generate(i => (short)(v * Math.Sin(i * theta)), seconds);
        }

        public SoundEffect Generate(Func<int,short> generator, double seconds)
        {
            byte[] buffer = new byte[(int)(44100 * 2 * seconds)];
            for (int i = 0; i < buffer.Length/2; i++)
            {
                short value = generator(i);
                buffer[2*i] = (byte)value;
                buffer[(2*i)+1] = (byte)(value >> 8);
            }

            return new SoundEffect(buffer, 44100, AudioChannels.Mono);
        }

    }
}
