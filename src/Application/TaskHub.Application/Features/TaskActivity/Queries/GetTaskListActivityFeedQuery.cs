using FluentValidation;
using MediatR;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Application.Abstractions.Repositories.Query;
using TaskHub.Application.Common.Pagination;
using TaskHub.Application.Features.TaskActivity.ReadModels;
using TaskHub.Domain.Common;

namespace TaskHub.Application.Features.TaskActivity.Queries;

public sealed record GetTaskListActivityFeedQuery(Guid TaskListId, int Page = 1, int Count = 50)
    : IRequest<Result<PagedResult<TaskActivityReadModel>>>;

public sealed class GetTaskListActivityFeedQueryHandler(
    ITaskListCommandRepository taskListCommandRepository,
    ITaskActivityQueryRepository taskActivityQueryRepository)
    : IRequestHandler<GetTaskListActivityFeedQuery, Result<PagedResult<TaskActivityReadModel>>>
{
    public async Task<Result<PagedResult<TaskActivityReadModel>>> Handle(GetTaskListActivityFeedQuery request, CancellationToken ct)
    {
        var taskListResult = await taskListCommandRepository.GetByIdAsync(request.TaskListId, ct);
        if (taskListResult.IsFailed)
        {
            return Result.Fail<PagedResult<TaskActivityReadModel>>(taskListResult.Errors);
        }

        var activities = await taskActivityQueryRepository.GetByTaskListIdAsync(request.TaskListId, request.Page, request.Count, ct);
        return Result.Success(activities);
    }
}

public sealed class GetTaskListActivityFeedQueryValidator : AbstractValidator<GetTaskListActivityFeedQuery>
{
    public GetTaskListActivityFeedQueryValidator()
    {
        RuleFor(x => x.TaskListId).NotEmpty();
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");
        RuleFor(x => x.Count)
            .GreaterThan(0).WithMessage("Count must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Count must be less than or equal to 100.");
    }
}
