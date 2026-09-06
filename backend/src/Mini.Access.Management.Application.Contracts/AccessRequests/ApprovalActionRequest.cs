using System.ComponentModel.DataAnnotations;

namespace Mini.Access.Management.Application.Contracts;

public sealed class ApprovalActionRequest
{
    [Required, StringLength(500, MinimumLength = 3)]
    public string Remarks { get; init; } = string.Empty;
}
