using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskHub.Domain.Entities;
using TaskHub.Domain.ValueObjects;

namespace TaskHub.Infrastructure.Persistence.Configurations
{
    public sealed class TaskListConfiguration : IEntityTypeConfiguration<TaskList>
    {
        public void Configure(EntityTypeBuilder<TaskList> builder)
        {
            builder.ConfigureBase();
            builder.ToTable("task_lists");
            builder.OwnsOne(x => x.Title, tb =>
            {
                tb.Property(p => p.Value).HasColumnName("Title").HasMaxLength(TaskListTitle.MaxLength).IsRequired();
            });

            builder
                .HasMany(t => t.Tasks)
                .WithOne()
                .HasForeignKey(t => t.TaskListId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
