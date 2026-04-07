using FluentValidation;
using MediatR;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Domain.Base;

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

        return Result.Success();
    }
}

public sealed class DeleteTaskListCommandValidator : AbstractValidator<DeleteTaskListCommand>
{
    public DeleteTaskListCommandValidator()
    {
        RuleFor(x => x.TaskListId).NotEmpty();
    }
}
