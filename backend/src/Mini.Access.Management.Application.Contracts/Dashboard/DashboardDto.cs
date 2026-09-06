namespace Mini.Access.Management.Application.Contracts;

public sealed record DashboardDto(
    int PendingApprovals,
    Dictionary<string, int> RequestsByStatus,
    Dictionary<string, int> UsersByRole,
    AuditLogDto[] LatestAuditLogs);
