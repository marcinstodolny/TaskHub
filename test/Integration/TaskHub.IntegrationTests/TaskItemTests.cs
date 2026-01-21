using TaskHub.Application.Commands;
using TaskHub.Domain.Enums;
using TaskHub.IntegrationTests.Infrastructure;
using Xunit;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class TaskItemTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task Create_valid_ShouldSuccess()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for items"));

        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Task Item",
            "Description",
            DateTime.Now,
            TaskPriority.Normal));

        Assert.True(createItemResult.IsSuccess);
    }

    [Fact]
    public async Task Invalid_Create_MissingTaskList_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            Guid.NewGuid(),
            "Task Item",
            "Description",
            DateTime.Now,
            TaskPriority.Normal));

        Assert.True(createItemResult.IsFailed);
    }

    [Fact]
    public async Task Invalid_Create_EmptyTitle_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for empty title"));

        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            string.Empty,
            "Description",
            DateTime.Now,
            TaskPriority.Normal));

        Assert.True(createItemResult.IsFailed);
    }

    [Fact]
    public async Task Invalid_Create_TooLongDescription_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for long description"));
        var tooLongDescription = new string('a', 1001);

        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Task Item",
            tooLongDescription,
            DateTime.Now,
            TaskPriority.Normal));

        Assert.True(createItemResult.IsFailed);
    }

    [Fact]
    public async Task Invalid_UpdateStatus_MissingTaskItem_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var updateResult = await fixture.SendAsync(new UpdateTaskItemStatusCommand(Guid.NewGuid(), TaskStatus.Done));

        Assert.True(updateResult.IsFailed);
    }

    [Fact]
    public async Task Invalid_UpdateStatus_UnsupportedStatus_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for status"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Task Item",
            "Description",
            DateTime.Now,
            TaskPriority.Normal));

        var updateResult = await fixture.SendAsync(new UpdateTaskItemStatusCommand(createItemResult.Value, TaskStatus.Todo));

        Assert.True(updateResult.IsFailed);
    }
}
