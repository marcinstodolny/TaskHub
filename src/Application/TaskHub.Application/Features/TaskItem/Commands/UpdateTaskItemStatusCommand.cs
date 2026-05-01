using FluentValidation;
using MediatR;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Domain.Common;
using TaskHub.Domain.Enums;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Application.Features.TaskItem.Commands;

public sealed record UpdateTaskItemStatusCommand(Guid TaskItemId, TaskStatus Status) : IRequest<Result>;

public sealed class UpdateTaskItemStatusCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskItemCommandRepository taskItemCommandRepository,
    ITaskActivityCommandRepository taskActivityCommandRepository,
    ITaskActivityNotifier taskActivityNotifier,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateTaskItemStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateTaskItemStatusCommand request, CancellationToken ct)
    {
        var getResult = await taskItemCommandRepository.GetByIdAsync(request.TaskItemId, ct);
        if (getResult.IsFailed)
        {
            return Result.Fail(getResult.Errors);
        }

        var task = getResult.Value;
        var previousStatus = task.Status;
        var updateResult = task.UpdateStatus(request.Status, dateTimeProvider.UtcNow());

        if (updateResult.IsFailed)
        {
            return updateResult;
        }

        var activityResult = Domain.Entities.TaskActivity.Create(
            task.TaskListId,
            task.Id,
            TaskActivityType.TaskStatusChanged,
            $"Moved task \"{task.Title.Value}\" from {previousStatus} to {task.Status}",
            dateTimeProvider.UtcNow(),
            task.Title.Value);
        if (activityResult.IsFailed)
        {
            return Result.Fail(activityResult.Errors);
        }

        await taskActivityCommandRepository.AddAsync(activityResult.Value, ct);

        await unitOfWork.SaveChangesAsync(ct);
        await taskActivityNotifier.NotifyTaskListActivityChangedAsync(task.TaskListId, ct);
        return Result.Success();
    }
}

public sealed class UpdateTaskItemStatusCommandValidator : AbstractValidator<UpdateTaskItemStatusCommand>
{
    public UpdateTaskItemStatusCommandValidator()
    {
        RuleFor(x => x.TaskItemId).NotEmpty();
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid task status.");
    }
}
