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
    private static readonly Guid UserAId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserBId = new("22222222-2222-2222-2222-222222222222");
    private const string UserAUsername = "integration-user-a";
    private const string UserBUsername = "integration-user-b";

    [Fact]
    public async Task TaskLists_AreVisibleOnlyToTheirOwners()
    {
        await fixture.ResetAsync();

        var userA = await fixture.EnsureUserAsync(UserAId, UserAUsername);
        var userB = await fixture.EnsureUserAsync(UserBId, UserBUsername);

        using var userAClient = await fixture.CreateAuthorizedClientAsync(userA);
        using var userBClient = await fixture.CreateAuthorizedClientAsync(userB);

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

        var userA = await fixture.EnsureUserAsync(UserAId, UserAUsername);
        var userB = await fixture.EnsureUserAsync(UserBId, UserBUsername);

        using var userAClient = await fixture.CreateAuthorizedClientAsync(userA);
        using var userBClient = await fixture.CreateAuthorizedClientAsync(userB);

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

        var userA = await fixture.EnsureUserAsync(UserAId, UserAUsername);
        var userB = await fixture.EnsureUserAsync(UserBId, UserBUsername);

        using var userAClient = await fixture.CreateAuthorizedClientAsync(userA);
        using var userBClient = await fixture.CreateAuthorizedClientAsync(userB);

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

    [Fact]
    public async Task ForeignTaskById_Reads_ReturnNotFoundForOtherUsers()
    {
        await fixture.ResetAsync();

        var userA = await fixture.EnsureUserAsync(UserAId, UserAUsername);
        var userB = await fixture.EnsureUserAsync(UserBId, UserBUsername);

        using var userAClient = await fixture.CreateAuthorizedClientAsync(userA);
        using var userBClient = await fixture.CreateAuthorizedClientAsync(userB);

        var userAList = await CreateTaskListAsync(userAClient, "User A List");
        var userBList = await CreateTaskListAsync(userBClient, "User B List");

        var userATaskId = await CreateTaskItemAsync(userAClient, userAList.Id, "User A Task");
        var userBTaskId = await CreateTaskItemAsync(userBClient, userBList.Id, "User B Task");

        var userAReadingUserBTask = await userAClient.GetAsync($"/api/TaskItem/{userBTaskId}");
        var userBReadingUserATask = await userBClient.GetAsync($"/api/TaskItem/{userATaskId}");

        Assert.Equal(HttpStatusCode.NotFound, userAReadingUserBTask.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, userBReadingUserATask.StatusCode);
    }

    [Fact]
    public async Task PaginatedTaskItems_AreVisibleOnlyToTheirOwners()
    {
        await fixture.ResetAsync();

        var userA = await fixture.EnsureUserAsync(UserAId, UserAUsername);
        var userB = await fixture.EnsureUserAsync(UserBId, UserBUsername);

        using var userAClient = await fixture.CreateAuthorizedClientAsync(userA);
        using var userBClient = await fixture.CreateAuthorizedClientAsync(userB);

        var userAList = await CreateTaskListAsync(userAClient, "User A List");
        var userBList = await CreateTaskListAsync(userBClient, "User B List");

        var userATaskOneId = await CreateTaskItemAsync(userAClient, userAList.Id, "User A Task One");
        var userATaskTwoId = await CreateTaskItemAsync(userAClient, userAList.Id, "User A Task Two");
        var userBTaskId = await CreateTaskItemAsync(userBClient, userBList.Id, "User B Task");

        var userATasks = await userAClient.GetFromJsonAsync<PaginatedResponse<TaskItemLightResponse>>("/api/TaskItem?page=1&count=10");
        var userBTasks = await userBClient.GetFromJsonAsync<PaginatedResponse<TaskItemLightResponse>>("/api/TaskItem?page=1&count=10");

        Assert.NotNull(userATasks);
        Assert.NotNull(userBTasks);

        Assert.Equal(2, userATasks!.Items.Count);
        Assert.Contains(userATasks.Items, item => item.Id == userATaskOneId && item.Title == "User A Task One");
        Assert.Contains(userATasks.Items, item => item.Id == userATaskTwoId && item.Title == "User A Task Two");
        Assert.DoesNotContain(userATasks.Items, item => item.Id == userBTaskId);

        var visibleToUserB = Assert.Single(userBTasks!.Items);
        Assert.Equal(userBTaskId, visibleToUserB.Id);
        Assert.Equal("User B Task", visibleToUserB.Title);
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
