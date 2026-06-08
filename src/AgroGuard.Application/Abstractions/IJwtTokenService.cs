using AgroGuard.Domain.Entities;

namespace AgroGuard.Application.Abstractions;

public interface IJwtTokenService
{
    JwtTokenResult Generate(User user);
}

public sealed record JwtTokenResult(string Token, DateTime ExpiresAt);
