using FluentValidation;
using MediatR;
using TaskHub.Application.Abstractions.Repositories.Query;
using TaskHub.Application.Common.Pagination;
using TaskHub.Application.Features.TaskItem.ReadModels;
using TaskHub.Domain.Common;

namespace TaskHub.Application.Features.TaskItem.Queries;

public sealed record GetTaskItemsQuery(int Page = 1, int Count = 10)
    : IRequest<Result<PagedResult<TaskItemReadModel>>>;

public sealed class GetTaskItemsQueryHandler(ITaskItemQueryRepository taskItemQueryRepository)
    : IRequestHandler<GetTaskItemsQuery, Result<PagedResult<TaskItemReadModel>>>
{
    public async Task<Result<PagedResult<TaskItemReadModel>>> Handle(GetTaskItemsQuery request, CancellationToken ct)
    {
        var items = await taskItemQueryRepository.GetAllAsync(request.Page, request.Count, ct);
        return Result.Success(items);
    }
}

public sealed class GetTaskItemsQueryValidator : AbstractValidator<GetTaskItemsQuery>
{
    public GetTaskItemsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");
        RuleFor(x => x.Count)
            .GreaterThan(0).WithMessage("Count must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Count must be less than or equal to 100.");
    }
}
