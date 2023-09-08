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
        }

        public override void OnStartup()
        {
        }

        public byte Next()
        {
            var randomValue = _randomValue.Value;

            _randomValue.Value = (byte)((_randomValue + 177) << 3);

            return randomValue;
        }

        public byte Next(byte max)
        {
            return (byte)(Next() % max);
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
