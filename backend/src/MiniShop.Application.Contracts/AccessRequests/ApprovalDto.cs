using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public sealed record ApprovalDto(
    long Id, int Level, Guid ApproverId, string ApproverName,
    ApprovalDecision Decision, string? Remarks, DateTime? DecisionAtUtc);
