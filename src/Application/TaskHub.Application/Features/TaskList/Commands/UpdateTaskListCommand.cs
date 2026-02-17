using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Application.Features.TaskList.Response;
using TaskHub.Domain.Base;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Features.TaskList.Commands;

public sealed record UpdateTaskListCommand(Guid TaskListId, string Title) : IRequest<Result<TaskListResponse>>;

public sealed class UpdateTaskListCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskListCommandRepository taskListCommandRepository)
    : IRequestHandler<UpdateTaskListCommand, Result<TaskListResponse>>
{
    public async Task<Result<TaskListResponse>> Handle(UpdateTaskListCommand request, CancellationToken ct)
    {
        var getResult = await taskListCommandRepository.GetByIdAsync(request.TaskListId, ct);
        if (getResult.IsFailed)
        {
            return Result.Fail<TaskListResponse>(getResult.Errors);
        }

        var titleResult = TaskListTitle.Create(request.Title);
        if (titleResult.IsFailed)
        {
            return Result.Fail<TaskListResponse>(titleResult.Errors);
        }

        var taskList = getResult.Value;
        var renameResult = taskList.Rename(titleResult.Value);
        if (renameResult.IsFailed)
        {
            return Result.Fail<TaskListResponse>(renameResult.Errors);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new TaskListResponse { Id = taskList.Id, Title = taskList.Title.Value });
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
