using MiniShop.Application.Contracts;

namespace MiniShop.Application;

public interface IAuditLogAppService
{
    Task<PagedResult<AuditLogDto>> GetAsync(PagedRequest page, CancellationToken cancellationToken);
}
