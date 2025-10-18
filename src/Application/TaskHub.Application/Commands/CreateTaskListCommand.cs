using FluentResults;
using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Domain.Entities;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Commands
{
    public sealed record CreateTaskListCommand(string Title) : IRequest<Result<Guid>>;

    public sealed class CreateTaskListCommandHandler(IUnitOfWork unitOfWork, ITaskListCommandRepository taskListCommandRepository) : IRequestHandler<CreateTaskListCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateTaskListCommand request, CancellationToken ct)
        {
            var titleResult = Title.Create(request.Title);
            if (titleResult.IsFailed)
            {
                return Result.Fail(titleResult.Errors);
            }

            var taskList = TaskList.Create(titleResult.Value);
            if (taskList.IsFailed)
            {
                return Result.Fail(taskList.Errors);
            }
            await taskListCommandRepository.AddAsync(taskList.Value, ct);

            await unitOfWork.SaveChangesAsync(ct);
            return taskList.Value.Id;
        }
    }

    public sealed class CreateTaskListCommandValidator : AbstractValidator<CreateTaskListCommand>
    {
        public CreateTaskListCommandValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        }
    }
}
