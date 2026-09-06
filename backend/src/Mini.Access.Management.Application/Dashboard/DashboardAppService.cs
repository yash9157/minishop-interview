using Microsoft.EntityFrameworkCore;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.Domain.Shared;
using Mini.Access.Management.EntityFrameworkCore;

namespace Mini.Access.Management.Application;

public sealed class DashboardAppService(AccessManagementDbContext db) : IDashboardAppService
{
    public async Task<DashboardDto> GetAsync()
    {
        var pending = await db.AccessRequests.CountAsync(
            x => x.Status == AccessRequestStatus.Pending);
        var requestCounts = (await db.AccessRequests.AsNoTracking()
                .GroupBy(x => x.Status)
                .Select(x => new { Status = x.Key, Count = x.Count() })
                .ToListAsync())
            .ToDictionary(x => x.Status.ToString(), x => x.Count);
        var roleCounts = (await (
                from role in db.Roles
                join userRole in db.UserRoles on role.Id equals userRole.RoleId
                group userRole by role.Name into grouped
                select new { Role = grouped.Key!, Count = grouped.Count() })
            .ToListAsync())
            .ToDictionary(x => x.Role, x => x.Count);
        var logs = await db.AuditLogs.AsNoTracking()
            .OrderByDescending(x => x.TimestampUtc).Take(10)
            .Select(x => new AuditLogDto(
                x.Id, x.User == null ? "System" : x.User.FullName,
                x.Action, x.Entity, x.EntityId, x.TimestampUtc, x.OldValue, x.NewValue))
            .ToArrayAsync();
        return new DashboardDto(pending, requestCounts, roleCounts, logs);
    }
}
