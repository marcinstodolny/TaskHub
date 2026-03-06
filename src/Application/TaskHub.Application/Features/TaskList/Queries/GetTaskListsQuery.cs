using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Common.Pagination;
using TaskHub.Application.Features.TaskList.ReadModels;
using TaskHub.Domain.Base;

namespace TaskHub.Application.Features.TaskList.Queries
{
    public sealed record GetTaskListsQuery(int Page = 1, int Count = 10) : IRequest<Result<PagedResult<TaskListReadModel>>>;

    public sealed class GetTaskListsQueryHandler(ITaskListQueryRepository taskListQueryRepository) : IRequestHandler<GetTaskListsQuery, Result<PagedResult<TaskListReadModel>>>
    {
        public async Task<Result<PagedResult<TaskListReadModel>>> Handle(GetTaskListsQuery request, CancellationToken ct)
        {
            var list = await taskListQueryRepository.GetAllAsync(request.Page, request.Count, ct);
            return Result.Success(list);
        }
    }

    public sealed class GetTaskListsQueryValidator : AbstractValidator<GetTaskListsQuery>
    {
        public GetTaskListsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page number must be greater than 0.");
            RuleFor(x => x.Count)
                .GreaterThan(0).WithMessage("Count must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Count must be less than or equal to 100.");
        }
    }
}
