using TaskItemReadModel = TaskHub.Application.Features.TaskItem.ReadModels.TaskItemReadModel;
using TaskListReadModel = TaskHub.Application.Features.TaskList.ReadModels.TaskListReadModel;
using TaskHub.Contracts.Common;
using TaskHub.Contracts.TaskItem;
using TaskHub.Contracts.TaskList;

namespace TaskHub.Api.Controllers;

internal static class ContractMappings
{
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

    public static TaskListLightResponse ToContract(this TaskListReadModel source) => new()
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
