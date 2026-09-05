using MiniShop.Domain.Shared;

namespace MiniShop.Domain;

public sealed class ApprovalHistory
{
    public long Id { get; set; }
    public long AccessRequestId { get; set; }
    public int Level { get; set; }
    public Guid ApproverId { get; set; }
    public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;
    public string? Remarks { get; set; }
    public DateTime? DecisionAtUtc { get; set; }

    public AccessRequest AccessRequest { get; set; } = null!;
    public ApplicationUser Approver { get; set; } = null!;
}
