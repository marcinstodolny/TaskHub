using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskHub.Api.Hubs;

[Authorize]
public sealed class TaskActivityHub : Hub
{
    public Task JoinTaskListGroup(Guid taskListId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetTaskListGroupName(taskListId));
    }

    public Task LeaveTaskListGroup(Guid taskListId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetTaskListGroupName(taskListId));
    }

    public static string GetTaskListGroupName(Guid taskListId) => $"task-list:{taskListId}";
}
