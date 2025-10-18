using FluentResults;
using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Domain.Entities;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Commands
{
    public sealed record CreateTaskItemCommand(Guid TaskListId, string Title, string? Description, DateTime? DueDateUtc, TaskPriority Priority) : IRequest<Result<Guid>>;

    public sealed class CreateTaskItemHandler(IUnitOfWork unitOfWork, ITaskListCommandRepository taskListCommandRepository, ITaskItemCommandRepository taskItemCommandRepository) : IRequestHandler<CreateTaskItemCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateTaskItemCommand request, CancellationToken ct)
        {
            var getResult = await taskListCommandRepository.GetByIdAsync(request.TaskListId, ct);
            if (getResult.IsFailed)
            {
                return Result.Fail(getResult.Errors);
            }

            var titleResult = Title.Create(request.Title);
            if (titleResult.IsFailed)
            {
                return Result.Fail(titleResult.Errors);
            }
            var descriptionResult = request.Description is null ? null : TaskDescription.Create(request.Description);
            if (descriptionResult is not null && descriptionResult.IsFailed)
            {
                return Result.Fail(descriptionResult.Errors);
            }

            var createResult = TaskItem.Create(request.TaskListId, titleResult.Value, descriptionResult?.Value, request.Priority);
            if (createResult.IsFailed)
            {
                return Result.Fail(createResult.Errors);
            }

            await taskItemCommandRepository.AddAsync(createResult.Value, ct);

            await unitOfWork.SaveChangesAsync(ct);
            return createResult.Value.Id;
        }
    }

    public sealed class CreateTaskItemValidator : AbstractValidator<CreateTaskItemCommand>
    {
        public CreateTaskItemValidator()
        {
            RuleFor(x => x.TaskListId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        }
    }
}
