using Microsoft.EntityFrameworkCore;
using MiniShop.Application.Contracts;
using MiniShop.Domain.Shared;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class DashboardAppService(MiniShopDbContext db)
{
    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken)
    {
        var pending = await db.AccessRequests.CountAsync(
            x => x.Status == AccessRequestStatus.Pending, cancellationToken);
        var requestCounts = (await db.AccessRequests.AsNoTracking()
                .GroupBy(x => x.Status)
                .Select(x => new { Status = x.Key, Count = x.Count() })
                .ToListAsync(cancellationToken))
            .ToDictionary(x => x.Status.ToString(), x => x.Count);
        var roleCounts = (await (
                from role in db.Roles
                join userRole in db.UserRoles on role.Id equals userRole.RoleId
                group userRole by role.Name into grouped
                select new { Role = grouped.Key!, Count = grouped.Count() })
            .ToListAsync(cancellationToken))
            .ToDictionary(x => x.Role, x => x.Count);
        var logs = await db.AuditLogs.AsNoTracking()
            .OrderByDescending(x => x.TimestampUtc).Take(10)
            .Select(x => new AuditLogDto(
                x.Id, x.User == null ? "System" : x.User.FullName,
                x.Action, x.Entity, x.EntityId, x.TimestampUtc, x.OldValue, x.NewValue))
            .ToArrayAsync(cancellationToken);
        return new DashboardDto(pending, requestCounts, roleCounts, logs);
    }
}
