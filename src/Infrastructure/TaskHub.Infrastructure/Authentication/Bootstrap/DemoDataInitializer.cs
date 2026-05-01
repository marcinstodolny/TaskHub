using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Features.Auth;
using TaskHub.Domain.Entities;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;
using TaskHub.Infrastructure.Authentication.Options;
using TaskHub.Infrastructure.Persistence;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Infrastructure.Authentication.Bootstrap;

public sealed class DemoDataInitializer(
    TaskHubDbContext dbContext,
    IOptions<DemoAuthOptions> demoAuthOptions,
    IDateTimeProvider dateTimeProvider,
    ILogger<DemoDataInitializer> logger)
{
    private static readonly DemoTaskListSeed[] DemoTaskLists =
    [
        new(
            "Product Launch",
            [
                new(
                    "Prepare API contract polish",
                    "Review response shapes and error details before the portfolio walkthrough.",
                    TaskPriority.High,
                    TaskStatus.InProgress),
                new(
                    "Update README demo flow",
                    "Document the primary demo path and setup expectations.",
                    TaskPriority.Normal,
                    TaskStatus.Todo),
            ]),
        new(
            "Engineering Improvements",
            [
                new(
                    "Add SignalR activity refresh",
                    "Keep activity feeds current when task cards change.",
                    TaskPriority.High,
                    TaskStatus.Done),
                new(
                    "Review validation behavior",
                    "Check request and use-case validation messages for consistency.",
                    TaskPriority.Normal,
                    TaskStatus.InProgress),
                new(
                    "Write integration tests",
                    "Cover ownership, activity feed, and demo seed behavior.",
                    TaskPriority.Critical,
                    TaskStatus.Done),
            ]),
        new(
            "Portfolio Demo",
            [
                new(
                    "Prepare stakeholder walkthrough",
                    "Walk through task lists, board updates, and activity history.",
                    TaskPriority.Low,
                    TaskStatus.Todo),
            ]),
    ];

    public async Task EnsureDemoDataAsync(CancellationToken cancellationToken = default)
    {
        var options = demoAuthOptions.Value;
        if (!options.Enabled)
        {
            return;
        }

        var user = await FindDemoUserAsync(options, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Demo data was not seeded because the configured demo user does not exist.");
            return;
        }

        var demoListTitles = DemoTaskLists.Select(x => x.Title).ToArray();
        var existingLists = await dbContext.TaskLists
            .Include(x => x.Tasks)
            .Where(x => x.UserId == user.Id && demoListTitles.Contains(x.Title.Value))
            .ToListAsync(cancellationToken);

        var nowUtc = dateTimeProvider.UtcNow();
        var activityOffset = 0;
        var seededListCount = 0;
        var seededTaskCount = 0;
        var seededActivityCount = 0;

        foreach (var listSeed in DemoTaskLists)
        {
            var taskList = ResolveExistingTaskList(existingLists, listSeed.Title, user.Id);
            var taskListWasCreated = false;

            if (taskList is null)
            {
                taskList = CreateTaskList(listSeed.Title, user.Id);
                await dbContext.TaskLists.AddAsync(taskList, cancellationToken);
                existingLists.Add(taskList);
                taskListWasCreated = true;
                seededListCount++;
            }

            if (await EnsureActivityAsync(
                    taskList.Id,
                    null,
                    TaskActivityType.TaskListCreated,
                    $"Created task list \"{taskList.Title.Value}\"",
                    nowUtc.AddMinutes(activityOffset++),
                    null,
                    cancellationToken))
            {
                seededActivityCount++;
            }

            foreach (var taskSeed in listSeed.Tasks)
            {
                var task = ResolveExistingTask(taskList, taskSeed.Title);
                var taskWasCreated = false;

                if (task is null)
                {
                    task = CreateTask(taskList, taskSeed);
                    if (!taskListWasCreated)
                    {
                        await dbContext.TaskItems.AddAsync(task, cancellationToken);
                    }

                    taskWasCreated = true;
                    seededTaskCount++;
                }

                if (await EnsureActivityAsync(
                        taskList.Id,
                        task.Id,
                        TaskActivityType.TaskCreated,
                        $"Created task \"{task.Title.Value}\"",
                        nowUtc.AddMinutes(activityOffset++),
                        task.Title.Value,
                        cancellationToken))
                {
                    seededActivityCount++;
                }

                if (taskWasCreated && taskSeed.Status != TaskStatus.Todo)
                {
                    var previousStatus = task.Status;
                    var updateStatusResult = task.UpdateStatus(taskSeed.Status, nowUtc);
                    if (updateStatusResult.IsFailed)
                    {
                        throw new InvalidOperationException(string.Join("; ", updateStatusResult.Errors));
                    }

                    if (await EnsureActivityAsync(
                            taskList.Id,
                            task.Id,
                            TaskActivityType.TaskStatusChanged,
                            $"Moved task \"{task.Title.Value}\" from {previousStatus} to {task.Status}",
                            nowUtc.AddMinutes(activityOffset++),
                            task.Title.Value,
                            cancellationToken))
                    {
                        seededActivityCount++;
                    }
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (seededListCount > 0 || seededTaskCount > 0 || seededActivityCount > 0)
        {
            logger.LogInformation(
                "Seeded demo data for user '{Username}': {TaskListCount} lists, {TaskCount} tasks, {ActivityCount} activities.",
                user.Username,
                seededListCount,
                seededTaskCount,
                seededActivityCount);
        }
    }

    private TaskList? ResolveExistingTaskList(IReadOnlyCollection<TaskList> existingLists, string title, Guid userId)
    {
        var matches = existingLists
            .Where(x => string.Equals(x.Title.Value, title, StringComparison.Ordinal))
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToList();

        if (matches.Count > 1)
        {
            logger.LogWarning(
                "Found {TaskListCount} demo task lists titled '{Title}' for user '{UserId}'. Using task list '{TaskListId}' as the demo seed anchor and leaving duplicates untouched.",
                matches.Count,
                title,
                userId,
                matches[0].Id);
        }

        return matches.FirstOrDefault();
    }

    private TaskItem? ResolveExistingTask(TaskList taskList, string title)
    {
        var matches = taskList.Tasks
            .Where(x => string.Equals(x.Title.Value, title, StringComparison.Ordinal))
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToList();

        if (matches.Count > 1)
        {
            logger.LogWarning(
                "Found {TaskCount} demo tasks titled '{Title}' in task list '{TaskListId}'. Using task '{TaskId}' as the demo seed anchor and leaving duplicates untouched.",
                matches.Count,
                title,
                taskList.Id,
                matches[0].Id);
        }

        return matches.FirstOrDefault();
    }

    private async Task<User?> FindDemoUserAsync(DemoAuthOptions options, CancellationToken cancellationToken)
    {
        var canonicalUsername = UsernameNormalizer.Normalize(options.Username);

        var user = await dbContext.Users
            .SingleOrDefaultAsync(x => x.Id == options.UserId, cancellationToken);

        user ??= await dbContext.Users
            .SingleOrDefaultAsync(x => x.Username == canonicalUsername, cancellationToken);

        return user;
    }

    private static TaskList CreateTaskList(string title, Guid userId)
    {
        var titleResult = TaskListTitle.Create(title);
        if (titleResult.IsFailed)
        {
            throw new InvalidOperationException(string.Join("; ", titleResult.Errors));
        }

        var taskListResult = TaskList.Create(titleResult.Value, userId);
        if (taskListResult.IsFailed)
        {
            throw new InvalidOperationException(string.Join("; ", taskListResult.Errors));
        }

        return taskListResult.Value;
    }

    private static TaskItem CreateTask(TaskList taskList, DemoTaskSeed taskSeed)
    {
        var titleResult = TaskItemTitle.Create(taskSeed.Title);
        if (titleResult.IsFailed)
        {
            throw new InvalidOperationException(string.Join("; ", titleResult.Errors));
        }

        var descriptionResult = TaskDescription.Create(taskSeed.Description);
        if (descriptionResult.IsFailed)
        {
            throw new InvalidOperationException(string.Join("; ", descriptionResult.Errors));
        }

        var taskResult = taskList.AddTask(titleResult.Value, descriptionResult.Value, taskSeed.Priority);
        if (taskResult.IsFailed)
        {
            throw new InvalidOperationException(string.Join("; ", taskResult.Errors));
        }

        return taskResult.Value;
    }

    private async Task<bool> EnsureActivityAsync(
        Guid taskListId,
        Guid? taskItemId,
        TaskActivityType activityType,
        string message,
        DateTime createdAtUtc,
        string? taskTitleSnapshot,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.TaskActivities.AnyAsync(
            x => x.TaskListId == taskListId
                && x.TaskItemId == taskItemId
                && x.Type == activityType
                && x.Message == message,
            cancellationToken);

        if (exists)
        {
            return false;
        }

        var activityResult = TaskActivity.Create(
            taskListId,
            taskItemId,
            activityType,
            message,
            createdAtUtc,
            taskTitleSnapshot);
        if (activityResult.IsFailed)
        {
            throw new InvalidOperationException(string.Join("; ", activityResult.Errors));
        }

        await dbContext.TaskActivities.AddAsync(activityResult.Value, cancellationToken);
        return true;
    }

    private sealed record DemoTaskListSeed(string Title, IReadOnlyCollection<DemoTaskSeed> Tasks);

    private sealed record DemoTaskSeed(
        string Title,
        string Description,
        TaskPriority Priority,
        TaskStatus Status);
}
