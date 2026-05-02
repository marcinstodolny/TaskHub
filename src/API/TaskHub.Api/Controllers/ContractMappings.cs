using TaskActivityReadModel = TaskHub.Application.Features.TaskActivity.ReadModels.TaskActivityReadModel;
using DashboardSummaryReadModel = TaskHub.Application.Features.Dashboard.ReadModels.DashboardSummaryReadModel;
using TaskItemReadModel = TaskHub.Application.Features.TaskItem.ReadModels.TaskItemReadModel;
using TaskListDetailsReadModel = TaskHub.Application.Features.TaskList.ReadModels.TaskListDetailsReadModel;
using TaskListSummaryReadModel = TaskHub.Application.Features.TaskList.ReadModels.TaskListSummaryReadModel;
using TaskHub.Contracts.Common;
using TaskHub.Contracts.Dashboard;
using TaskHub.Contracts.TaskActivity;
using TaskHub.Contracts.TaskItem;
using TaskHub.Contracts.TaskList;

namespace TaskHub.Api.Controllers;

internal static class ContractMappings
{
    public static DashboardSummaryResponse ToContract(this DashboardSummaryReadModel source) => new()
    {
        TaskListCount = source.TaskListCount,
        TotalTaskCount = source.TotalTaskCount,
        TodoCount = source.TodoCount,
        InProgressCount = source.InProgressCount,
        DoneCount = source.DoneCount,
        CancelledCount = source.CancelledCount,
        RecentActivities = source.RecentActivities.Select(activity => activity.ToContract()).ToList()
    };

    public static TaskActivityResponse ToContract(this TaskActivityReadModel source) => new()
    {
        Id = source.Id,
        TaskListId = source.TaskListId,
        TaskItemId = source.TaskItemId,
        Type = source.Type,
        Message = source.Message,
        CreatedAtUtc = source.CreatedAtUtc,
        TaskTitleSnapshot = source.TaskTitleSnapshot
    };

    public static TaskCardResponse ToContract(this TaskItemReadModel source) => new()
    {
        Id = source.Id,
        TaskListId = source.TaskListId,
        Title = source.Title,
        Description = source.Description,
        Priority = source.Priority,
        Status = source.Status,
        AllowedTargetStatuses = source.AllowedTargetStatuses
    };

    public static TaskItemLightResponse ToLightContract(this TaskItemReadModel source) => new()
    {
        Id = source.Id,
        TaskListId = source.TaskListId,
        Title = source.Title
    };

    public static TaskListLightResponse ToContract(this TaskListSummaryReadModel source) => new()
    {
        Id = source.Id,
        Title = source.Title,
        TasksCount = source.TasksCount
    };

    public static TaskListLightResponse ToContract(this TaskListDetailsReadModel source) => new()
    {
        Id = source.Id,
        Title = source.Title,
        TasksCount = source.TasksCount
    };

    public static PaginatedResponse<TTarget> Map<TSource, TTarget>(this Application.Common.Pagination.PagedResult<TSource> source, Func<TSource, TTarget> map)
        => new()
        {
            Items = source.Items.Select(map).ToList(),
            CurrentPage = source.PageNumber,
            TotalPageCount = source.TotalPages
        };
}
