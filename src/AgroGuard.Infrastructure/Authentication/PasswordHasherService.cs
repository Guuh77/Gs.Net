using AgroGuard.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace AgroGuard.Infrastructure.Authentication;

public sealed class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<object> _passwordHasher = new();
    private static readonly object UserContext = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(UserContext, password);
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(UserContext, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
