using FluentResults;
using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Features.TaskList.Response;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Features.TaskList.Queries
{
    public sealed record GetTaskListByTitleQuery(string Title) : IRequest<Result<TaskListResponse>>;

    public sealed class GetTaskListByTitleQueryHandler(ITaskListQueryRepository taskListQueryRepository)
        : IRequestHandler<GetTaskListByTitleQuery, Result<TaskListResponse>>
    {
        public async Task<Result<TaskListResponse>> Handle(GetTaskListByTitleQuery request, CancellationToken ct)
        {
            var list = await taskListQueryRepository.GetByTitleAsync(request.Title, ct);
            return list is null
                ? Result.Fail<TaskListResponse>($"TaskList with title '{request.Title}' not found.")
                : Result.Ok(list);
        }
    }

    public sealed class GetTaskListByTitleQueryValidator : AbstractValidator<GetTaskListByTitleQuery>
    {
        public GetTaskListByTitleQueryValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(TaskListTitle.MaxLength);
        }
    }
}
