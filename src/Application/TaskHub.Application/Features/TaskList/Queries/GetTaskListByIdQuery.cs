using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Features.TaskList.ReadModels;
using TaskHub.Domain.Base;

namespace TaskHub.Application.Features.TaskList.Queries
{
    public sealed record GetTaskListByIdQuery(Guid TaskListId) : IRequest<Result<TaskListReadModel>>;

    public sealed class GetTaskListByIdQueryHandler(ITaskListQueryRepository taskListQueryRepository) : IRequestHandler<GetTaskListByIdQuery, Result<TaskListReadModel>>
    {
        public async Task<Result<TaskListReadModel>> Handle(GetTaskListByIdQuery request, CancellationToken ct)
        {
            var list = await taskListQueryRepository.GetByIdAsync(request.TaskListId, ct);
            return list is null ? Result.Fail<TaskListReadModel>($"TaskList {request.TaskListId} not found.") : Result.Success(list);
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
