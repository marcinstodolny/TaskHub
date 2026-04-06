using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskHub.Domain.Entities;

namespace TaskHub.Infrastructure.Persistence.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ConfigureBase();
            builder.ToTable("users");

            builder.Property(x => x.Username).HasMaxLength(100).IsRequired();
            builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Role).HasMaxLength(64).IsRequired();

            builder.HasIndex(x => x.Username).IsUnique();
        }
    }
}
