using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.GameSystem
{
    class RandomModule : Module
    {
        private GameByte _randomValue;

        public RandomModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _randomValue = memoryBuilder.AddByte();
            _randomValue.Value = 1;
        }

        public override void OnStartup()
        {
        }

        public byte Next()
        {
            var randomValue = _randomValue.Value;

            var b1 = (randomValue & 1) > 0 ? 1 : 0;
            var b2 = (randomValue & 2) > 0 ? 1 : 0;
            var b3 = (randomValue & 3) > 0 ? 1 : 0;
            var b4 = (randomValue & 7) > 0 ? 1 : 0;

            var add = b1 ^ b2 ^ b3 ^ b4;

            _randomValue.Value = (byte)(_randomValue >> 1);
            _randomValue.Value = (byte)(_randomValue + (add << 7));

            return randomValue;
        }

        public byte Next(byte max)
        {
            var sample = Next() / 255.0;
            return (byte)(sample * max);
        }

        public static byte FixedRandom(byte seed)
        {
            var b1 = (seed & 1) > 0 ? 1 : 0;
            var b2 = (seed & 2) > 0 ? 1 : 0;
            var b3 = (seed & 3) > 0 ? 1 : 0;
            var b4 = (seed & 7) > 0 ? 1 : 0;

            var add = b1 ^ b2 ^ b3 ^ b4;

            var result = (byte)(seed >> 1);
            result = (byte)(result + (add << 7));

            return result;
        }

        public T RandomItem<T>(params T[] choices)
        {
            return choices[Next((byte)choices.Length)];
        }

        public bool RandomChance(int percent)
        {
            return Next(100) <= percent;
        }
    }
}
