using ChompGame.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ChompGame.Tests
{
    public class NBitPlaneTests
    {
        [Fact]
        public void CanSetFieldOnNBitPlane()
        {
            SystemMemory memory = new SystemMemory(b =>
            {
                b.AddBytes(256);
            });

            var plane = new NBitPlane(0, memory, 2, 4, 4);
            plane[0, 0] = 2;
            plane[1, 1] = 3;
            plane[2, 3] = 1;


            Assert.Equal(2, plane[0, 0]);
            Assert.Equal(3, plane[1, 1]);
            Assert.Equal(1, plane[2, 3]);

        }
    }
}
