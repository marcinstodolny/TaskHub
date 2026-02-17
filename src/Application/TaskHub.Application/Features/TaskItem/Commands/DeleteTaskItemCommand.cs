using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Domain.Base;

namespace TaskHub.Application.Features.TaskItem.Commands;

public sealed record DeleteTaskItemCommand(Guid TaskItemId) : IRequest<Result>;

public sealed class DeleteTaskItemCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskItemCommandRepository taskItemCommandRepository)
    : IRequestHandler<DeleteTaskItemCommand, Result>
{
    public async Task<Result> Handle(DeleteTaskItemCommand request, CancellationToken ct)
    {
        var getResult = await taskItemCommandRepository.GetByIdAsync(request.TaskItemId, ct);
        if (getResult.IsFailed)
        {
            return Result.Fail(getResult.Errors);
        }

        taskItemCommandRepository.Remove(getResult.Value);
        await unitOfWork.SaveChangesAsync(ct);

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
