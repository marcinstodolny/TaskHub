using TaskHub.Domain;
using Xunit;
using Task = TaskHub.Domain.Task;
using TaskStatus = TaskHub.Domain.TaskStatus;

namespace TaskHub.UnitTests
{
    public class TaskTests
    {
        public TaskTests()
        {

        }

        [Fact]
        public void Create_SetsDefaults_AndRaisesEvent()
        {
            // Arrange
            var title = TaskTitle.Create("Manual Testing").Value;
            var now = DateTime.UtcNow;
            // Act
            var result = Task.Create(title, now);

            // Assert
            Assert.True(result.IsSuccess);
            var task = result.Value;
            Assert.NotEqual(Guid.Empty, task.Id);
            Assert.Equal(TaskStatus.New, task.Status);
            Assert.Equal(now, task.CreatedAt);

            // Domain event
            Assert.Single(task.DomainEvents);
            Assert.IsType<TaskCreated>(task.DomainEvents[0]);
            var ev = (TaskCreated)task.DomainEvents[0];
            Assert.Equal(task.Id, ev.TaskId);
        }

        [Fact]
        public void Create_Fails_If_Title_Is_Null()
        {
            // Arrange
            TaskTitle? title = null;

            // Act
            var result = Task.Create(title!, DateTime.Now);

            // Assert
            Assert.True(result.IsFailed);
        }

        [Fact]
        public void ChangeStatus_AllowedTransition_RaisesEvent()
        {
            // Arrange
            var title = TaskTitle.Create("Manual Testing").Value;
            var task = Task.Create(title, DateTime.Now).Value;

            // Act
            var result = task.ChangeStatus(TaskStatus.InProgress);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(TaskStatus.InProgress, task.Status);

            var last = task.DomainEvents[^1];
            var ev = Assert.IsType<TaskUpdated>(last);
            Assert.Equal(task.Id, ev.TaskId);
        }

        [Fact]
        public void ChangeStatus_DisallowedTransition_Fails()
        {
            // Arrange
            var title = TaskTitle.Create("Manual Testing").Value;
            var task = Task.Create(title, DateTime.Now).Value;

            var result = task.ChangeStatus(TaskStatus.Done);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(TaskStatus.New, task.Status);
        }

        [Fact]
        public void UpdateTitle_UpdatingTitle_AndRaisesEvent()
        {
            // Arrange
            var title = TaskTitle.Create("Manual Testing").Value;
            var task = Task.Create(title, DateTime.Now).Value;

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
            var title = TaskTitle.Create("Manual Testing").Value;
            var task = Task.Create(title, DateTime.Now).Value;

            // Act
            var result = task.UpdateTitle(null);

            // Assert
            Assert.True(result.IsFailed);
        }

    }
}
