using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskHub.Domain.Entities;

namespace TaskHub.Infrastructure.Persistence.Configurations
{
    public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ConfigureBase();
            builder.ToTable("task_items");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Priority).HasConversion<string>().IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().IsRequired();

            builder.Property(x => x.CreatedAt).IsRequired();

            builder.OwnsOne(x => x.Title, tb =>
            {
                tb.Property(p => p.Value).HasColumnName("title").HasMaxLength(200).IsRequired();
            });

            builder.OwnsOne(x => x.Description, db =>
            {
                db.Property(p => p.Value).HasColumnName("description").HasMaxLength(2000);
            });

            builder.HasIndex(x => new { x.TaskListId, x.Status });
        }
    }
}
