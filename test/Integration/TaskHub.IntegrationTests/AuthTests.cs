using System.Net;
using System.Net.Http.Json;
using TaskHub.Application.Features.TaskList.Commands;
using TaskHub.Contracts.Common;
using TaskHub.Contracts.TaskList;
using TaskHub.IntegrationTests.Infrastructure;
using Xunit;

namespace TaskHub.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class AuthTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GetTaskLists_WithoutToken_ShouldReturn401()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();

        var response = await client.GetAsync("/api/TaskList?page=1&count=10");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTaskLists_WithValidToken_ShouldReturn200()
    {
        await fixture.ResetAsync();

        await fixture.SendAsync(new CreateTaskListCommand("Secured list"));

        using var client = await fixture.CreateAuthorizedClientAsync();

        var response = await client.GetAsync("/api/TaskList?page=1&count=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<TaskListLightResponse>>();

        Assert.NotNull(payload);
        Assert.NotEmpty(payload!.Items);
    }
}
