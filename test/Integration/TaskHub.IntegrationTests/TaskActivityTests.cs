using System.Net;
using System.Net.Http.Json;
using TaskHub.Contracts.Common;
using TaskHub.Contracts.TaskActivity;
using TaskHub.Contracts.TaskItem;
using TaskHub.Contracts.TaskList;
using TaskHub.Domain.Enums;
using TaskHub.IntegrationTests.Infrastructure;
using Xunit;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class TaskActivityTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task CreateTaskItem_ShouldRecordActivity()
    {
        await fixture.ResetAsync();

        using var client = await fixture.CreateAuthorizedClientAsync();
        var taskList = await CreateTaskListAsync(client, "Activity create list");

        var taskId = await CreateTaskItemAsync(client, taskList.Id, "Fix login validation");

        var feed = await GetActivityFeedAsync(client, taskList.Id);
        Assert.Contains(feed.Items, activity =>
            activity.TaskListId == taskList.Id
            && activity.TaskItemId == taskId
            && activity.Type == TaskActivityType.TaskCreated
            && activity.Message == "Created task \"Fix login validation\""
            && activity.TaskTitleSnapshot == "Fix login validation");
    }

    [Fact]
    public async Task UpdateTaskItem_ShouldRecordActivity()
    {
        await fixture.ResetAsync();

        using var client = await fixture.CreateAuthorizedClientAsync();
        var taskList = await CreateTaskListAsync(client, "Activity update list");
        var taskId = await CreateTaskItemAsync(client, taskList.Id, "Original task");

        var response = await client.PutAsJsonAsync(
            $"/api/TaskItem/{taskId}",
            new UpdateTaskItemRequest("Updated task", "Updated description", TaskPriority.High));
        response.EnsureSuccessStatusCode();

        var feed = await GetActivityFeedAsync(client, taskList.Id);
        Assert.Contains(feed.Items, activity =>
            activity.TaskItemId == taskId
            && activity.Type == TaskActivityType.TaskUpdated
            && activity.Message == "Updated task \"Updated task\""
            && activity.TaskTitleSnapshot == "Updated task");
    }

    [Fact]
    public async Task UpdateTaskStatus_ShouldRecordActivity()
    {
        await fixture.ResetAsync();

        using var client = await fixture.CreateAuthorizedClientAsync();
        var taskList = await CreateTaskListAsync(client, "Activity status list");
        var taskId = await CreateTaskItemAsync(client, taskList.Id, "Move status task");

        var response = await client.PostAsJsonAsync(
            $"/api/TaskItem/{taskId}/status",
            new UpdateTaskStatusRequest(TaskStatus.Done));
        response.EnsureSuccessStatusCode();

        var feed = await GetActivityFeedAsync(client, taskList.Id);
        Assert.Contains(feed.Items, activity =>
            activity.TaskItemId == taskId
            && activity.Type == TaskActivityType.TaskStatusChanged
            && activity.Message == "Moved task \"Move status task\" from Todo to Done"
            && activity.TaskTitleSnapshot == "Move status task");
    }

    [Fact]
    public async Task DeleteTaskItem_ShouldRecordActivityWithReadableTitleSnapshot()
    {
        await fixture.ResetAsync();

        using var client = await fixture.CreateAuthorizedClientAsync();
        var taskList = await CreateTaskListAsync(client, "Activity delete list");
        var taskId = await CreateTaskItemAsync(client, taskList.Id, "Delete snapshot task");

        var response = await client.DeleteAsync($"/api/TaskItem/{taskId}");
        response.EnsureSuccessStatusCode();

        var feed = await GetActivityFeedAsync(client, taskList.Id);
        Assert.Contains(feed.Items, activity =>
            activity.TaskItemId == taskId
            && activity.Type == TaskActivityType.TaskDeleted
            && activity.Message == "Deleted task \"Delete snapshot task\""
            && activity.TaskTitleSnapshot == "Delete snapshot task");
    }

    [Fact]
    public async Task GetTaskListActivities_ShouldReturnNewestEntriesFirst()
    {
        await fixture.ResetAsync();

        using var client = await fixture.CreateAuthorizedClientAsync();
        var taskList = await CreateTaskListAsync(client, "Activity ordering list");
        await CreateTaskItemAsync(client, taskList.Id, "Older task");
        await Task.Delay(20);
        await CreateTaskItemAsync(client, taskList.Id, "Newer task");

        var feed = await GetActivityFeedAsync(client, taskList.Id);

        var newerIndex = feed.Items.ToList().FindIndex(activity => activity.Message == "Created task \"Newer task\"");
        var olderIndex = feed.Items.ToList().FindIndex(activity => activity.Message == "Created task \"Older task\"");

        Assert.True(newerIndex >= 0);
        Assert.True(olderIndex >= 0);
        Assert.True(newerIndex < olderIndex);
    }

    [Fact]
    public async Task GetTaskListActivities_ShouldBeIsolatedPerUser()
    {
        await fixture.ResetAsync();

        var userA = await fixture.EnsureUserAsync(new Guid("33333333-3333-3333-3333-333333333333"), "activity-user-a");
        var userB = await fixture.EnsureUserAsync(new Guid("44444444-4444-4444-4444-444444444444"), "activity-user-b");

        using var userAClient = await fixture.CreateAuthorizedClientAsync(userA);
        using var userBClient = await fixture.CreateAuthorizedClientAsync(userB);

        var userAList = await CreateTaskListAsync(userAClient, "User A activity list");
        var userBList = await CreateTaskListAsync(userBClient, "User B activity list");

        await CreateTaskItemAsync(userAClient, userAList.Id, "User A task");
        await CreateTaskItemAsync(userBClient, userBList.Id, "User B task");

        var userAFeed = await GetActivityFeedAsync(userAClient, userAList.Id);
        var userBFeed = await GetActivityFeedAsync(userBClient, userBList.Id);
        var userAReadingUserBFeed = await userAClient.GetAsync($"/api/TaskList/{userBList.Id}/activities?page=1&count=50");

        Assert.Contains(userAFeed.Items, activity => activity.Message == "Created task \"User A task\"");
        Assert.DoesNotContain(userAFeed.Items, activity => activity.Message.Contains("User B", StringComparison.Ordinal));

        Assert.Contains(userBFeed.Items, activity => activity.Message == "Created task \"User B task\"");
        Assert.DoesNotContain(userBFeed.Items, activity => activity.Message.Contains("User A", StringComparison.Ordinal));

        Assert.Equal(HttpStatusCode.NotFound, userAReadingUserBFeed.StatusCode);
    }

    [Fact]
    public async Task GetTaskListActivities_Unauthenticated_ShouldBeRejected()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();
        var response = await client.GetAsync($"/api/TaskList/{Guid.NewGuid()}/activities?page=1&count=50");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
            new CreateTaskItemRequest(taskListId, title, "Activity test task", TaskPriority.Normal));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<PaginatedResponse<TaskActivityResponse>> GetActivityFeedAsync(HttpClient client, Guid taskListId)
    {
        var response = await client.GetAsync($"/api/TaskList/{taskListId}/activities?page=1&count=50");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<TaskActivityResponse>>();
        Assert.NotNull(payload);
        return payload!;
    }
}
