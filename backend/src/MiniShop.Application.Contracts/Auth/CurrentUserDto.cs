namespace MiniShop.Application.Contracts;

public sealed record CurrentUserDto(
    Guid Id,
    string FullName,
    string Email,
    long TenantId,
    string TenantName,
    IReadOnlyList<string> Roles);
