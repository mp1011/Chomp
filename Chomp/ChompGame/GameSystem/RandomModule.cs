using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.GameSystem
{
    class RandomModule : Module
    {
        private GameByte _state;

        public RandomModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _state = memoryBuilder.AddByte();
            _state.Value = 1;
        }

        public override void OnStartup()
        {
        }

        public byte Generate(int bits)
        {
            byte randomValue = 0;
            while(bits-- > 0)
            {
                randomValue = (byte)(randomValue << 1);
                randomValue |= NextBit();                
            }

            return randomValue;
        }

        public byte GenerateByte() => Generate(8);

        public byte GenerateRange(int max) =>
            (byte)(GenerateByte() % max);

        private byte NextBit()
        {
            var randomValue = _state.Value;

            var newBit = randomValue
                            ^ (randomValue >> 1)
                            ^ (randomValue >> 2)
                            ^ (randomValue >> 7);
            newBit = newBit & 1;

            _state.Value = (byte)((_state >> 1) | (newBit << 7));

            return (byte)newBit;
        }

        public T RandomItem<T>(params T[] choices)
        {
            return choices[GenerateRange((byte)choices.Length)];
        }

        public bool RandomChance(int percent)
        {
            return GenerateRange(100) <= percent;
        }

        public byte FixedRandom(byte seed, byte bits)
        {
            var originalState = _state.Value;
            _state.Value = seed;
            var ret = Generate(bits);
            _state.Value = originalState;
            return ret;
        }
    }
}
