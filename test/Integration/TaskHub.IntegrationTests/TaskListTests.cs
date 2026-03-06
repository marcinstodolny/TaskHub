using System.Net.Http.Json;
using FluentAssertions;
using TaskHub.Application.Features.TaskItem.Commands;
using TaskHub.Application.Features.TaskList.Commands;
using TaskHub.Application.Features.TaskList.Queries;
using TaskHub.Contracts.Common;
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
    public async Task GetAll_Api_ShouldIncludeTasksCount()
    {
        await fixture.ResetAsync();

        var firstList = await fixture.SendAsync(new CreateTaskListCommand("API List A"));
        var secondList = await fixture.SendAsync(new CreateTaskListCommand("API List B"));

        await fixture.SendAsync(new CreateTaskItemCommand(firstList.Value.Id, "Task A1", "Desc", TaskPriority.Normal));

        using var client = fixture.ApiFactory.CreateClient();
        var response = await client.GetAsync("/api/TaskList?page=1&count=10");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<TaskListLightResponse>>();

        Assert.NotNull(payload);
        Assert.NotNull(payload!.Items);
        Assert.Contains(payload.Items, item => item.Id == firstList.Value.Id && item.TasksCount >= 0);
        Assert.Contains(payload.Items, item => item.Id == secondList.Value.Id && item.TasksCount >= 0);
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