using FluentValidation;
using MediatR;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Application.Features.TaskList.ReadModels;
using TaskHub.Domain.Base;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Application.Features.TaskList.Commands
{
    public sealed record CreateTaskListCommand(string Title) : IRequest<Result<TaskListSummaryReadModel>>;

    public sealed class CreateTaskListCommandHandler(IUnitOfWork unitOfWork, ITaskListCommandRepository taskListCommandRepository) : IRequestHandler<CreateTaskListCommand, Result<TaskListSummaryReadModel>>
    {
        public async Task<Result<TaskListSummaryReadModel>> Handle(CreateTaskListCommand request, CancellationToken ct)
        {
            var titleResult = TaskListTitle.Create(request.Title);
            if (titleResult.IsFailed)
            {
                return Result.Fail<TaskListSummaryReadModel>(titleResult.Errors);
            }

            var taskList = Domain.Entities.TaskList.Create(titleResult.Value, string.Empty);
            if (taskList.IsFailed)
            {
                return Result.Fail<TaskListSummaryReadModel>(taskList.Errors);
            }
            await taskListCommandRepository.AddAsync(taskList.Value, ct);

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success(new TaskListSummaryReadModel() { Id = taskList.Value.Id, Title = taskList.Value.Title.Value });
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
