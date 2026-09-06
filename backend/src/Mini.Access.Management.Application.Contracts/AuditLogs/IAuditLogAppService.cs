namespace Mini.Access.Management.Application.Contracts;

public interface IAuditLogAppService
{
    Task<PagedResult<AuditLogDto>> GetAsync(PagedRequest page);
}
