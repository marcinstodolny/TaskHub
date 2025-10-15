using FluentResults;
using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Domain.Enums;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Commands
{
    public sealed record CreateTaskCommand(Guid TaskListId, string Title, string? Description, DateTime? DueDateUtc, TaskPriority Priority) : IRequest<Result<Guid>>;

    public sealed class CreateTaskHandler(IUnitOfWork unitOfWork, ITaskListCommandRepository taskListCommandRepository) : IRequestHandler<CreateTaskCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateTaskCommand request, CancellationToken ct)
        {
            var getResult = await taskListCommandRepository.GetByIdAsync(request.TaskListId, ct);
            if (getResult.IsFailed)
            {
                return Result.Fail(getResult.Errors);
            }

            var titleResult = TaskTitle.Create(request.Title);
            if (titleResult.IsFailed)
            {
                return Result.Fail(titleResult.Errors);
            }
            var descriptionResult = request.Description is null ? null : TaskDescription.Create(request.Description);
            if (descriptionResult is not null && descriptionResult.IsFailed)
            {
                return Result.Fail(descriptionResult.Errors);
            }

            var addResult = getResult.Value.AddTask(titleResult.Value, descriptionResult?.Value, request.Priority);

            await unitOfWork.SaveChangesAsync(ct);
            return addResult.Value.Id;
        }
    }

    public sealed class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
    {
        public CreateTaskValidator()
        {
            RuleFor(x => x.TaskListId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        }
    }
}
