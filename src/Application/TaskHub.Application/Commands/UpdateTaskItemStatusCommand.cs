using FluentResults;
using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskStatus = TaskHub.Domain.Enums.TaskStatus;

namespace TaskHub.Application.Commands;

public sealed record UpdateTaskItemStatusCommand(Guid TaskItemId, TaskStatus Status) : IRequest<Result>;

public sealed class UpdateTaskItemStatusCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskItemCommandRepository taskItemCommandRepository,
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
        Result updateResult = request.Status switch
        {
            TaskStatus.InProgress => task.Start(dateTimeProvider.UtcNow()),
            TaskStatus.Done => task.Complete(dateTimeProvider.UtcNow()),
            TaskStatus.Cancelled => task.Cancel(dateTimeProvider.UtcNow()),
            _ => Result.Fail($"Status '{request.Status}' is not supported for updates.")
        };

        if (updateResult.IsFailed)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

public sealed class UpdateTaskItemStatusCommandValidator : AbstractValidator<UpdateTaskItemStatusCommand>
{
    public UpdateTaskItemStatusCommandValidator()
    {
        RuleFor(x => x.TaskItemId).NotEmpty();
        RuleFor(x => x.Status)
            .Must(status => status is TaskStatus.InProgress or TaskStatus.Done or TaskStatus.Cancelled)
            .WithMessage("Status must be InProgress, Done, or Cancelled.");
    }
}