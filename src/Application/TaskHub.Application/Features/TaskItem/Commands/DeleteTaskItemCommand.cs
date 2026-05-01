using FluentValidation;
using MediatR;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Domain.Common;
using TaskHub.Domain.Enums;

namespace TaskHub.Application.Features.TaskItem.Commands;

public sealed record DeleteTaskItemCommand(Guid TaskItemId) : IRequest<Result>;

public sealed class DeleteTaskItemCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskItemCommandRepository taskItemCommandRepository,
    ITaskActivityCommandRepository taskActivityCommandRepository,
    ITaskActivityNotifier taskActivityNotifier,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<DeleteTaskItemCommand, Result>
{
    public async Task<Result> Handle(DeleteTaskItemCommand request, CancellationToken ct)
    {
        var getResult = await taskItemCommandRepository.GetByIdAsync(request.TaskItemId, ct);
        if (getResult.IsFailed)
        {
            return Result.Fail(getResult.Errors);
        }

        var task = getResult.Value;
        var taskTitle = task.Title.Value;

        taskItemCommandRepository.Remove(task);

        var activityResult = Domain.Entities.TaskActivity.Create(
            task.TaskListId,
            task.Id,
            TaskActivityType.TaskDeleted,
            $"Deleted task \"{taskTitle}\"",
            dateTimeProvider.UtcNow(),
            taskTitle);
        if (activityResult.IsFailed)
        {
            return Result.Fail(activityResult.Errors);
        }

        await taskActivityCommandRepository.AddAsync(activityResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);
        await taskActivityNotifier.NotifyTaskListActivityChangedAsync(task.TaskListId, CancellationToken.None);

        return Result.Success();
    }
}

public sealed class DeleteTaskItemCommandValidator : AbstractValidator<DeleteTaskItemCommand>
{
    public DeleteTaskItemCommandValidator()
    {
        RuleFor(x => x.TaskItemId).NotEmpty();
    }
}
