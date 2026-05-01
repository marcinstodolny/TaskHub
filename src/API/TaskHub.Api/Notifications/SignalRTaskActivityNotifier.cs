using Microsoft.AspNetCore.SignalR;
using TaskHub.Api.Hubs;
using TaskHub.Application.Abstractions;

namespace TaskHub.Api.Notifications;

public sealed class SignalRTaskActivityNotifier(IHubContext<TaskActivityHub> hubContext) : ITaskActivityNotifier
{
    public Task NotifyTaskListActivityChangedAsync(Guid taskListId, CancellationToken ct)
    {
        return hubContext.Clients
            .Group(TaskActivityHub.GetTaskListGroupName(taskListId))
            .SendAsync("TaskListActivityChanged", taskListId, ct);
    }
}
