using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskHub.Domain.Entities;

namespace TaskHub.Infrastructure.Persistence.Configurations
{
    public sealed class TaskListConfiguration : IEntityTypeConfiguration<TaskList>
    {
        public void Configure(EntityTypeBuilder<TaskList> builder)
        {
            builder.ConfigureBase();
            builder.ToTable("task_lists");
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

            builder
                .HasMany(t => t.Tasks)
                .WithOne()
                .HasForeignKey(t => t.TaskListId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
