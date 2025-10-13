using TaskHub.Domain;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;
using Xunit;
using TaskItem = TaskHub.Domain.Entities.TaskItem;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.UnitTests
{
    public class TaskItemTests
    {
        private readonly TaskItem _taskItem;
        public TaskItemTests()
        {
            _taskItem = TaskItem.Create(Guid.NewGuid(), TaskTitle.Create("Test task").Value, null, TaskPriority.Normal).Value;
        }

        [Fact]
        public void Create_SetsDefaults_AndRaisesEvent()
        {
            // Arrange
            var title = TaskTitle.Create("Manual Testing").Value;
            // Act
            var result = TaskItem.Create(Guid.NewGuid(), title, null, TaskPriority.Normal);

            // Assert
            Assert.True(result.IsSuccess);
            var task = result.Value;
            Assert.NotEqual(Guid.Empty, task.Id);
            Assert.Equal(TaskStatus.Todo, task.Status);
        }

        [Fact]
        public void Create_Fails_If_Title_Is_Null()
        {
            // Arrange
            TaskTitle? title = null;

            // Act
            var result = TaskItem.Create(Guid.NewGuid(), title, null, TaskPriority.Normal);

            // Assert
            Assert.True(result.IsFailed);
        }

        [Fact]
        public void Cancel_AlreadyCanceled_ShouldFails()
        {
            // Arrange
            _taskItem.Cancel(DateTime.Now);

            // Act
            var result = _taskItem.Cancel(DateTime.Now);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(TaskStatus.Cancelled, _taskItem.Status);
        }

        [Fact]
        public void Complete_AlreadyCompleted_ShouldFails()
        {
            // Arrange
            _taskItem.Complete(DateTime.Now);

            // Act
            var result = _taskItem.Complete(DateTime.Now);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(TaskStatus.Done, _taskItem.Status);
        }

        [Fact]
        public void Start_AlreadyStarted_ShouldFails()
        {
            // Arrange
            _taskItem.Start(DateTime.Now);

            // Act
            var result = _taskItem.Start(DateTime.Now);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(TaskStatus.InProgress, _taskItem.Status);
        }

        [Fact]
        public void UpdateTitle_UpdatingTitle_AndRaisesEvent()
        {
            // Arrange
            var title = TaskTitle.Create("Manual Testing").Value;
            var task = TaskItem.Create(Guid.NewGuid(), title, null, TaskPriority.Normal).Value;

            var newTitle = TaskTitle.Create("Different title").Value;

            // Act
            var result = task.UpdateTitle(newTitle);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newTitle, task.Title);

            var last = task.DomainEvents[^1];
            var ev = Assert.IsType<TaskUpdated>(last);
            Assert.Equal(task.Id, ev.TaskId);
        }

        [Fact]
        public void UpdateTitle_Fails_If_Title_Is_Null()
        {
            // Arrange

            // Act
            var result = _taskItem.UpdateTitle(null);

            // Assert
            Assert.True(result.IsFailed);
        }

    }
}
