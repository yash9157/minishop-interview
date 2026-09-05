namespace MiniShop.Application.Contracts;

public sealed record RoleDto(Guid Id, string Name, bool IsRequestable, long[] PermissionIds);
