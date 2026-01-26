using FluentResults;
using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;

namespace TaskHub.Application.Features.TaskList.Commands;

public sealed record DeleteTaskListCommand(Guid TaskListId) : IRequest<Result>;

public sealed class DeleteTaskListCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskListCommandRepository taskListCommandRepository)
    : IRequestHandler<DeleteTaskListCommand, Result>
{
    public async Task<Result> Handle(DeleteTaskListCommand request, CancellationToken ct)
    {
        var getResult = await taskListCommandRepository.GetByIdAsync(request.TaskListId, ct);
        if (getResult.IsFailed)
        {
            return Result.Fail(getResult.Errors);
        }

        taskListCommandRepository.Remove(getResult.Value);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

public sealed class DeleteTaskListCommandValidator : AbstractValidator<DeleteTaskListCommand>
{
    public DeleteTaskListCommandValidator()
    {
        RuleFor(x => x.TaskListId).NotEmpty();
    }
}
