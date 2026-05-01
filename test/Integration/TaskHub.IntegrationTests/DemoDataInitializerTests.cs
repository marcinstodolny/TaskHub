using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TaskHub.Application.Abstractions;
using TaskHub.Domain.Entities;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;
using TaskHub.Infrastructure.Authentication.Bootstrap;
using TaskHub.Infrastructure.Authentication.Options;
using TaskHub.Infrastructure.Persistence;
using TaskHub.IntegrationTests.Infrastructure;
using Xunit;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class DemoDataInitializerTests(IntegrationTestFixture fixture)
{
    private static readonly string[] DemoListTitles =
    [
        "Product Launch",
        "Engineering Improvements",
        "Portfolio Demo",
    ];

    private static readonly string[] DemoTaskTitles =
    [
        "Prepare API contract polish",
        "Update README demo flow",
        "Add SignalR activity refresh",
        "Review validation behavior",
        "Write integration tests",
        "Prepare stakeholder walkthrough",
    ];

    [Fact]
    public async Task EnsureDemoDataAsync_ShouldSeedPortfolioDatasetAndBeIdempotent()
    {
        await fixture.ResetAsync();

        await RunDemoDataInitializerAsync();
        var firstSnapshot = await ReadDemoDataSnapshotAsync();

        await RunDemoDataInitializerAsync();
        var secondSnapshot = await ReadDemoDataSnapshotAsync();

        firstSnapshot.Should().Be(new DemoDataSnapshot(3, 6, 13));
        secondSnapshot.Should().Be(firstSnapshot);
        await AssertDemoTasksHaveUsefulStatusAndPrioritySpreadAsync();
    }

    [Fact]
    public async Task EnsureDemoDataAsync_WhenDemoAuthDisabled_ShouldNotSeedData()
    {
        await fixture.ResetAsync();

        await using var scope = fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var initializer = new DemoDataInitializer(
            dbContext,
            Options.Create(new DemoAuthOptions { Enabled = false }),
            dateTimeProvider,
            NullLogger<DemoDataInitializer>.Instance);

        await initializer.EnsureDemoDataAsync();

        var snapshot = await ReadDemoDataSnapshotAsync();
        snapshot.Should().Be(new DemoDataSnapshot(0, 0, 0));
    }

    [Fact]
    public async Task EnsureDemoDataAsync_WhenDemoListAndTaskAlreadyExist_ShouldNotDuplicateOrOverwriteThem()
    {
        await fixture.ResetAsync();
        await SeedExistingDemoListAndTaskAsync();

        await RunDemoDataInitializerAsync();

        await using var scope = fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();
        var demoUserId = await GetDemoUserIdAsync(scope.ServiceProvider);

        var productLaunchLists = await dbContext.TaskLists
            .AsNoTracking()
            .Where(x => x.UserId == demoUserId && x.Title.Value == "Product Launch")
            .ToListAsync();

        productLaunchLists.Should().HaveCount(1);
        var productLaunchListId = productLaunchLists.Single().Id;

        var matchingTasks = await dbContext.TaskItems
            .AsNoTracking()
            .Where(x => x.TaskListId == productLaunchListId && x.Title.Value == "Prepare API contract polish")
            .ToListAsync();

        matchingTasks.Should().HaveCount(1);
        matchingTasks.Single().Priority.Should().Be(TaskPriority.Low);
        matchingTasks.Single().Status.Should().Be(TaskStatus.Todo);
    }

    [Fact]
    public async Task EnsureDemoDataAsync_WhenDuplicateSeedTitlesExist_ShouldUseOneAnchorAndLeaveDuplicatesUntouched()
    {
        await fixture.ResetAsync();
        await SeedDuplicateDemoListAndTaskTitlesAsync();

        var act = RunDemoDataInitializerAsync;

        await act.Should().NotThrowAsync();

        await using var scope = fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();
        var demoUserId = await GetDemoUserIdAsync(scope.ServiceProvider);

        var productLaunchLists = await dbContext.TaskLists
            .AsNoTracking()
            .Where(x => x.UserId == demoUserId && x.Title.Value == "Product Launch")
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync();

        productLaunchLists.Should().HaveCount(2);

        var seedAnchorListId = productLaunchLists[0].Id;
        var untouchedDuplicateListId = productLaunchLists[1].Id;

        var duplicateSeedTasks = await dbContext.TaskItems
            .AsNoTracking()
            .Where(x => x.TaskListId == seedAnchorListId && x.Title.Value == "Prepare API contract polish")
            .ToListAsync();

        duplicateSeedTasks.Should().HaveCount(2);
        duplicateSeedTasks.Should().OnlyContain(x => x.Priority == TaskPriority.Low && x.Status == TaskStatus.Todo);

        var addedTaskInAnchor = await dbContext.TaskItems
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TaskListId == seedAnchorListId && x.Title.Value == "Update README demo flow");

        addedTaskInAnchor.Should().NotBeNull();

        var untouchedDuplicateListTasks = await dbContext.TaskItems
            .AsNoTracking()
            .Where(x => x.TaskListId == untouchedDuplicateListId)
            .ToListAsync();

        untouchedDuplicateListTasks.Should().ContainSingle();
        untouchedDuplicateListTasks.Single().Title.Value.Should().Be("Keep duplicate list untouched");

        var engineeringImprovementsExists = await dbContext.TaskLists
            .AsNoTracking()
            .AnyAsync(x => x.UserId == demoUserId && x.Title.Value == "Engineering Improvements");
        var portfolioDemoExists = await dbContext.TaskLists
            .AsNoTracking()
            .AnyAsync(x => x.UserId == demoUserId && x.Title.Value == "Portfolio Demo");

        engineeringImprovementsExists.Should().BeTrue();
        portfolioDemoExists.Should().BeTrue();
    }

    private async Task RunDemoDataInitializerAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DemoDataInitializer>();
        await initializer.EnsureDemoDataAsync();
    }

    private async Task<DemoDataSnapshot> ReadDemoDataSnapshotAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();
        var demoUserId = await GetDemoUserIdAsync(scope.ServiceProvider);

        var demoListIds = await dbContext.TaskLists
            .AsNoTracking()
            .Where(x => x.UserId == demoUserId && DemoListTitles.Contains(x.Title.Value))
            .Select(x => x.Id)
            .ToListAsync();

        var taskCount = await dbContext.TaskItems
            .AsNoTracking()
            .CountAsync(x => demoListIds.Contains(x.TaskListId) && DemoTaskTitles.Contains(x.Title.Value));

        var activityCount = await dbContext.TaskActivities
            .AsNoTracking()
            .CountAsync(x => demoListIds.Contains(x.TaskListId));

        return new DemoDataSnapshot(demoListIds.Count, taskCount, activityCount);
    }

    private async Task AssertDemoTasksHaveUsefulStatusAndPrioritySpreadAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();
        var demoUserId = await GetDemoUserIdAsync(scope.ServiceProvider);

        var demoListIds = await dbContext.TaskLists
            .AsNoTracking()
            .Where(x => x.UserId == demoUserId && DemoListTitles.Contains(x.Title.Value))
            .Select(x => x.Id)
            .ToListAsync();

        var tasks = await dbContext.TaskItems
            .AsNoTracking()
            .Where(x => demoListIds.Contains(x.TaskListId) && DemoTaskTitles.Contains(x.Title.Value))
            .ToListAsync();

        tasks.Select(x => x.Status).Should().Contain([TaskStatus.Todo, TaskStatus.InProgress, TaskStatus.Done]);
        tasks.Select(x => x.Priority).Should().Contain([TaskPriority.Low, TaskPriority.Normal, TaskPriority.High, TaskPriority.Critical]);
    }

    private async Task SeedExistingDemoListAndTaskAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();
        var demoUserId = await GetDemoUserIdAsync(scope.ServiceProvider);

        var listTitle = TaskListTitle.Create("Product Launch").Value;
        var taskList = TaskList.Create(listTitle, demoUserId).Value;
        var taskTitle = TaskItemTitle.Create("Prepare API contract polish").Value;
        var taskDescription = TaskDescription.Create("Existing local demo task.").Value;
        taskList.AddTask(taskTitle, taskDescription, TaskPriority.Low);

        await dbContext.TaskLists.AddAsync(taskList);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedDuplicateDemoListAndTaskTitlesAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();
        var demoUserId = await GetDemoUserIdAsync(scope.ServiceProvider);

        var firstList = TaskList.Create(TaskListTitle.Create("Product Launch").Value, demoUserId).Value;
        firstList.AddTask(
            TaskItemTitle.Create("Prepare API contract polish").Value,
            TaskDescription.Create("Existing duplicate seed task A.").Value,
            TaskPriority.Low);
        firstList.AddTask(
            TaskItemTitle.Create("Prepare API contract polish").Value,
            TaskDescription.Create("Existing duplicate seed task B.").Value,
            TaskPriority.Low);

        var secondList = TaskList.Create(TaskListTitle.Create("Product Launch").Value, demoUserId).Value;
        secondList.AddTask(
            TaskItemTitle.Create("Keep duplicate list untouched").Value,
            TaskDescription.Create("This user-created duplicate list should not be modified.").Value,
            TaskPriority.Critical);

        await dbContext.TaskLists.AddRangeAsync(firstList, secondList);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<Guid> GetDemoUserIdAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<TaskHubDbContext>();
        var options = serviceProvider.GetRequiredService<IOptions<DemoAuthOptions>>().Value;

        var user = await dbContext.Users.SingleAsync(x => x.Id == options.UserId || x.Username == options.Username);
        return user.Id;
    }

    private sealed record DemoDataSnapshot(int TaskListCount, int TaskCount, int ActivityCount);
}
