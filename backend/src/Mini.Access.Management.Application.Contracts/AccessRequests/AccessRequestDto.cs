using Mini.Access.Management.Domain.Shared;

namespace Mini.Access.Management.Application.Contracts;

public sealed record AccessRequestDto(
    long Id, Guid RequesterId, string RequesterName, long TargetSystemId,
    string TargetSystem, Guid RequestedRoleId, string RequestedRole,
    string BusinessJustification, AccessRequestStatus Status, DateTime CreatedAtUtc,
    DateTime? SubmittedAtUtc, Guid? ProvisionedById, string? ProvisionedByName,
    DateTime? ProvisionedAtUtc, ApprovalDto[] Approvals);
