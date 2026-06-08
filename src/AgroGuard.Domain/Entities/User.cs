using AgroGuard.Domain.Common;
using AgroGuard.Domain.Enums;

namespace AgroGuard.Domain.Entities;

public sealed class User : Entity
{
    private readonly List<Farm> _farms = [];

    private User()
    {
    }

    public User(string name, string email, string passwordHash, UserRole role = UserRole.Producer)
    {
        Name = GuardRequired(name, nameof(name));
        Email = NormalizeEmail(email);
        PasswordHash = GuardRequired(passwordHash, nameof(passwordHash));
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.Producer;
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<Farm> Farms => _farms;

    public void PromoteTo(UserRole role)
    {
        Role = role;
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = GuardRequired(passwordHash, nameof(passwordHash));
    }

    public static string NormalizeEmail(string email)
    {
        var normalized = GuardRequired(email, nameof(email)).Trim().ToLowerInvariant();
        if (!normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Email must be valid.", nameof(email));
        }

        return normalized;
    }

    private static string GuardRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }
}
