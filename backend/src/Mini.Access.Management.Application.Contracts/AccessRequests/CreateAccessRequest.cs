using System.ComponentModel.DataAnnotations;

namespace Mini.Access.Management.Application.Contracts;

public sealed class CreateAccessRequest
{
    [Range(1, long.MaxValue)]
    public long TargetSystemId { get; init; }

    public Guid RequestedRoleId { get; init; }

    [Required, StringLength(1000, MinimumLength = 10)]
    public string BusinessJustification { get; init; } = string.Empty;
}
