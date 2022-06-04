using ChompGame.Data;
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
            });

            memory[0] = original;

            var gameBit = new GameBit(0, bit, memory);
            gameBit.Value = true;

            Assert.True(gameBit.Value);
            Assert.Equal(expected, memory[0]);
            

        }
    }
}
