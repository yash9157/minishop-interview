using Mini.Access.Management.Domain.Shared;

namespace Mini.Access.Management.Application.Contracts;

public sealed record ApprovalDto(
    long Id, int Level, Guid ApproverId, string ApproverName,
    ApprovalDecision Decision, string? Remarks, DateTime? DecisionAtUtc);
