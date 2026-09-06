namespace Mini.Access.Management.Application.Contracts;

public sealed class SetRolePermissionsRequest
{
    public long[] PermissionIds { get; init; } = [];
}
