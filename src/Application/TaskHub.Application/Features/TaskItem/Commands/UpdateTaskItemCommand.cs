using FluentValidation;
using MediatR;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Application.Features.TaskItem.ReadModels;
using TaskHub.Domain.Common;
using TaskHub.Domain.ValueObjects;
using TaskPriority = TaskHub.Domain.Enums.TaskPriority;

namespace TaskHub.Application.Features.TaskItem.Commands;

public sealed record UpdateTaskItemCommand(
    Guid TaskItemId,
    string Title,
    string? Description,
    TaskPriority Priority) : IRequest<Result<TaskItemReadModel>>;

public sealed class UpdateTaskItemCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskItemCommandRepository taskItemCommandRepository,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateTaskItemCommand, Result<TaskItemReadModel>>
{
    public async Task<Result<TaskItemReadModel>> Handle(UpdateTaskItemCommand request, CancellationToken ct)
    {
        var getResult = await taskItemCommandRepository.GetByIdAsync(request.TaskItemId, ct);
        if (getResult.IsFailed)
        {
            return Result.Fail<TaskItemReadModel>(getResult.Errors);
        }

        var titleResult = TaskItemTitle.Create(request.Title);
        if (titleResult.IsFailed)
        {
            return Result.Fail<TaskItemReadModel>(titleResult.Errors);
        }

        TaskDescription? description = null;
        if (request.Description is not null)
        {
            var descriptionResult = TaskDescription.Create(request.Description);
            if (descriptionResult.IsFailed)
            {
                return Result.Fail<TaskItemReadModel>(descriptionResult.Errors);
            }

            description = descriptionResult.Value;
        }

        var taskItem = getResult.Value;
        var updateResult = taskItem.Update(titleResult.Value, description, request.Priority, dateTimeProvider.UtcNow());
        if (updateResult.IsFailed)
        {
            return Result.Fail<TaskItemReadModel>(updateResult.Errors);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new TaskItemReadModel
        {
            Id = taskItem.Id,
            TaskListId = taskItem.TaskListId,
            Title = taskItem.Title.Value,
            Description = taskItem.Description?.Value,
            Priority = taskItem.Priority,
            Status = taskItem.Status
        });
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
