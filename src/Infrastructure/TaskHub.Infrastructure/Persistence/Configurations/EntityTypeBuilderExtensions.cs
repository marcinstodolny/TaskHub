using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskHub.Domain.Common;

namespace TaskHub.Infrastructure.Persistence.Configurations
{
    public static class EfCoreBuilderExtensions
    {
        public static void ConfigureBase<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : Entity<Guid>
        {
            builder.HasKey(e => e.Id);
            builder.Ignore(e => e.DomainEvents);
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedAt).IsRequired(false);
        }
    }
}
