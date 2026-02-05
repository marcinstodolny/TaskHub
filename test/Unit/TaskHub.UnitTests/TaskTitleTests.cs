using TaskHub.Domain.ValueObjects;
using Xunit;

namespace TaskHub.UnitTests
{
    public class TaskTitleTests
    {
        [Theory]
        [InlineData("Valid Title")]
        [InlineData("  Trimmed Title  ")]
        public void Create_TaskItemTitle_Valid_ShouldSuccess(string title)
        {
            var result = TaskItemTitle.Create(title);

            Assert.True(result.IsSuccess);
            Assert.Equal(title.Trim(), result.Value.Value);
        }

        [Fact]
        public void Create_TaskListTitle_Valid_ShouldSuccess()
        {
            var title = new string('x', TaskListTitle.MaxLength);

            var result = TaskListTitle.Create(title);

            Assert.True(result.IsSuccess);
            Assert.Equal(title, result.Value.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_Invalid_ShouldFail(string? title)
        {
            var itemResult = TaskItemTitle.Create(title);
            var listResult = TaskListTitle.Create(title);

            Assert.True(itemResult.IsFailed);
            Assert.True(listResult.IsFailed);
        }

        [Fact]
        public void Create_TaskItemTitle_TooLong_ShouldFail()
        {
            var longText = new string('x', TaskItemTitle.MaxLength + 1);

            var result = TaskItemTitle.Create(longText);

            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("exceed"));
        }

        [Fact]
        public void Create_TaskListTitle_TooLong_ShouldFail()
        {
            var longText = new string('x', TaskListTitle.MaxLength + 1);

            var result = TaskListTitle.Create(longText);

            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains("exceed"));
        }
    }
}