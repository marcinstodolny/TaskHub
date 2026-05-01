using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Application.Features.TaskItem.Commands;
using TaskHub.Application.Features.TaskList.Commands;
using TaskHub.Domain.Common;
using TaskHub.Domain.Entities;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;
using Xunit;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.UnitTests;

public class TaskActivityNotificationTests
{
    private static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateTime NowUtc = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateTaskList_WhenSuccessful_ShouldNotifyAfterSaveWithCreatedTaskListId()
    {
        var unitOfWork = new FakeUnitOfWork();
        var activityRepository = new RecordingTaskActivityCommandRepository();
        var notifier = new RecordingTaskActivityNotifier(() => unitOfWork.SaveCompleted, () => activityRepository.Activities.Count);
        var handler = new CreateTaskListCommandHandler(
            unitOfWork,
            new FakeTaskListCommandRepository(),
            activityRepository,
            new FakeCurrentUserAccessor(UserId),
            notifier,
            new FakeDateTimeProvider());

        var result = await handler.Handle(new CreateTaskListCommand("Planning"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        AssertSingleNotification(notifier, result.Value.Id);
    }

    [Fact]
    public async Task CreateTaskList_WhenRequestIsCancelledAfterSave_ShouldStillReturnSuccess()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var unitOfWork = new FakeUnitOfWork(afterSave: cancellationTokenSource.Cancel);
        var activityRepository = new RecordingTaskActivityCommandRepository();
        var notifier = new RecordingTaskActivityNotifier(() => unitOfWork.SaveCompleted, () => activityRepository.Activities.Count);
        var handler = new CreateTaskListCommandHandler(
            unitOfWork,
            new FakeTaskListCommandRepository(),
            activityRepository,
            new FakeCurrentUserAccessor(UserId),
            notifier,
            new FakeDateTimeProvider());

        var result = await handler.Handle(new CreateTaskListCommand("Planning"), cancellationTokenSource.Token);

        Assert.True(result.IsSuccess);
        AssertSingleNotification(notifier, result.Value.Id);
    }

    [Fact]
    public async Task UpdateTaskList_WhenSuccessful_ShouldNotifyAfterSaveWithTaskListId()
    {
        var taskList = CreateTaskList();
        var unitOfWork = new FakeUnitOfWork();
        var activityRepository = new RecordingTaskActivityCommandRepository();
        var notifier = new RecordingTaskActivityNotifier(() => unitOfWork.SaveCompleted, () => activityRepository.Activities.Count);
        var handler = new UpdateTaskListCommandHandler(
            unitOfWork,
            new FakeTaskListCommandRepository(taskList),
            activityRepository,
            notifier,
            new FakeDateTimeProvider());

        var result = await handler.Handle(new UpdateTaskListCommand(taskList.Id, "Renamed"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        AssertSingleNotification(notifier, taskList.Id);
    }

    [Fact]
    public async Task CreateTaskItem_WhenSuccessful_ShouldNotifyAfterSaveWithTaskListId()
    {
        var taskList = CreateTaskList();
        var unitOfWork = new FakeUnitOfWork();
        var activityRepository = new RecordingTaskActivityCommandRepository();
        var notifier = new RecordingTaskActivityNotifier(() => unitOfWork.SaveCompleted, () => activityRepository.Activities.Count);
        var handler = new CreateTaskItemHandler(
            unitOfWork,
            new FakeTaskListCommandRepository(taskList),
            new FakeTaskItemCommandRepository(),
            activityRepository,
            notifier,
            new FakeDateTimeProvider());

        var result = await handler.Handle(
            new CreateTaskItemCommand(taskList.Id, "Write tests", null, TaskPriority.Normal),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        AssertSingleNotification(notifier, taskList.Id);
    }

    [Fact]
    public async Task UpdateTaskItem_WhenSuccessful_ShouldNotifyAfterSaveWithTaskListId()
    {
        var taskList = CreateTaskList();
        var taskItem = CreateTaskItem(taskList.Id);
        var unitOfWork = new FakeUnitOfWork();
        var activityRepository = new RecordingTaskActivityCommandRepository();
        var notifier = new RecordingTaskActivityNotifier(() => unitOfWork.SaveCompleted, () => activityRepository.Activities.Count);
        var handler = new UpdateTaskItemCommandHandler(
            unitOfWork,
            new FakeTaskItemCommandRepository(taskItem),
            activityRepository,
            notifier,
            new FakeDateTimeProvider());

        var result = await handler.Handle(
            new UpdateTaskItemCommand(taskItem.Id, "Updated task", "Description", TaskPriority.High),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        AssertSingleNotification(notifier, taskList.Id);
    }

    [Fact]
    public async Task UpdateTaskItemStatus_WhenSuccessful_ShouldNotifyAfterSaveWithTaskListId()
    {
        var taskList = CreateTaskList();
        var taskItem = CreateTaskItem(taskList.Id);
        var unitOfWork = new FakeUnitOfWork();
        var activityRepository = new RecordingTaskActivityCommandRepository();
        var notifier = new RecordingTaskActivityNotifier(() => unitOfWork.SaveCompleted, () => activityRepository.Activities.Count);
        var handler = new UpdateTaskItemStatusCommandHandler(
            unitOfWork,
            new FakeTaskItemCommandRepository(taskItem),
            activityRepository,
            notifier,
            new FakeDateTimeProvider());

        var result = await handler.Handle(
            new UpdateTaskItemStatusCommand(taskItem.Id, TaskStatus.InProgress),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        AssertSingleNotification(notifier, taskList.Id);
    }

    [Fact]
    public async Task DeleteTaskItem_WhenSuccessful_ShouldNotifyAfterSaveWithTaskListId()
    {
        var taskList = CreateTaskList();
        var taskItem = CreateTaskItem(taskList.Id);
        var unitOfWork = new FakeUnitOfWork();
        var activityRepository = new RecordingTaskActivityCommandRepository();
        var notifier = new RecordingTaskActivityNotifier(() => unitOfWork.SaveCompleted, () => activityRepository.Activities.Count);
        var handler = new DeleteTaskItemCommandHandler(
            unitOfWork,
            new FakeTaskItemCommandRepository(taskItem),
            activityRepository,
            notifier,
            new FakeDateTimeProvider());

        var result = await handler.Handle(new DeleteTaskItemCommand(taskItem.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        AssertSingleNotification(notifier, taskList.Id);
    }

    [Fact]
    public async Task CreateTaskItem_WhenTaskListIsMissing_ShouldNotNotify()
    {
        var unitOfWork = new FakeUnitOfWork();
        var notifier = new RecordingTaskActivityNotifier(() => unitOfWork.SaveCompleted);
        var handler = new CreateTaskItemHandler(
            unitOfWork,
            new FakeTaskListCommandRepository(),
            new FakeTaskItemCommandRepository(),
            new RecordingTaskActivityCommandRepository(),
            notifier,
            new FakeDateTimeProvider());

        var result = await handler.Handle(
            new CreateTaskItemCommand(Guid.NewGuid(), "Missing list task", null, TaskPriority.Normal),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(notifier.Notifications);
    }

    [Fact]
    public async Task DeleteTaskItem_WhenSaveFails_ShouldNotNotify()
    {
        var taskList = CreateTaskList();
        var taskItem = CreateTaskItem(taskList.Id);
        var unitOfWork = new FakeUnitOfWork(throwOnSave: true);
        var notifier = new RecordingTaskActivityNotifier(() => unitOfWork.SaveCompleted);
        var handler = new DeleteTaskItemCommandHandler(
            unitOfWork,
            new FakeTaskItemCommandRepository(taskItem),
            new RecordingTaskActivityCommandRepository(),
            notifier,
            new FakeDateTimeProvider());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteTaskItemCommand(taskItem.Id), CancellationToken.None));

        Assert.Empty(notifier.Notifications);
    }

    private static void AssertSingleNotification(RecordingTaskActivityNotifier notifier, Guid expectedTaskListId)
    {
        var notification = Assert.Single(notifier.Notifications);
        Assert.Equal(expectedTaskListId, notification.TaskListId);
        Assert.True(notification.SaveCompleted);
        Assert.Equal(1, notification.ActivityCount);
        Assert.Equal(CancellationToken.None, notification.CancellationToken);
    }

    private static TaskList CreateTaskList()
    {
        var title = TaskListTitle.Create("Portfolio").Value;
        return TaskList.Create(title, UserId).Value;
    }

    private static TaskItem CreateTaskItem(Guid taskListId)
    {
        var title = TaskItemTitle.Create("Task").Value;
        return TaskItem.Create(taskListId, title, null, TaskPriority.Normal).Value;
    }

    private sealed class FakeUnitOfWork(bool throwOnSave = false, Action? afterSave = null) : IUnitOfWork
    {
        public bool SaveCompleted { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (throwOnSave)
            {
                throw new InvalidOperationException("Save failed.");
            }

            SaveCompleted = true;
            afterSave?.Invoke();
            return Task.FromResult(1);
        }
    }

    private sealed class FakeTaskListCommandRepository(TaskList? taskList = null) : ITaskListCommandRepository
    {
        public Task AddAsync(TaskList list, CancellationToken ct)
        {
            taskList = list;
            return Task.CompletedTask;
        }

        public void Remove(TaskList task)
        {
            taskList = null;
        }

        public Task<Result<TaskList>> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(taskList is not null && taskList.Id == id
                ? Result.Success(taskList)
                : Result.Fail<TaskList>($"Task list with id {id} not found"));
        }
    }

    private sealed class FakeTaskItemCommandRepository(TaskItem? taskItem = null) : ITaskItemCommandRepository
    {
        public Task AddAsync(TaskItem task, CancellationToken ct)
        {
            taskItem = task;
            return Task.CompletedTask;
        }

        public void Remove(TaskItem task)
        {
            taskItem = null;
        }

        public Task<Result<TaskItem>> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(taskItem is not null && taskItem.Id == id
                ? Result.Success(taskItem)
                : Result.Fail<TaskItem>($"Task item with id {id} not found"));
        }
    }

    private sealed class RecordingTaskActivityCommandRepository : ITaskActivityCommandRepository
    {
        public List<TaskActivity> Activities { get; } = [];

        public Task AddAsync(TaskActivity activity, CancellationToken ct)
        {
            Activities.Add(activity);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingTaskActivityNotifier(
        Func<bool> saveCompleted,
        Func<int>? activityCount = null) : ITaskActivityNotifier
    {
        public List<(Guid TaskListId, bool SaveCompleted, int ActivityCount, CancellationToken CancellationToken)> Notifications { get; } = [];

        public Task NotifyTaskListActivityChangedAsync(Guid taskListId, CancellationToken ct)
        {
            Notifications.Add((taskListId, saveCompleted(), activityCount?.Invoke() ?? 0, ct));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCurrentUserAccessor(Guid? userId) : ICurrentUserAccessor
    {
        public Guid? UserId => userId;
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow() => NowUtc;
    }
}
