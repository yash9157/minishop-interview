using Microsoft.EntityFrameworkCore;
using MiniShop.Application.Contracts;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class AuditLogAppService(MiniShopDbContext db) : IAuditLogAppService
{
    public async Task<PagedResult<AuditLogDto>> GetAsync(
        PagedRequest page, CancellationToken cancellationToken)
    {
        var query = db.AuditLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(page.Search))
        {
            var search = page.Search.Trim();
            query = query.Where(x => x.Action.Contains(search) ||
                x.Entity.Contains(search) || x.EntityId.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.TimestampUtc)
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .Select(x => new AuditLogDto(
                x.Id, x.User == null ? "System" : x.User.FullName,
                x.Action, x.Entity, x.EntityId, x.TimestampUtc, x.OldValue, x.NewValue))
            .ToArrayAsync(cancellationToken);
        return new PagedResult<AuditLogDto>(items, totalCount, page.Page, page.PageSize);
    }
}
