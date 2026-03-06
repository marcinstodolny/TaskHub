using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Application.Features.TaskItem.ReadModels;
using TaskHub.Domain.Base;

namespace TaskHub.Application.Features.TaskItem.Queries;

public sealed record GetTaskItemByIdQuery(Guid TaskItemId) : IRequest<Result<TaskItemReadModel>>;

public sealed class GetTaskItemByIdQueryHandler(ITaskItemQueryRepository taskItemQueryRepository)
    : IRequestHandler<GetTaskItemByIdQuery, Result<TaskItemReadModel>>
{
    public async Task<Result<TaskItemReadModel>> Handle(GetTaskItemByIdQuery request, CancellationToken ct)
    {
        var item = await taskItemQueryRepository.GetByIdAsync(request.TaskItemId, ct);
        if (item == null)
        {
            return Result.Fail<TaskItemReadModel>($"TaskItem {request.TaskItemId} not found.");
        }

        return Result.Success(item);
    }
}

public sealed class GetTaskItemByIdQueryValidator : AbstractValidator<GetTaskItemByIdQuery>
{
    public GetTaskItemByIdQueryValidator()
    {
        RuleFor(x => x.TaskItemId).NotEmpty();
    }
}