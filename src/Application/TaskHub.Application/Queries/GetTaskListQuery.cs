using FluentResults;
using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Domain.Entities;

namespace TaskHub.Application.Queries
{
    public sealed record GetTaskListQuery(Guid TaskListId) : IRequest<Result<List<TaskItem>>>;

    public sealed class GetTaskListQueryHandler(ITaskListQueryRepository taskListQueryRepository) : IRequestHandler<GetTaskListQuery, Result<List<TaskItem>>>
    {
        public async Task<Result<List<TaskItem>>> Handle(GetTaskListQuery request, CancellationToken ct)
        {
            var list = await taskListQueryRepository.GetByIdAsync(request.TaskListId, ct);
            return list?.Tasks.ToList() ?? Result.Fail<List<TaskItem>>($"TaskList {request.TaskListId} not found.");
        }
    }

    public sealed class CreateTaskValidator : AbstractValidator<GetTaskListQuery>
    {
        public CreateTaskValidator()
        {
            RuleFor(x => x.TaskListId).NotEmpty();
        }
    }
}
