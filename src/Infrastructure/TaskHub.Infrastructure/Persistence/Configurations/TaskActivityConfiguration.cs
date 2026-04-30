using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskHub.Domain.Entities;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Infrastructure.Persistence.Configurations
{
    public sealed class TaskActivityConfiguration : IEntityTypeConfiguration<TaskActivity>
    {
        public void Configure(EntityTypeBuilder<TaskActivity> builder)
        {
            builder.ConfigureBase();
            builder.ToTable("task_activities");

            builder.Property(x => x.TaskListId).IsRequired();
            builder.Property(x => x.TaskItemId).IsRequired(false);
            builder.Property(x => x.Type).HasConversion<string>().IsRequired();
            builder.Property(x => x.Message).HasMaxLength(TaskActivity.MaxMessageLength).IsRequired();
            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.TaskTitleSnapshot).HasMaxLength(TaskItemTitle.MaxLength).IsRequired(false);

            builder
                .HasOne<TaskList>()
                .WithMany()
                .HasForeignKey(x => x.TaskListId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.TaskListId, x.CreatedAtUtc });
            builder.HasIndex(x => x.TaskItemId);
        }
    }
}
