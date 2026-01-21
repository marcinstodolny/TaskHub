using FluentAssertions;
using TaskHub.Application.Commands;
using TaskHub.Application.Queries;
using TaskHub.Domain.Enums;
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

        var createTaskItemResult = await fixture.SendAsync(new CreateTaskItemCommand(createListResult.Value.Id, "First task", "First task description", DateTime.Now, TaskPriority.High));
        
        createTaskItemResult.IsSuccess.Should().BeTrue();

        var secondCreateTaskItemResult = await fixture.SendAsync(new CreateTaskItemCommand(createListResult.Value.Id, "Second Task", "Second task description", DateTime.Now.AddDays(5), TaskPriority.Critical));

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
        Assert.Single(getAllResult.Value);

        Assert.Contains(getAllResult.Value, list => list.Id == createListResult.Value.Id && list.Title == listTitle);
    }

    [Fact]
    public async Task Invalid_Create_EmptyTitle_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var createListResult = await fixture.SendAsync(new CreateTaskListCommand(string.Empty));

        Assert.True(createListResult.IsFailed);

        var getAllResult = await fixture.SendAsync(new GetTaskListsQuery());

        Assert.True(getAllResult.IsSuccess);
        Assert.Empty(getAllResult.Value);
    }

    [Fact]
    public async Task Invalid_Get_MissingTaskList_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var missingId = Guid.NewGuid();
        var getResult = await fixture.SendAsync(new GetTaskListByIdQuery(missingId));

        Assert.True(getResult.IsFailed);
    }

    [Fact]
    public async Task Invalid_Create_TitleTooLong_ShouldReturnFailure()
    {
        await fixture.ResetAsync();

        var tooLongTitle = new string('a', 101);
        var createListResult = await fixture.SendAsync(new CreateTaskListCommand(tooLongTitle));

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
        Assert.Equal(2, getAllResult.Value.Count);
        Assert.Contains(getAllResult.Value, list => list.Id == firstResult.Value.Id);
        Assert.Contains(getAllResult.Value, list => list.Id == secondResult.Value.Id);
    }
}