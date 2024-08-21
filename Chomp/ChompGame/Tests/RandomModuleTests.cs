using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(16)]
        public void RandomChanceIsEvenlyDistributed(byte max)
        {
            var specs = new PongSpecs();
            var rng = new RandomModule(null);
            SystemMemory memory = new SystemMemory(b =>
            {
                b.AddBytes(256);
            }, specs);

            rng.BuildMemory(new SystemMemoryBuilder(memory, specs));

            int[] counts = new int[max];

            for (int i = 0; i < 100000; i++)
            {
                var randomValue = rng.GenerateRange(max);
                counts[randomValue]++;
            }

            int expected = 100000 / max;

            foreach(var item in counts)
            {
                Assert.InRange(item, expected-500, expected+500);
            }
        }

        [Fact]
        public void RandomNumberIsEvenlyDistributed()
        {
            var specs = new PongSpecs();
            var rng = new RandomModule(null);
            SystemMemory memory = new SystemMemory(b =>
            {
                b.AddBytes(256);
            }, specs);

            rng.BuildMemory(new SystemMemoryBuilder(memory, specs));

            int[] counts = new int[256];
            var values = new List<byte>();
            for (int i = 0; i < 100000; i++)
            {
                var randomValue = rng.GenerateByte();
                values.Add(randomValue);
                counts[randomValue]++;
            }

            int expected = 100000 / 256;

            foreach (var item in counts)
            {
                Assert.InRange(item, expected-500, expected+500);
            }
        }
    }
}
