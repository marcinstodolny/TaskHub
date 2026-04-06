using System.Net.Http.Json;
using FluentAssertions;
using System.Net;
using TaskHub.Application.Features.TaskItem.Commands;
using TaskHub.Application.Features.TaskList.Commands;
using TaskHub.Application.Features.TaskList.Queries;
using TaskHub.Contracts.Common;
using TaskHub.Contracts.TaskItem;
using TaskHub.Contracts.TaskList;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;
using TaskHub.IntegrationTests.Infrastructure;
using Xunit;

namespace TaskHub.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class TaskListTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task Create_valid_ShouldSuccess()
    {
        await fixture.ResetAsync();

        const string listTitle = "My Task List";
        var createListResult = await fixture.SendAsync(new CreateTaskListCommand(listTitle));

        var createTaskItemResult = await fixture.SendAsync(new CreateTaskItemCommand(createListResult.Value.Id, "First task", "First task description", TaskPriority.High));
        
        createTaskItemResult.IsSuccess.Should().BeTrue();

        var secondCreateTaskItemResult = await fixture.SendAsync(new CreateTaskItemCommand(createListResult.Value.Id, "Second Task", "Second task description", TaskPriority.Critical));

        var getResult = await fixture.SendAsync(new GetTaskListByIdQuery(createListResult.Value.Id));

        var getAllResult = await fixture.SendAsync(new GetTaskListsQuery());

        Assert.True(createListResult.IsSuccess);
        Assert.Equal(listTitle, createListResult.Value.Title);

        Assert.True(createTaskItemResult.IsSuccess);
        Assert.True(secondCreateTaskItemResult.IsSuccess);

        Assert.True(getResult.IsSuccess);
        Assert.Equal(getResult.Value.Id, createListResult.Value.Id);
        Assert.Equal(listTitle, getResult.Value.Title);
        Assert.Equal(2, getResult.Value.Tasks.Count);

        Assert.True(getAllResult.IsSuccess);
        Assert.Single(getAllResult.Value.Items);

        Assert.Contains(getAllResult.Value.Items, list => list.Id == createListResult.Value.Id && list.Title == listTitle);
    }

    [Fact]
    public async Task GetById_WithExistingTasks_ShouldReturnChildTaskData()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand("List with tasks"));

        var firstTaskResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "First child task",
            "First child description",
            TaskPriority.High));
        var secondTaskResult = await fixture.SendAsync(new CreateTaskItemCommand(
            createListResult.Value.Id,
            "Second child task",
            "Second child description",
            TaskPriority.Normal));

        var getResult = await fixture.SendAsync(new GetTaskListByIdQuery(createListResult.Value.Id));

        Assert.True(firstTaskResult.IsSuccess);
        Assert.True(secondTaskResult.IsSuccess);
        Assert.True(getResult.IsSuccess);
        Assert.NotEmpty(getResult.Value.Tasks);
        Assert.Equal(2, getResult.Value.Tasks.Count);
        Assert.Contains(getResult.Value.Tasks, task =>
            task.Id == firstTaskResult.Value &&
            task.TaskListId == createListResult.Value.Id &&
            task.Title == "First child task" &&
            task.Description == "First child description" &&
            task.Priority == TaskPriority.High);
        Assert.Contains(getResult.Value.Tasks, task =>
            task.Id == secondTaskResult.Value &&
            task.TaskListId == createListResult.Value.Id &&
            task.Title == "Second child task" &&
            task.Description == "Second child description" &&
            task.Priority == TaskPriority.Normal);
    }


    [Fact]
    public async Task GetAll_Api_ShouldIncludeTasksCount()
    {
        await fixture.ResetAsync();

        using var client = await fixture.CreateAuthorizedClientAsync();

        var firstCreateResponse = await client.PostAsJsonAsync("/api/TaskList", new CreateTaskListRequest("API List A"));
        var secondCreateResponse = await client.PostAsJsonAsync("/api/TaskList", new CreateTaskListRequest("API List B"));

        firstCreateResponse.EnsureSuccessStatusCode();
        secondCreateResponse.EnsureSuccessStatusCode();

        var firstList = await firstCreateResponse.Content.ReadFromJsonAsync<TaskListLightResponse>();
        var secondList = await secondCreateResponse.Content.ReadFromJsonAsync<TaskListLightResponse>();

        Assert.NotNull(firstList);
        Assert.NotNull(secondList);

        var createTaskResponse = await client.PostAsJsonAsync(
            "/api/TaskItem",
            new CreateTaskItemRequest(firstList!.Id, "Task A1", "Desc", TaskPriority.Normal));

        createTaskResponse.EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/TaskList?page=1&count=10");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<TaskListLightResponse>>();

        Assert.NotNull(payload);
        Assert.NotNull(payload!.Items);

        var firstListPayload = Assert.Single(payload.Items, item => item.Id == firstList.Id);
        var secondListPayload = Assert.Single(payload.Items, item => item.Id == secondList.Id);

        Assert.True(firstListPayload.TasksCount >= 0);
        Assert.True(secondListPayload.TasksCount >= 0);
        Assert.Equal(1, firstListPayload.TasksCount);
        Assert.Equal(0, secondListPayload.TasksCount);
    }

    [Fact]
    public async Task Create_Invalid_InvalidTitle_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand(string.Empty));

        Assert.True(createListResult.IsFailed);

        var getAllResult = await fixture.SendAsync(new GetTaskListsQuery());

        Assert.True(getAllResult.IsSuccess);
        Assert.Empty(getAllResult.Value.Items);
    }

    [Fact]
    public async Task Invalid_Get_MissingTaskList_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var getResult = await fixture.SendAsync(new GetTaskListByIdQuery(Guid.NewGuid()));

        Assert.True(getResult.IsFailed);
    }

    [Theory]
    [MemberData(nameof(InvalidTitleData))]
    public async Task Create_Invalid_TitleTooLong_ShouldReturnFailure(string? invalidTitle)
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand(invalidTitle));

        Assert.True(createListResult.IsFailed);
    }

    [Fact]
    public async Task GetAll_WithMultipleLists_ShouldReturnAll()
    {
        await fixture.ResetAsync();

        var firstResult = await fixture.SendAsync(new CreateTaskListCommand("First List"));
        var secondResult = await fixture.SendAsync(new CreateTaskListCommand("Second List"));

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);

        var getAllResult = await fixture.SendAsync(new GetTaskListsQuery());

        Assert.True(getAllResult.IsSuccess);
        Assert.Equal(2, getAllResult.Value.Items.Count);
        Assert.Contains(getAllResult.Value.Items, list => list.Id == firstResult.Value.Id);
        Assert.Contains(getAllResult.Value.Items, list => list.Id == secondResult.Value.Id);
    }

    [Fact]
    public async Task Update_Valid_ShouldUpdateTitle()
    {
        await fixture.ResetAsync();

        var createResult = await fixture.SendAsync(new CreateTaskListCommand("Initial Title"));

        var updateResult = await fixture.SendAsync(new UpdateTaskListCommand(createResult.Value.Id, "Updated Title"));

        var getResult = await fixture.SendAsync(new GetTaskListByIdQuery(createResult.Value.Id));

        Assert.True(updateResult.IsSuccess);
        Assert.Equal("Updated Title", updateResult.Value.Title);
        Assert.True(getResult.IsSuccess);
        Assert.Equal("Updated Title", getResult.Value.Title);
    }

    [Theory]
    [MemberData(nameof(InvalidTitleData))]
    public async Task Update_InvalidTitle_ShouldReturnFailure(string? invalidTitle)
    {
        await fixture.ResetAsync();

        var createResult = await fixture.SendAsync(new CreateTaskListCommand("Initial Title"));

        var updateResult = await fixture.SendAsync(new UpdateTaskListCommand(createResult.Value.Id, invalidTitle));

        Assert.True(updateResult.IsFailed);
    }

    [Fact]
    public async Task Update_MissingTaskList_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var updateResult = await fixture.SendAsync(new UpdateTaskListCommand(Guid.NewGuid(), "Updated Title"));

        Assert.True(updateResult.IsFailed);
    }

    [Fact]
    public async Task Update_ForeignTaskList_Endpoint_ShouldReturnNotFound()
    {
        await fixture.ResetAsync();

        var createResult = await fixture.SendAsync(new CreateTaskListCommand("Foreign title"));

        using var client = await fixture.CreateAuthorizedClientAsync();
        var response = await client.PutAsJsonAsync($"/api/TaskList/{createResult.Value.Id}", new UpdateTaskListRequest("Updated Title"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_EmptyTaskListId_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var updateResult = await fixture.SendAsync(new UpdateTaskListCommand(Guid.Empty, "Updated Title"));

        Assert.True(updateResult.IsFailed);
    }

    [Fact]
    public async Task Delete_MissingTaskList_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var deleteResult = await fixture.SendAsync(new DeleteTaskListCommand(Guid.NewGuid()));

        Assert.True(deleteResult.IsFailed);
    }

    [Fact]
    public async Task Delete_ForeignTaskList_Endpoint_ShouldReturnNotFound()
    {
        await fixture.ResetAsync();

        var createResult = await fixture.SendAsync(new CreateTaskListCommand("Foreign delete"));

        using var client = await fixture.CreateAuthorizedClientAsync();
        var response = await client.DeleteAsync($"/api/TaskList/{createResult.Value.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingTaskList_ShouldRemoveAndNotFoundOnGet()
    {
        await fixture.ResetAsync();

        var createResult = await fixture.SendAsync(new CreateTaskListCommand("To Delete"));

        var deleteResult = await fixture.SendAsync(new DeleteTaskListCommand(createResult.Value.Id));

        var getResult = await fixture.SendAsync(new GetTaskListByIdQuery(createResult.Value.Id));

        Assert.True(deleteResult.IsSuccess);
        Assert.True(getResult.IsFailed);
    }

    public static IEnumerable<object[]?> InvalidTitleData =>
        new List<object[]?>
        {
            new object[] { new string('a', TaskListTitle.MaxLength + 1) },
            new object[] { string.Empty },
            new object[] { null },
        };
}
