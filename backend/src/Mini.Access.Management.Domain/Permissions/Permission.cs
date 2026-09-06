using System.ComponentModel.DataAnnotations;

namespace Mini.Access.Management.Domain;

public sealed class Permission
{
    public long Id { get; set; }

    [MaxLength(80)]
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ICollection<RolePermission> Roles { get; set; } = [];
}
