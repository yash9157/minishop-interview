using MiniShop.Domain.Shared;

namespace MiniShop.Domain;

public sealed class AccessRequest
{
    public long Id { get; set; }
    public Guid RequesterId { get; set; }
    public long TargetSystemId { get; set; }
    public Guid RequestedRoleId { get; set; }
    public string BusinessJustification { get; set; } = string.Empty;
    public AccessRequestStatus Status { get; set; } = AccessRequestStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAtUtc { get; set; }
    public Guid? ProvisionedById { get; set; }
    public DateTime? ProvisionedAtUtc { get; set; }

    public ApplicationUser Requester { get; set; } = null!;
    public TargetSystem TargetSystem { get; set; } = null!;
    public ApplicationRole RequestedRole { get; set; } = null!;
    public ApplicationUser? ProvisionedBy { get; set; }
    public ICollection<ApprovalHistory> Approvals { get; set; } = [];
}
