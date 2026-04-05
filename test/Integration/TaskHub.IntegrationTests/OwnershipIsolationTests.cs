using System.Net;
using System.Net.Http.Json;
using TaskHub.Contracts.Common;
using TaskHub.Contracts.TaskItem;
using TaskHub.Contracts.TaskList;
using TaskHub.Domain.Enums;
using TaskHub.IntegrationTests.Infrastructure;
using Xunit;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class OwnershipIsolationTests(IntegrationTestFixture fixture)
{
    private const string UserA = "integration-user-a";
    private const string UserB = "integration-user-b";

    [Fact]
    public async Task TaskLists_AreVisibleOnlyToTheirOwners()
    {
        await fixture.ResetAsync();

        using var userAClient = await fixture.CreateAuthorizedClientAsync(UserA);
        using var userBClient = await fixture.CreateAuthorizedClientAsync(UserB);

        var userAList = await CreateTaskListAsync(userAClient, "User A List");
        var userBList = await CreateTaskListAsync(userBClient, "User B List");

        var userATaskLists = await userAClient.GetFromJsonAsync<PaginatedResponse<TaskListLightResponse>>("/api/TaskList?page=1&count=10");
        var userBTaskLists = await userBClient.GetFromJsonAsync<PaginatedResponse<TaskListLightResponse>>("/api/TaskList?page=1&count=10");

        Assert.NotNull(userATaskLists);
        Assert.NotNull(userBTaskLists);

        var visibleToUserA = Assert.Single(userATaskLists!.Items);
        var visibleToUserB = Assert.Single(userBTaskLists!.Items);

        Assert.Equal(userAList.Id, visibleToUserA.Id);
        Assert.Equal("User A List", visibleToUserA.Title);

        Assert.Equal(userBList.Id, visibleToUserB.Id);
        Assert.Equal("User B List", visibleToUserB.Title);
    }

    [Fact]
    public async Task ForeignTaskList_ReadsAndMutations_ReturnNotFound()
    {
        await fixture.ResetAsync();

        using var userAClient = await fixture.CreateAuthorizedClientAsync(UserA);
        using var userBClient = await fixture.CreateAuthorizedClientAsync(UserB);

        var userBList = await CreateTaskListAsync(userBClient, "User B List");

        var getResponse = await userAClient.GetAsync($"/api/TaskList/{userBList.Id}");
        var updateResponse = await userAClient.PutAsJsonAsync(
            $"/api/TaskList/{userBList.Id}",
            new UpdateTaskListRequest("Updated by User A"));
        var deleteResponse = await userAClient.DeleteAsync($"/api/TaskList/{userBList.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task ForeignTaskBoardAndMutations_ReturnNotFound()
    {
        await fixture.ResetAsync();

        using var userAClient = await fixture.CreateAuthorizedClientAsync(UserA);
        using var userBClient = await fixture.CreateAuthorizedClientAsync(UserB);

        var userBList = await CreateTaskListAsync(userBClient, "User B List");
        var userBTaskId = await CreateTaskItemAsync(userBClient, userBList.Id, "User B Task");

        var boardResponse = await userAClient.GetAsync($"/api/TaskItem/board/{userBList.Id}");
        var createResponse = await userAClient.PostAsJsonAsync(
            "/api/TaskItem",
            new CreateTaskItemRequest(userBList.Id, "User A Foreign Task", "Blocked", TaskPriority.Normal));
        var updateResponse = await userAClient.PutAsJsonAsync(
            $"/api/TaskItem/{userBTaskId}",
            new UpdateTaskItemRequest("Updated by User A", "Blocked", TaskPriority.High));
        var deleteResponse = await userAClient.DeleteAsync($"/api/TaskItem/{userBTaskId}");
        var statusResponse = await userAClient.PostAsJsonAsync(
            $"/api/TaskItem/{userBTaskId}/status",
            new UpdateTaskStatusRequest(TaskStatus.Done));

        Assert.Equal(HttpStatusCode.NotFound, boardResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, statusResponse.StatusCode);
    }

    private static async Task<TaskListLightResponse> CreateTaskListAsync(HttpClient client, string title)
    {
        var response = await client.PostAsJsonAsync("/api/TaskList", new CreateTaskListRequest(title));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TaskListLightResponse>();
        Assert.NotNull(payload);
        return payload!;
    }

    private static async Task<Guid> CreateTaskItemAsync(HttpClient client, Guid taskListId, string title)
    {
        var response = await client.PostAsJsonAsync(
            "/api/TaskItem",
            new CreateTaskItemRequest(taskListId, title, "Owned task", TaskPriority.Normal));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}
