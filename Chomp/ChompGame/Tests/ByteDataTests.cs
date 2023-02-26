using ChompGame.Data;
using ChompGame.GameSystem;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ChompGame.Tests
{
    public class ByteDataTests
    {
        [Theory]
        [InlineData(Bit.Bit0,0,1)]
        [InlineData(Bit.Bit1,0,2)]
        [InlineData(Bit.Bit2,0,4)]
        [InlineData(Bit.Bit1, 108, 110)]
        public void CanSetBit(Bit bit, byte original, byte expected)
        {
            SystemMemory memory = new SystemMemory(b =>
            {
                b.AddByte();
            }, new PongSpecs());

            memory[0] = original;

            var gameBit = new GameBit(0, bit, memory);
            gameBit.Value = true;

            Assert.True(gameBit.Value);
            Assert.Equal(expected, memory[0]);
            

        }

        [Fact]
        public void CanSetMaskedByte()
        {
            SystemMemory memory = new SystemMemory(b =>
            {
                b.AddByte();
            }, new PongSpecs());

            var mb1 = new MaskedByte(0, (Bit)28, memory, leftShift:2);
            var mb2 = new MaskedByte(0, (Bit)224, memory, leftShift:5);

            mb1.Value = 5;
            mb2.Value = 4;

            Assert.Equal(5, mb1.Value);
            Assert.Equal(4, mb2.Value);

        }
    }
}
