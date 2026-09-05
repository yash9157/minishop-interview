using Microsoft.AspNetCore.Identity;
using MiniShop.Domain.Shared;

namespace MiniShop.Domain;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public bool IsRequestable { get; set; }
}

public sealed class Permission
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ICollection<RolePermission> Roles { get; set; } = [];
}

public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public long PermissionId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

public sealed class TargetSystem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

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

public sealed class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public ApplicationUser? User { get; set; }
}
