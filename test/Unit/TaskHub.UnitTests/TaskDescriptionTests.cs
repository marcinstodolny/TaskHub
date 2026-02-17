using TaskHub.Domain.ValueObjects;
using Xunit;

namespace TaskHub.UnitTests
{
    public class TaskDescriptionTests
    {
        public TaskDescriptionTests()
        {

        }

        [Theory]
        [InlineData("Valid Description")]
        [InlineData("  Trimmed Description  ")]
        public void Create_valid_shouldSuccess(string description)
        {
            var result = TaskDescription.Create(description);
            Assert.True(result.IsSuccess);
            Assert.Equal(description.Trim(), result.Value.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_invalid_ShouldFail(string? description)
        {
            var result = TaskDescription.Create(description);
            Assert.True(result.IsFailed);
        }

        [Fact]
        public void Create_TooLong_ShouldFail()
        {
            var longText = new string('x', TaskDescription.MaxLength + 1);

            var result = TaskDescription.Create(longText);

            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Contains("exceed"));
        }
    }
}
