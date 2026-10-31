using TaskHub.Domain.Entities;
using TaskHub.Domain.ValueObjects;
using Xunit;

namespace TaskHub.UnitTests
{
    public class TaskListTests
    {
        private static readonly Guid validUserId = new("11111111-1111-1111-1111-111111111111");

        [Fact]
        public void Create_NullTitle_ShouldFail()
        {
            var creationResult = TaskList.Create(null!, validUserId);

            Assert.True(creationResult.IsFailed);
            Assert.Contains("Title is required.", creationResult.Errors);
        }

        [Fact]
        public void Rename_NullTitle_ShouldFailAndPreserveTitle()
        {
            var originalTitle = TaskListTitle.Create("Original title").Value;
            var taskList = TaskList.Create(originalTitle, validUserId).Value;

            var renameResult = taskList.Rename(null!);

            Assert.True(renameResult.IsFailed);
            Assert.Contains("Title is required.", renameResult.Errors);
            Assert.Same(originalTitle, taskList.Title);
        }

        [Fact]
        public void Create_Valid_ShouldSuccessAndSetTitleAndUserId()
        {
            var title = TaskListTitle.Create("Original title").Value;

            var creationResult = TaskList.Create(title, validUserId);

            Assert.True(creationResult.IsSuccess);
            Assert.Equal("Original title", creationResult.Value.Title.Value);
            Assert.Equal(validUserId, creationResult.Value.UserId);
        }

        [Fact]
        public void Rename_Valid_ShouldSuccessAndSetTitle()
        {
            var originalTitle = TaskListTitle.Create("Original title").Value;
            var taskList = TaskList.Create(originalTitle, validUserId).Value;
            var renamedTitle = TaskListTitle.Create("Renamed title").Value;

            var renameResult = taskList.Rename(renamedTitle);

            Assert.True(renameResult.IsSuccess);
            Assert.Equal("Renamed title", taskList.Title.Value);
        }

        [Fact]
        public void Create_EmptyUserId_ShouldFail()
        {
            var title = TaskListTitle.Create("Original title").Value;

            var creationResult = TaskList.Create(title, Guid.Empty);

            Assert.True(creationResult.IsFailed);
            Assert.Equal("User id is required.", Assert.Single(creationResult.Errors));
        }

        [Fact]
        public void Create_NullTitleAndEmptyUserId_ShouldFailWithUserIdError()
        {
            var creationResult = TaskList.Create(null!, Guid.Empty);

            Assert.True(creationResult.IsFailed);
            Assert.Equal("User id is required.", Assert.Single(creationResult.Errors));
        }
    }
}
