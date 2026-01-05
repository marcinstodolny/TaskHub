using FluentResults;
using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Response;

namespace TaskHub.Application.Queries
{
    public sealed record GetTaskListsQuery() : IRequest<Result<List<TaskListResponse>>>;

    public sealed class GetTaskListsQueryHandler(ITaskListQueryRepository taskListQueryRepository) : IRequestHandler<GetTaskListsQuery, Result<List<TaskListResponse>>>
    {
        public async Task<Result<List<TaskListResponse>>> Handle(GetTaskListsQuery request, CancellationToken ct)
        {
            var list = await taskListQueryRepository.GetAllAsync(ct);
            return list ?? new List<TaskListResponse>();
        }
    }

    public sealed class GetTaskListsQueryValidator : AbstractValidator<GetTaskListsQuery>
    {
        public GetTaskListsQueryValidator()
        {
        }
    }
}
