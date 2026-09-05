using System.ComponentModel.DataAnnotations;

namespace MiniShop.Application.Contracts;

public sealed class ApprovalActionRequest
{
    [StringLength(500)]
    public string? Remarks { get; init; }
}
