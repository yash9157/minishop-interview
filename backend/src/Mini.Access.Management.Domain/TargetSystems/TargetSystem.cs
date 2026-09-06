using System.ComponentModel.DataAnnotations;

namespace Mini.Access.Management.Domain;

public sealed class TargetSystem
{
    public long Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
