using TaskHub.Domain.Common;

namespace TaskHub.Domain.Entities
{
    public sealed class User : Entity<Guid>
    {
        public string Username { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public string Role { get; private set; } = string.Empty;

        private User() { }

        private User(Guid id, string username, string passwordHash, string displayName, string role) : base(id)
        {
            Username = username;
            PasswordHash = passwordHash;
            DisplayName = displayName;
            Role = role;
        }

        public void Synchronize(string username, string passwordHash, string displayName, string role)
        {
            Username = username;
            PasswordHash = passwordHash;
            DisplayName = displayName;
            Role = role;
            UpdatedAt = DateTime.UtcNow;
        }

        public static Result<User> Create(Guid id, string username, string passwordHash, string displayName, string role)
        {
            return Result.Success(new User(id, username, passwordHash, displayName, role));
        }

        public static Result<User> Create(string username, string passwordHash, string displayName, string role)
        {
            return Create(Guid.NewGuid(), username, passwordHash, displayName, role);
        }
    }
}
