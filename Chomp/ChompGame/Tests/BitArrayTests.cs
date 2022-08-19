using ChompGame.Data;
using ChompGame.GameSystem;
using Xunit;

namespace ChompGame.Tests
{
    public class BitArrayTests
    {
        [Fact]
        public void TestBitArray()
        {
            SystemMemory memory = new SystemMemory(b =>
            {
                b.AddBytes(256);
            }, new PongSpecs());

            BitArray array = new BitArray(0, memory);

            array[0] = true;

            Assert.True(array[0]);
            Assert.False(array[1]);

            array[1] = true;
            Assert.True(array[0]);
            Assert.True(array[1]);

            array[0] = false;
            Assert.False(array[0]);
            Assert.True(array[1]);
        }

        [Fact]
        public void TestTwoBitArray()
        {
            SystemMemory memory = new SystemMemory(b =>
            {
                b.AddBytes(256);
            }, new PongSpecs());

            TwoBitArray array = new TwoBitArray(
                new BitArray(0, memory),
                new BitArray(100, memory));

            array[0] = 2;
            Assert.Equal(2, array[0]);
            Assert.Equal(0, array[1]);

            array[1] = 3;
            Assert.Equal(2, array[0]);
            Assert.Equal(3, array[1]);

            array[0] = 1;
            Assert.Equal(1, array[0]);
            Assert.Equal(3, array[1]);
        }
    }
}
