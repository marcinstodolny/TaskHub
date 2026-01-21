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

        var createResult = await fixture.SendAsync(new CreateTaskListCommand("My list"));

        var createTaskItemResult = await fixture.SendAsync(new CreateTaskItemCommand(createResult.Value.Id, "My task", "description", DateTime.Now, TaskPriority.High));
        
        createTaskItemResult.IsSuccess.Should().BeTrue();

        var secondCreateTaskItemResult = await fixture.SendAsync(new CreateTaskItemCommand(createResult.Value.Id, "Second Task", "Second description", DateTime.Now.AddDays(5), TaskPriority.Critical));

        var getResult = await fixture.SendAsync(new GetTaskListByIdQuery(createResult.Value.Id));

        var getAllResult = await fixture.SendAsync(new GetTaskListsQuery());

        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Title.Should().Be("My list");

        createTaskItemResult.IsSuccess.Should().BeTrue();
        secondCreateTaskItemResult.IsSuccess.Should().BeTrue();

        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Title.Should().Be("My list");
        getResult.Value.Id.Should().Be(createResult.Value.Id);
        getResult.Value.Tasks.Should().HaveCount(2);

        getAllResult.IsSuccess.Should().BeTrue();
        getAllResult.Value.Should().ContainSingle(list =>
            list.Id == createResult.Value.Id &&
            list.Title == "My list");
    }
}