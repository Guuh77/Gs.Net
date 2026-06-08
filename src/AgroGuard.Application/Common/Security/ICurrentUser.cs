using AgroGuard.Domain.Enums;

namespace AgroGuard.Application.Common.Security;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}
