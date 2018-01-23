using Xunit;

namespace Serilog.Sinks.ElkStreams.Tests
{
    public class StringExtentionsTests
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("1234567890123456", "1234567890123456")]
        [InlineData("12345678901234567", "12[truncated 15]")]
        [InlineData("12345678901234567890", "12[truncated 18]")]
        [InlineData("123456789012345678901234567890", "12[truncated 28]")]

        public void TestTruncate(string initial, string expeñted)
        {
            Assert.Equal(expeñted, initial.Truncate(16));
        }
    }
}