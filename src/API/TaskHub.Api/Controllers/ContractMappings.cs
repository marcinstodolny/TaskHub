using TaskItemApplicationResponse = TaskHub.Application.Features.TaskItem.Response.TaskItemResponse;
using TaskListApplicationResponse = TaskHub.Application.Features.TaskList.Response.TaskListResponse;
using TaskHub.Contracts.Common;
using TaskHub.Contracts.TaskItem;
using TaskHub.Contracts.TaskList;

namespace TaskHub.Api.Controllers;

internal static class ContractMappings
{
    public static TaskCardResponse ToContract(this TaskItemApplicationResponse source) => new()
    {
        Id = source.Id,
        TaskListId = source.TaskListId,
        Title = source.Title,
        Description = source.Description,
        Priority = source.Priority,
        Status = source.Status,
        AllowedTargetStatuses = source.AllowedTargetStatuses
    };

    public static TaskItemLightResponse ToLightContract(this TaskItemApplicationResponse source) => new()
    {
        Id = source.Id,
        TaskListId = source.TaskListId,
        Title = source.Title
    };

    public static TaskListLightResponse ToContract(this TaskListApplicationResponse source) => new()
    {
        Id = source.Id,
        Title = source.Title
    };

    public static PaginatedResponse<TTarget> Map<TSource, TTarget>(this TaskHub.Application.Base.Response.PaginatedResponse<TSource> source, Func<TSource, TTarget> map)
        => new()
        {
            Items = source.Items.Select(map).ToList(),
            CurrentPage = source.CurrentPage,
            TotalPageCount = source.TotalPageCount
        };
}
