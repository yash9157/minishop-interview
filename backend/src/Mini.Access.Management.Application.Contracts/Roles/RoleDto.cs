namespace Mini.Access.Management.Application.Contracts;

public sealed record RoleDto(
    Guid Id, string Name, bool IsRequestable, bool IsBuiltIn, long[] PermissionIds);
