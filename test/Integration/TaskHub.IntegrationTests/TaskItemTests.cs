using TaskHub.Application.Features.TaskItem.Commands;
using TaskHub.Application.Features.TaskList.Commands;
using TaskHub.Application.Features.TaskList.Queries;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;
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
            TaskPriority.Normal));

        Assert.True(createItemResult.IsSuccess);
    }

    [Fact]
    public async Task Create_Invalid_MissingTaskList_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            Guid.NewGuid(),
            "Task Item",
            "Description",
            TaskPriority.Normal));

        Assert.True(createItemResult.IsFailed);
    }

    [Fact]
    public async Task Create_Invalid_EmptyTitle_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for empty title"));

        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            string.Empty,
            "Description",
            TaskPriority.Normal));

        Assert.True(createItemResult.IsFailed);
    }

    [Fact]
    public async Task Create_Invalid_TooLongDescription_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for long description"));
        var tooLongDescription = new string('a', TaskDescription.MaxLength + 1);

        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Task Item",
            tooLongDescription,
            TaskPriority.Normal));

        Assert.True(createItemResult.IsFailed);
    }

    [Fact]
    public async Task UpdateStatus_Invalid_MissingTaskItem_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var updateResult = await fixture.SendAsync(new UpdateTaskItemStatusCommand(Guid.NewGuid(), TaskStatus.Done));

        Assert.True(updateResult.IsFailed);
    }

    [Fact]
    public async Task UpdateStatus_Invalid_UnsupportedStatus_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for status"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Task Item",
            "Description",
            TaskPriority.Normal));

        var updateResult = await fixture.SendAsync(new UpdateTaskItemStatusCommand(createItemResult.Value, TaskStatus.Todo));

        Assert.True(updateResult.IsFailed);
    }

    [Fact]
    public async Task Update_valid_ShouldUpdateDetails()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for update"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Original title",
            "Original description",
            TaskPriority.Normal));

        var updateResult = await fixture.SendAsync(new UpdateTaskItemCommand(
            createItemResult.Value,
            "Updated title",
            "Updated description",
            TaskPriority.High));

        Assert.True(updateResult.IsSuccess);
        Assert.Equal("Updated title", updateResult.Value.Title);
        Assert.Equal("Updated description", updateResult.Value.Description);
        Assert.Equal(TaskPriority.High, updateResult.Value.Priority);

        var getListResult = await fixture.SendAsync(new GetTaskListByIdQuery(createListResult.Value.Id));
        Assert.True(getListResult.IsSuccess);
        Assert.Contains(getListResult.Value.Tasks, task =>
            task.Id == createItemResult.Value &&
            task is { Title: "Updated title", Description: "Updated description", Priority: TaskPriority.High });
    }

    [Fact]
    public async Task Update_Invalid_MissingTaskItem_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var updateResult = await fixture.SendAsync(new UpdateTaskItemCommand(
            Guid.NewGuid(),
            "Updated title",
            "Updated description",
            TaskPriority.Normal));

        Assert.True(updateResult.IsFailed);
    }

    [Fact]
    public async Task Update_Invalid_TitleTooLong_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for update"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Original title",
            "Original description",
            TaskPriority.Normal));

        var tooLongTitle = new string('a', TaskItemTitle.MaxLength + 1);
        var updateResult = await fixture.SendAsync(new UpdateTaskItemCommand(
            createItemResult.Value,
            tooLongTitle,
            "Updated description",
            TaskPriority.Normal));

        Assert.True(updateResult.IsFailed);
    }

    [Fact]
    public async Task Update_Invalid_DescriptionTooLong_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for update"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Original title",
            "Original description",
            TaskPriority.Normal));

        var tooLongDescription = new string('a', TaskDescription.MaxLength + 1);
        var updateResult = await fixture.SendAsync(new UpdateTaskItemCommand(
            createItemResult.Value,
            "Updated title",
            tooLongDescription,
            TaskPriority.Normal));

        Assert.True(updateResult.IsFailed);
    }

    [Fact]
    public async Task Delete_Invalid_MissingTaskItem_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var deleteResult = await fixture.SendAsync(new DeleteTaskItemCommand(Guid.NewGuid()));

        Assert.True(deleteResult.IsFailed);
    }

    [Fact]
    public async Task Delete_valid_ShouldRemoveTaskItem()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for delete"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Task Item",
            "Description",
            TaskPriority.Normal));

        var deleteResult = await fixture.SendAsync(new DeleteTaskItemCommand(createItemResult.Value));

        Assert.True(deleteResult.IsSuccess);

        var getListResult = await fixture.SendAsync(new GetTaskListByIdQuery(createListResult.Value.Id));
        Assert.True(getListResult.IsSuccess);
        Assert.DoesNotContain(getListResult.Value.Tasks, task => task.Id == createItemResult.Value);
    }
}
