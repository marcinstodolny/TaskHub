using FluentAssertions;
using TaskHub.Application.Commands;
using TaskHub.Application.Queries;
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

        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Title.Should().Be("My list");

        var getResult = await fixture.SendAsync(new GetTaskListByIdQuery(createResult.Value.Id));

        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Title.Should().Be("My list");
        getResult.Value.Id.Should().Be(createResult.Value.Id);

        var getAllResult = await fixture.SendAsync(new GetTaskListsQuery());

        getAllResult.IsSuccess.Should().BeTrue();
        getAllResult.Value.Should().ContainSingle(list =>
            list.Id == createResult.Value.Id &&
            list.Title == "My list");
    }
}