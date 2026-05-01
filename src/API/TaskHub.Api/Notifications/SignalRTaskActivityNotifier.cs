using Microsoft.AspNetCore.SignalR;
using TaskHub.Api.Hubs;
using TaskHub.Application.Abstractions;

namespace TaskHub.Api.Notifications;

public sealed class SignalRTaskActivityNotifier(
    IHubContext<TaskActivityHub> hubContext,
    ILogger<SignalRTaskActivityNotifier> logger) : ITaskActivityNotifier
{
    public async Task NotifyTaskListActivityChangedAsync(Guid taskListId, CancellationToken ct)
    {
        try
        {
            await hubContext.Clients
                .Group(TaskActivityHub.GetTaskListGroupName(taskListId))
                .SendAsync("TaskListActivityChanged", taskListId, ct);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to publish task activity notification for task list {TaskListId}.",
                taskListId);
        }
    }
}
