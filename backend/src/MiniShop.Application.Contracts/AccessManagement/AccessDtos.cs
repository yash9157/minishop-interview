using System.ComponentModel.DataAnnotations;
using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public sealed record RoleDto(Guid Id, string Name, bool IsRequestable, long[] PermissionIds);
public sealed record PermissionDto(long Id, string Code, string Name);
public sealed record TargetSystemDto(long Id, string Name);
public sealed record UserDto(
    Guid Id, string FullName, string Email, Guid? ManagerId, string? ManagerName,
    bool IsActive, string[] Roles);
public sealed record ApprovalDto(
    long Id, int Level, Guid ApproverId, string ApproverName,
    ApprovalDecision Decision, string? Remarks, DateTime? DecisionAtUtc);
public sealed record AccessRequestDto(
    long Id, Guid RequesterId, string RequesterName, long TargetSystemId,
    string TargetSystem, Guid RequestedRoleId, string RequestedRole,
    string BusinessJustification, AccessRequestStatus Status, DateTime CreatedAtUtc,
    ApprovalDto[] Approvals);
public sealed record AuditLogDto(
    long Id, string User, string Action, string Entity, string EntityId,
    DateTime TimestampUtc, string? OldValue, string? NewValue);
public sealed record DashboardDto(
    int PendingApprovals,
    Dictionary<string, int> RequestsByStatus,
    Dictionary<string, int> UsersByRole,
    AuditLogDto[] LatestAuditLogs);

public sealed class CreateUserRequest
{
    [Required, StringLength(120)] public string FullName { get; init; } = string.Empty;
    [Required, EmailAddress] public string Email { get; init; } = string.Empty;
    [Required, MinLength(8)] public string Password { get; init; } = string.Empty;
    public Guid? ManagerId { get; init; }
}

public sealed class UpdateUserRequest
{
    [Required, StringLength(120)] public string FullName { get; init; } = string.Empty;
    public Guid? ManagerId { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class SaveRoleRequest
{
    [Required, StringLength(80)] public string Name { get; init; } = string.Empty;
    public bool IsRequestable { get; init; }
}

public sealed class SetRolePermissionsRequest
{
    public long[] PermissionIds { get; init; } = [];
}

public sealed class SavePermissionRequest
{
    [Required, StringLength(80)] public string Code { get; init; } = string.Empty;
    [Required, StringLength(120)] public string Name { get; init; } = string.Empty;
}

public sealed class AssignRoleRequest
{
    public Guid RoleId { get; init; }
}

public sealed class CreateAccessRequest
{
    [Range(1, long.MaxValue)] public long TargetSystemId { get; init; }
    public Guid RequestedRoleId { get; init; }
    [Required, StringLength(1000, MinimumLength = 10)]
    public string BusinessJustification { get; init; } = string.Empty;
}

public sealed class ApprovalActionRequest
{
    [StringLength(500)] public string? Remarks { get; init; }
}
