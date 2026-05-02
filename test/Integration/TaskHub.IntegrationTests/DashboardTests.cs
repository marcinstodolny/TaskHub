using System.Net;
using System.Net.Http.Json;
using TaskHub.Contracts.Dashboard;
using TaskHub.Contracts.TaskItem;
using TaskHub.Contracts.TaskList;
using TaskHub.Domain.Enums;
using TaskHub.IntegrationTests.Infrastructure;
using Xunit;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class DashboardTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GetDashboardSummary_WithoutToken_ShouldReturn401()
    {
        await fixture.ResetAsync();

        using var client = fixture.ApiFactory.CreateClient();
        var response = await client.GetAsync("/api/Dashboard/summary");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboardSummary_WithNoUserData_ShouldReturnZeroCounts()
    {
        await fixture.ResetAsync();

        var user = await fixture.EnsureUserAsync(new Guid("55555555-5555-5555-5555-555555555555"), "dashboard-empty-user");
        using var client = await fixture.CreateAuthorizedClientAsync(user);

        var summary = await GetDashboardSummaryAsync(client);

        Assert.Equal(0, summary.TaskListCount);
        Assert.Equal(0, summary.TotalTaskCount);
        Assert.Equal(0, summary.TodoCount);
        Assert.Equal(0, summary.InProgressCount);
        Assert.Equal(0, summary.DoneCount);
        Assert.Equal(0, summary.CancelledCount);
        Assert.Empty(summary.RecentActivities);
    }

    [Fact]
    public async Task GetDashboardSummary_WithValidToken_ShouldReturnCurrentUserCountsAndRecentActivities()
    {
        await fixture.ResetAsync();

        using var client = await fixture.CreateAuthorizedClientAsync();
        var firstList = await CreateTaskListAsync(client, "Dashboard API work");
        var secondList = await CreateTaskListAsync(client, "Dashboard UI work");

        await CreateTaskItemAsync(client, firstList.Id, "Keep todo task");
        var inProgressTaskId = await CreateTaskItemAsync(client, firstList.Id, "Move in progress task");
        var doneTaskId = await CreateTaskItemAsync(client, secondList.Id, "Move done task");
        var cancelledTaskId = await CreateTaskItemAsync(client, secondList.Id, "Move cancelled task");

        await UpdateTaskStatusAsync(client, inProgressTaskId, TaskStatus.InProgress);
        await UpdateTaskStatusAsync(client, doneTaskId, TaskStatus.Done);
        await UpdateTaskStatusAsync(client, cancelledTaskId, TaskStatus.Cancelled);

        var summary = await GetDashboardSummaryAsync(client);

        Assert.Equal(2, summary.TaskListCount);
        Assert.Equal(4, summary.TotalTaskCount);
        Assert.Equal(1, summary.TodoCount);
        Assert.Equal(1, summary.InProgressCount);
        Assert.Equal(1, summary.DoneCount);
        Assert.Equal(1, summary.CancelledCount);
        Assert.Equal(5, summary.RecentActivities.Count);
        Assert.All(summary.RecentActivities, activity =>
            Assert.Contains(activity.TaskListId, new[] { firstList.Id, secondList.Id }));
    }

    [Fact]
    public async Task GetDashboardSummary_ShouldExcludeForeignUserData()
    {
        await fixture.ResetAsync();

        var userA = await fixture.EnsureUserAsync(new Guid("66666666-6666-6666-6666-666666666666"), "dashboard-user-a");
        var userB = await fixture.EnsureUserAsync(new Guid("77777777-7777-7777-7777-777777777777"), "dashboard-user-b");

        using var userAClient = await fixture.CreateAuthorizedClientAsync(userA);
        using var userBClient = await fixture.CreateAuthorizedClientAsync(userB);

        var userAList = await CreateTaskListAsync(userAClient, "User A dashboard list");
        await CreateTaskItemAsync(userAClient, userAList.Id, "User A task");

        var userBList = await CreateTaskListAsync(userBClient, "User B dashboard list");
        var userBTaskId = await CreateTaskItemAsync(userBClient, userBList.Id, "User B task");
        await UpdateTaskStatusAsync(userBClient, userBTaskId, TaskStatus.Done);

        var userASummary = await GetDashboardSummaryAsync(userAClient);

        Assert.Equal(1, userASummary.TaskListCount);
        Assert.Equal(1, userASummary.TotalTaskCount);
        Assert.Equal(1, userASummary.TodoCount);
        Assert.Equal(0, userASummary.DoneCount);
        Assert.All(userASummary.RecentActivities, activity => Assert.Equal(userAList.Id, activity.TaskListId));
    }

    private static async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/Dashboard/summary");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<DashboardSummaryResponse>();
        Assert.NotNull(payload);
        return payload!;
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
            new CreateTaskItemRequest(taskListId, title, "Dashboard summary test task", TaskPriority.Normal));
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task UpdateTaskStatusAsync(HttpClient client, Guid taskId, TaskStatus status)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/TaskItem/{taskId}/status",
            new UpdateTaskStatusRequest(status));
        response.EnsureSuccessStatusCode();
    }
}
