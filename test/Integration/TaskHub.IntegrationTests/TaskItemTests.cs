using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaskHub.Contracts.TaskItem;
using TaskHub.Application.Features.TaskItem.Commands;
using TaskHub.Application.Features.TaskItem.Queries;
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
    public async Task UpdateStatus_Invalid_SameStatus_ShouldReturnFailure()
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

    [Fact]
    public async Task GetById_Valid_ShouldReturnTaskItem()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for get item"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Task Item",
            "Description",
            TaskPriority.Normal));

        using var client = await fixture.CreateAuthorizedClientAsync();

        var response = await fixture.SendAsync(new GetTaskItemByIdQuery(createItemResult.Value));

        Assert.True(response.IsSuccess);
        Assert.Equal(createItemResult.Value, response.Value.Id);
        Assert.Equal(createListResult.Value.Id, response.Value.TaskListId);
        Assert.Equal("Task Item", response.Value.Title);
    }

    [Fact]
    public async Task GetAll_WithMultipleItems_ShouldReturnAll()
    {
        await fixture.ResetAsync();

        var firstListResult = await fixture.SendAsync(new CreateTaskListCommand("List A"));
        var secondListResult = await fixture.SendAsync(new CreateTaskListCommand("List B"));

        var firstItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            firstListResult.Value.Id,
            "Task Item A",
            "Description A",
            TaskPriority.Normal));

        var secondItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            secondListResult.Value.Id,
            "Task Item B",
            "Description B",
            TaskPriority.High));

        var response = await fixture.SendAsync(new GetTaskItemsQuery());

        Assert.True(response.IsSuccess);
        Assert.Equal(2, response.Value.Items.Count);
        Assert.Contains(response.Value.Items, list => list.Id == firstItemResult.Value);
        Assert.Contains(response.Value.Items, list => list.Id == secondItemResult.Value);
    }


    [Fact]
    public async Task UpdateStatus_Endpoint_DisallowedTransition_ShouldReturnProblemDetails()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for invalid transition"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Task with invalid transition",
            "Description",
            TaskPriority.Normal));

        using var client = await fixture.CreateAuthorizedClientAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/TaskItem/{createItemResult.Value}/status",
            new { Status = TaskStatus.Todo });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
        Assert.True(problem.Extensions.TryGetValue("errors", out var errorsObject));

        var errors = Assert.IsType<JsonElement>(errorsObject);
        Assert.Equal(JsonValueKind.Array, errors.ValueKind);
        Assert.NotEmpty(errors.EnumerateArray());
    }

    [Fact]
    public async Task UpdateStatus_Endpoint_MissingTask_ShouldReturnNotFoundProblemDetails()
    {
        await fixture.ResetAsync();

        using var client = await fixture.CreateAuthorizedClientAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/TaskItem/{Guid.NewGuid()}/status",
            new { Status = TaskStatus.Done });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Unable to update task status", problem.Title);
        Assert.True(problem.Extensions.TryGetValue("errors", out var errorsObject));

        var errors = Assert.IsType<JsonElement>(errorsObject);
        Assert.Equal(JsonValueKind.Array, errors.ValueKind);
        Assert.Contains(errors.EnumerateArray(), element =>
            element.ValueKind == JsonValueKind.String &&
            element.GetString() is not null &&
            element.GetString()!.Contains("not found", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task UpdateStatus_Endpoint_ReopenDoneTask_ShouldSucceed()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for reopen"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Task for reopen",
            "Description",
            TaskPriority.Normal));

        using var client = await fixture.CreateAuthorizedClientAsync();

        var moveToDoneResponse = await client.PostAsJsonAsync(
            $"/api/TaskItem/{createItemResult.Value}/status",
            new { Status = TaskStatus.Done });

        Assert.Equal(HttpStatusCode.OK, moveToDoneResponse.StatusCode);

        var reopenResponse = await client.PostAsJsonAsync(
            $"/api/TaskItem/{createItemResult.Value}/status",
            new { Status = TaskStatus.InProgress });

        Assert.Equal(HttpStatusCode.OK, reopenResponse.StatusCode);

        var getResponse = await client.GetFromJsonAsync<TaskCardResponse>($"/api/TaskItem/{createItemResult.Value}");

        Assert.NotNull(getResponse);
        Assert.Equal(TaskStatus.InProgress, getResponse.Status);
    }

    [Fact]
    public async Task UpdateStatus_Endpoint_ShouldChangeStatusEndToEnd()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List for endpoint status"));
        var createItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Task for status update",
            "Description",
            TaskPriority.Normal));

        using var client = await fixture.CreateAuthorizedClientAsync();

        var updateResponse = await client.PostAsJsonAsync(
            $"/api/TaskItem/{createItemResult.Value}/status",
            new { Status = TaskStatus.InProgress });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var getResponse = await client.GetFromJsonAsync<TaskCardResponse>($"/api/TaskItem/{createItemResult.Value}");

        Assert.NotNull(getResponse);
        Assert.Equal(TaskStatus.InProgress, getResponse.Status);
    }

    [Fact]
    public async Task GetTaskItemsBoard_Endpoint_ShouldReturnTasksForSelectedList()
    {
        await fixture.ResetAsync();

        var selectedListResult = await fixture.SendAsync(new CreateTaskListCommand("Selected list"));
        var otherListResult = await fixture.SendAsync(new CreateTaskListCommand("Other list"));

        var selectedItemResult = await fixture.SendAsync(new CreateTaskItemCommand(
            selectedListResult.Value.Id,
            "Selected task",
            "Selected description",
            TaskPriority.High));

        await fixture.SendAsync(new CreateTaskItemCommand(
            otherListResult.Value.Id,
            "Other task",
            "Other description",
            TaskPriority.Low));

        using var client = await fixture.CreateAuthorizedClientAsync();
        var boardItems = await client.GetFromJsonAsync<List<TaskCardResponse>>($"/api/TaskItem/board/{selectedListResult.Value.Id}");

        Assert.NotNull(boardItems);
        Assert.Single(boardItems);
        Assert.Equal(selectedItemResult.Value, boardItems[0].Id);
        Assert.Equal("Selected task", boardItems[0].Title);
        Assert.Equal(TaskStatus.Todo, boardItems[0].Status);
        Assert.Equal([TaskStatus.InProgress, TaskStatus.Done, TaskStatus.Cancelled], boardItems[0].AllowedTargetStatuses);
    }

}
