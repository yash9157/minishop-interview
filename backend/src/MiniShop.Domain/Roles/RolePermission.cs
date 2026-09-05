namespace MiniShop.Domain;

public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public long PermissionId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
