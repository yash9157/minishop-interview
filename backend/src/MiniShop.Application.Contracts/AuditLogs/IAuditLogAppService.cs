namespace MiniShop.Application.Contracts;

public interface IAuditLogAppService
{
    Task<PagedResult<AuditLogDto>> GetAsync(PagedRequest page, CancellationToken cancellationToken);
}
