using FluentValidation;
using MediatR;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Application.Abstractions.Repositories.Query;
using TaskHub.Application.Features.TaskItem.ReadModels;
using TaskHub.Domain.Base;
using TaskHub.Domain.Policies;

namespace TaskHub.Application.Features.TaskItem.Queries;

public sealed record GetTaskItemsBoardQuery(Guid TaskListId) : IRequest<Result<IReadOnlyCollection<TaskItemReadModel>>>;

public sealed class GetTaskItemsBoardQueryHandler(
    ITaskItemQueryRepository taskItemQueryRepository,
    ITaskListCommandRepository taskListCommandRepository)
    : IRequestHandler<GetTaskItemsBoardQuery, Result<IReadOnlyCollection<TaskItemReadModel>>>
{
    public async Task<Result<IReadOnlyCollection<TaskItemReadModel>>> Handle(GetTaskItemsBoardQuery request, CancellationToken ct)
    {
        var taskListResult = await taskListCommandRepository.GetByIdAsync(request.TaskListId, ct);
        if (taskListResult.IsFailed)
        {
            return Result.Fail<IReadOnlyCollection<TaskItemReadModel>>(taskListResult.Errors);
        }

        var tasks = await taskItemQueryRepository.GetByTaskListIdAsync(request.TaskListId, ct);

        foreach (var task in tasks)
        {
            task.AllowedTargetStatuses = TaskStatusTransitionPolicy.GetAllowedTargetStatuses(task.Status);
        }

        return Result.Success(tasks);
    }
}

public sealed class GetTaskItemsBoardQueryValidator : AbstractValidator<GetTaskItemsBoardQuery>
{
    public GetTaskItemsBoardQueryValidator()
    {
        RuleFor(x => x.TaskListId).NotEmpty();
    }
}
