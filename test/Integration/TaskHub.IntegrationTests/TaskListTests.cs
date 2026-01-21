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
    public async Task Should_create_task_list()
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
        Assert.Equal(2, getAllResult.Value.Count);

        Assert.Contains(getAllResult.Value, list => list.Id == createListResult.Value.Id && list.Title == listTitle);
    }
}