using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Application.Features.TaskList.ReadModels;
using TaskHub.Domain.Base;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Features.TaskList.Commands
{
    public sealed record CreateTaskListCommand(string Title) : IRequest<Result<TaskListReadModel>>;

    public sealed class CreateTaskListCommandHandler(IUnitOfWork unitOfWork, ITaskListCommandRepository taskListCommandRepository) : IRequestHandler<CreateTaskListCommand, Result<TaskListReadModel>>
    {
        public async Task<Result<TaskListReadModel>> Handle(CreateTaskListCommand request, CancellationToken ct)
        {
            var titleResult = TaskListTitle.Create(request.Title);
            if (titleResult.IsFailed)
            {
                return Result.Fail<TaskListReadModel>(titleResult.Errors);
            }

            var taskList = Domain.Entities.TaskList.Create(titleResult.Value);
            if (taskList.IsFailed)
            {
                return Result.Fail<TaskListReadModel>(taskList.Errors);
            }
            await taskListCommandRepository.AddAsync(taskList.Value, ct);

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success(new TaskListReadModel() { Id = taskList.Value.Id, Title = taskList.Value.Title.Value });
        }
    }

    public sealed class CreateTaskListCommandValidator : AbstractValidator<CreateTaskListCommand>
    {
        public CreateTaskListCommandValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(TaskListTitle.MaxLength);
        }
    }
}
