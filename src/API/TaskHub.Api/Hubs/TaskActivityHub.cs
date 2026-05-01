using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaskHub.Application.Features.TaskList.Queries;

namespace TaskHub.Api.Hubs;

[Authorize]
public sealed class TaskActivityHub(ISender sender) : Hub
{
    public async Task JoinTaskListGroup(Guid taskListId)
    {
        var result = await sender.Send(new GetTaskListByIdQuery(taskListId), Context.ConnectionAborted);
        if (result.IsFailed)
        {
            throw new HubException("Task list not found.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetTaskListGroupName(taskListId), Context.ConnectionAborted);
    }

    public Task LeaveTaskListGroup(Guid taskListId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetTaskListGroupName(taskListId), Context.ConnectionAborted);
    }

    public static string GetTaskListGroupName(Guid taskListId) => $"task-list:{taskListId}";
}
