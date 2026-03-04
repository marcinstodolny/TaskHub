using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Features.TaskItem.Response;
using TaskHub.Domain.Base;
using TaskHub.Domain.Policies;

namespace TaskHub.Application.Features.TaskItem.Queries;

public sealed record GetTaskItemsBoardQuery(Guid TaskListId) : IRequest<Result<IReadOnlyCollection<TaskItemResponse>>>;

public sealed class GetTaskItemsBoardQueryHandler(ITaskItemQueryRepository taskItemQueryRepository)
    : IRequestHandler<GetTaskItemsBoardQuery, Result<IReadOnlyCollection<TaskItemResponse>>>
{
    public async Task<Result<IReadOnlyCollection<TaskItemResponse>>> Handle(GetTaskItemsBoardQuery request, CancellationToken ct)
    {
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
