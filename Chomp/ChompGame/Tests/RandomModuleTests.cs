using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using System.Diagnostics;
using Xunit;

namespace ChompGame.Tests
{
    public class RandomModuleTests
    {
        const int MaxDeviation = 15;

        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(75)]
        public void RandomChanceHasAcceptableRange(int chance)
        {
            var specs = new PongSpecs();
            var rng = new RandomModule(null);
            SystemMemory memory = new SystemMemory(b =>
            {
                b.AddBytes(256);
            }, specs);

            rng.BuildMemory(new SystemMemoryBuilder(memory, specs));

            int total = 0;
            int inRange = 0;

            for (int i = 0; i < 100000; i++)
            {
                if (rng.RandomChance(chance))
                    inRange++;

                total++;
            }

            int actual = (int)(((double)inRange / total) * 100);
            Debug.WriteLine($"Expected = {chance}, Actual = {actual}");
            Assert.InRange(actual, chance - MaxDeviation, chance + MaxDeviation);
        }
    }
}
