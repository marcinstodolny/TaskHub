using FluentResults;
using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Application.Response;
using TaskHub.Domain.ValueObjects;
using TaskPriority = TaskHub.Domain.Enums.TaskPriority;

namespace TaskHub.Application.Commands;

public sealed record UpdateTaskItemCommand(
    Guid TaskItemId,
    string Title,
    string? Description,
    TaskPriority Priority) : IRequest<Result<TaskItemResponse>>;

public sealed class UpdateTaskItemCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskItemCommandRepository taskItemCommandRepository,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateTaskItemCommand, Result<TaskItemResponse>>
{
    public async Task<Result<TaskItemResponse>> Handle(UpdateTaskItemCommand request, CancellationToken ct)
    {
        var getResult = await taskItemCommandRepository.GetByIdAsync(request.TaskItemId, ct);
        if (getResult.IsFailed)
        {
            return Result.Fail(getResult.Errors);
        }

        var titleResult = TaskItemTitle.Create(request.Title);
        if (titleResult.IsFailed)
        {
            return Result.Fail(titleResult.Errors);
        }

        TaskDescription? description = null;
        if (request.Description is not null)
        {
            var descriptionResult = TaskDescription.Create(request.Description);
            if (descriptionResult.IsFailed)
            {
                return Result.Fail(descriptionResult.Errors);
            }

            description = descriptionResult.Value;
        }

        var taskItem = getResult.Value;
        var updateResult = taskItem.Update(titleResult.Value, description, request.Priority, dateTimeProvider.UtcNow());
        if (updateResult.IsFailed)
        {
            return Result.Fail(updateResult.Errors);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return new TaskItemResponse
        {
            Id = taskItem.Id,
            TaskListId = taskItem.TaskListId,
            Title = taskItem.Title.Value,
            Description = taskItem.Description?.Value,
            Priority = taskItem.Priority,
            Status = taskItem.Status
        };
    }
}

public sealed class UpdateTaskItemCommandValidator : AbstractValidator<UpdateTaskItemCommand>
{
    public UpdateTaskItemCommandValidator()
    {
        RuleFor(x => x.TaskItemId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(TaskItemTitle.MaxLength);
        RuleFor(x => x.Priority).IsInEnum();
        When(x => x.Description is not null, () =>
        {
            RuleFor(x => x.Description).NotEmpty().MaximumLength(TaskDescription.MaxLength);
        });
    }
}
