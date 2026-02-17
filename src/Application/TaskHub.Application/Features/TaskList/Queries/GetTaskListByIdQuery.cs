using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Features.TaskList.Response;
using TaskHub.Domain.Base;

namespace TaskHub.Application.Features.TaskList.Queries
{
    public sealed record GetTaskListByIdQuery(Guid TaskListId) : IRequest<Result<TaskListResponse>>;

    public sealed class GetTaskListByIdQueryHandler(ITaskListQueryRepository taskListQueryRepository) : IRequestHandler<GetTaskListByIdQuery, Result<TaskListResponse>>
    {
        public async Task<Result<TaskListResponse>> Handle(GetTaskListByIdQuery request, CancellationToken ct)
        {
            var list = await taskListQueryRepository.GetByIdAsync(request.TaskListId, ct);
            return list is null ? Result.Fail<TaskListResponse>($"TaskList {request.TaskListId} not found.") : Result.Success(list);
        }
    }

    public sealed class GetTaskListByIdQueryValidator : AbstractValidator<GetTaskListByIdQuery>
    {
        public GetTaskListByIdQueryValidator()
        {
            RuleFor(x => x.TaskListId).NotEmpty();
        }
    }
}
