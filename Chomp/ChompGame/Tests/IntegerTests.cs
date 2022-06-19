using ChompGame.Data;
using ChompGame.GameSystem;
using Xunit;

namespace ChompGame.Tests
{
    public class IntegerTests
    {
        [Fact]
        public void TestGameShort()
        {
            GameShort s=null;
            SystemMemory memory = new SystemMemory(b =>
            {
                s = b.AddShort();
            }, new Specs());

            s.Value = 1000;
            Assert.Equal(1000, s.Value);

            s.Value = 8750;
            Assert.Equal(8750, s.Value);
        }
    }
}
