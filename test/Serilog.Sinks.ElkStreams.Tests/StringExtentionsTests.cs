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

        public void TestTruncate(string initial, string expected)
        {
            Assert.Equal(expected, initial.Truncate(16));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("Still no guids here", "Still no guids here")]
        [InlineData("8908da8e-6d2f-44b6-a5d3-90c36b4051d1", "")]
        [InlineData("a8908da8e-6d2f-44b6-a5d3-90c36b4051d1bc8908da8e-6d2f-44b6-a5d3-90c36b4051d1d", "abcd")]  
        [InlineData("8908da8e-6d2f-44b6-a5d3-90c36b4051d", "8908da8e-6d2f-44b6-a5d3-90c36b4051d")]
        public void TestRemoveGuids(string initial, string expected)
        {
            Assert.Equal(expected, initial.RemoveGuids());
        }
    }
}