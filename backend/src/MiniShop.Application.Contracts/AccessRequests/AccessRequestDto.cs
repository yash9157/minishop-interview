using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public sealed record AccessRequestDto(
    long Id, Guid RequesterId, string RequesterName, long TargetSystemId,
    string TargetSystem, Guid RequestedRoleId, string RequestedRole,
    string BusinessJustification, AccessRequestStatus Status, DateTime CreatedAtUtc,
    ApprovalDto[] Approvals);
