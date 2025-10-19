using TaskHub.Domain.ValueObjects;
using Xunit;

namespace TaskHub.UnitTests
{
    public class TaskTitleTests
    {
        public TaskTitleTests()
        {

        }

        [Theory]
        [InlineData("Valid Title")]
        [InlineData("  Trimmed Title  ")]
        public void Create_valid_shouldSuccess(string title)
        {
            var result = Title.Create(title);
            Assert.True(result.IsSuccess);
            Assert.Equal(title.Trim(), result.Value.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_invalid_ShouldFail(string? title)
        {
            var result = Title.Create(title);
            Assert.True(result.IsFailed);
        }

        [Fact]
        public void Create_TooLong_ShouldFail()
        {
            var longText = new string('x', 101);

            var result = Title.Create(longText);

            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("exceed"));
        }
    }
}
