using FluentValidation;
using MediatR;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Domain.Common;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Features.TaskItem.Commands
{
    public sealed record CreateTaskItemCommand(Guid TaskListId, string Title, string? Description, TaskPriority Priority) : IRequest<Result<Guid>>;

    public sealed class CreateTaskItemHandler(IUnitOfWork unitOfWork, ITaskListCommandRepository taskListCommandRepository, ITaskItemCommandRepository taskItemCommandRepository) : IRequestHandler<CreateTaskItemCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateTaskItemCommand request, CancellationToken ct)
        {
            var getResult = await taskListCommandRepository.GetByIdAsync(request.TaskListId, ct);
            if (getResult.IsFailed)
            {
                return Result.Fail<Guid>(getResult.Errors);
            }

            var titleResult = TaskItemTitle.Create(request.Title);
            if (titleResult.IsFailed)
            {
                return Result.Fail<Guid>(titleResult.Errors);
            }
            var descriptionResult = request.Description is null ? null : TaskDescription.Create(request.Description);
            if (descriptionResult is not null && descriptionResult.IsFailed)
            {
                return Result.Fail<Guid>(descriptionResult.Errors);
            }

            var createResult = Domain.Entities.TaskItem.Create(request.TaskListId, titleResult.Value, descriptionResult?.Value, request.Priority);
            if (createResult.IsFailed)
            {
                return Result.Fail<Guid>(createResult.Errors);
            }

            await taskItemCommandRepository.AddAsync(createResult.Value, ct);

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success(createResult.Value.Id);
        }
    }

    public sealed class CreateTaskItemCommandValidator : AbstractValidator<CreateTaskItemCommand>
    {
        public CreateTaskItemCommandValidator()
        {
            RuleFor(x => x.TaskListId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(TaskItemTitle.MaxLength);
            RuleFor(x => x.Description).MaximumLength(TaskDescription.MaxLength).When(x => x.Description is not null);
        }
    }
}
