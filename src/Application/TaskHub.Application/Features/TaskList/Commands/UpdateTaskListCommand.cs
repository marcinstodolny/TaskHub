using FluentValidation;
using MediatR;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Application.Features.TaskList.ReadModels;
using TaskHub.Domain.Common;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Features.TaskList.Commands;

public sealed record UpdateTaskListCommand(Guid TaskListId, string Title) : IRequest<Result<TaskListSummaryReadModel>>;

public sealed class UpdateTaskListCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskListCommandRepository taskListCommandRepository,
    ITaskActivityCommandRepository taskActivityCommandRepository,
    ITaskActivityNotifier taskActivityNotifier,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateTaskListCommand, Result<TaskListSummaryReadModel>>
{
    public async Task<Result<TaskListSummaryReadModel>> Handle(UpdateTaskListCommand request, CancellationToken ct)
    {
        var getResult = await taskListCommandRepository.GetByIdAsync(request.TaskListId, ct);
        if (getResult.IsFailed)
        {
            return Result.Fail<TaskListSummaryReadModel>(getResult.Errors);
        }

        var titleResult = TaskListTitle.Create(request.Title);
        if (titleResult.IsFailed)
        {
            return Result.Fail<TaskListSummaryReadModel>(titleResult.Errors);
        }

        var taskList = getResult.Value;
        var previousTitle = taskList.Title.Value;
        var renameResult = taskList.Rename(titleResult.Value);
        if (renameResult.IsFailed)
        {
            return Result.Fail<TaskListSummaryReadModel>(renameResult.Errors);
        }

        var activityResult = Domain.Entities.TaskActivity.Create(
            taskList.Id,
            null,
            TaskActivityType.TaskListRenamed,
            $"Renamed task list from \"{previousTitle}\" to \"{taskList.Title.Value}\"",
            dateTimeProvider.UtcNow());
        if (activityResult.IsFailed)
        {
            return Result.Fail<TaskListSummaryReadModel>(activityResult.Errors);
        }

        await taskActivityCommandRepository.AddAsync(activityResult.Value, ct);

        await unitOfWork.SaveChangesAsync(ct);
        await taskActivityNotifier.NotifyTaskListActivityChangedAsync(taskList.Id, CancellationToken.None);

        return Result.Success(new TaskListSummaryReadModel { Id = taskList.Id, Title = taskList.Title.Value });
    }
}

public sealed class UpdateTaskListCommandValidator : AbstractValidator<UpdateTaskListCommand>
{
    public UpdateTaskListCommandValidator()
    {
        RuleFor(x => x.TaskListId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(TaskListTitle.MaxLength);
    }
}
